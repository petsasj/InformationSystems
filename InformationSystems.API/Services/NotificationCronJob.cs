using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InformationSystems.API.Services
{
    public class NotificationCronJob : CronJobService
    {
        private readonly ILogger<NotificationCronJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public NotificationCronJob(IScheduleConfig<NotificationCronJob> config,
            ILogger<NotificationCronJob> logger,
            IServiceProvider serviceProvider)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification CronJob started.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.UtcNow:hh:mm:ss} Notification CronJob  is working.");

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var failedRequests = unitOfWork
                .Query<InfrastructureModificationRequest>()
                .Where(mr => mr.Approved == false || mr.HasValidationConflicts == true)
                .Where(mr => mr.Company.ReceiveConflictNotification)
                .OrderBy(o => o.DateInitialResponse)
                .GroupBy(mr => new { mr.Company.Oid, mr.Company.Vat, mr.Company.ConflictCallbackUrl })
                .Take(5);

            unitOfWork.BeginTransaction();

            foreach (var companyRequests in failedRequests)
            {
                using var httpClient = new HttpClient();

                // if provider has given a page that informs in batches
                if (!string.IsNullOrWhiteSpace(companyRequests.Key.ConflictCallbackUrl))
                {
                    var body = Newtonsoft.Json.JsonConvert.SerializeObject(
                        companyRequests.Select(mr => new { mr.InternalId, mr.RejectionReason }));

                    var content = new StringContent(body, Encoding.UTF8, "application/json");

                    var response = httpClient.PostAsync(companyRequests.Key.ConflictCallbackUrl, content).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        companyRequests.ToList().ForEach(mr =>
                        {
                            mr.ProviderConflictNotified = true;
                            mr.DateNotified = DateTime.UtcNow;
                        });
                    }
                }
                // if provider wants unique notifications for each failed request
                else
                {
                    foreach (var modificationRequest in companyRequests)
                    {
                        if (!string.IsNullOrWhiteSpace(modificationRequest.CallbackUrl))
                        {
                            var body = Newtonsoft.Json.JsonConvert.SerializeObject(new
                            { modificationRequest.InternalId, modificationRequest.RejectionReason });

                            var content = new StringContent(body, Encoding.UTF8, "application/json");

                            var response = httpClient.PostAsync(modificationRequest.CallbackUrl, content).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                modificationRequest.ProviderConflictNotified = true;
                                modificationRequest.DateNotified = DateTime.UtcNow;
                            }
                        }
                    }
                }
            }

            unitOfWork.CommitTransaction();

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification CronJob is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
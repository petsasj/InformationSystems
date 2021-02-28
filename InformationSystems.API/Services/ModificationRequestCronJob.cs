using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpo;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using InformationSystems.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace InformationSystems.API.Services
{
    public class ModificationRequestCronJob : CronJobService
    {
        private readonly ILogger<ModificationRequestCronJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ModificationRequestCronJob(IScheduleConfig<ModificationRequestCronJob> config,
            ILogger<ModificationRequestCronJob> logger,
            IServiceProvider serviceProvider)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Modification Request Cron Job started.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.UtcNow:hh:mm:ss} Modification Request Cron Job is working.");

            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var unhandledModificationRequests = unitOfWork
                .Query<InfrastructureModificationRequest>()
                .Where(imr => !imr.DateValidated.HasValue)
                .OrderBy(o => o.DateCreated)
                .Take(5)
                .ToList();

            foreach (var modificationRequest in unhandledModificationRequests)
            {
                var tdr =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<TelecommunicationDataRequest>(modificationRequest
                        .OriginalRequest);

                var geoJson = tdr.Data;
                var type = geoJson["type"]?.Value<string>();

                if (type == null)
                {
                    throw new NullReferenceException("Type of model is unknown");
                }

                GeoJSON.Net.GeoJSONObject model = type.ToLower() switch
                {
                    "geometrycollection" => geoJson.ToObject<GeometryCollection>(),
                    "linestring" => geoJson.ToObject<LineString>(),
                    "multilinestring" => geoJson.ToObject<MultiLineString>(),
                    "multipoint" => geoJson.ToObject<MultiPoint>(),
                    "multipolygon" => geoJson.ToObject<MultiPolygon>(),
                    "point" => geoJson.ToObject<Point>(),
                    "polygon" => geoJson.ToObject<Polygon>(),
                    "feature" => geoJson.ToObject<Feature>(),
                    "featurecollection" => geoJson.ToObject<FeatureCollection>(),
                    _ => throw new ArgumentOutOfRangeException()
                };


                var infrastructure = new Infrastructure(unitOfWork)
                {
                    Company = modificationRequest.Company,
                    InfrastructureType = modificationRequest.InfrastructureType,
                    GeoJSONType = model?.Type.ToString(),
                    ModificationRequest = modificationRequest,
                };

                var geoLocations = new List<GeoLocation>();

                // boxing and unboxing
                switch (model)
                {
                    case MultiPoint multiPoint:
                        geoLocations = multiPoint?.Coordinates?.Select(c => new GeoLocation(unitOfWork)
                        {
                            Altitude = c.Coordinates.Altitude,
                            Latitude = c.Coordinates.Latitude,
                            Longitude = c.Coordinates.Longitude
                        }).ToList();
                        break;
                    case Point point:
                        geoLocations.Add(new GeoLocation(unitOfWork)
                        {
                            Altitude = point.Coordinates.Altitude,
                            Latitude = point.Coordinates.Latitude,
                            Longitude = point.Coordinates.Longitude
                        });
                        break;
                    case LineString lineString:
                        geoLocations = lineString?.Coordinates?.Select(c => new GeoLocation(unitOfWork)
                        {
                            Altitude = c.Altitude,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }).ToList();
                        break;
                    case MultiLineString multiLineString:
                        geoLocations = multiLineString.Coordinates?.SelectMany(c => c.Coordinates).Select(c => new GeoLocation(unitOfWork)
                        {
                            Altitude = c.Altitude,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }).ToList();
                        break;
                    // Not handled properly
                    case Polygon polygon:
                        geoLocations = polygon.Coordinates?.SelectMany(c => c.Coordinates).Select(c => new GeoLocation(unitOfWork)
                        {
                            Altitude = c.Altitude,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }).ToList();
                        break;
                    // Not handled properly
                    case MultiPolygon multiPolygon:
                        geoLocations = multiPolygon.Coordinates?.SelectMany(c => c.Coordinates).SelectMany(c => c.Coordinates).Select(c => new GeoLocation(unitOfWork)
                        {
                            Altitude = c.Altitude,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }).ToList();
                        break;
                }

                if (geoLocations?.Any() == true)
                {
                    infrastructure.GeoLocations.AddRange(geoLocations);
                }

                if (infrastructure.GeoLocations.Any())
                {
                    modificationRequest.DateValidated = DateTime.UtcNow;
                    unitOfWork.CommitChanges();
                }
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Modification Request Cron Job is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
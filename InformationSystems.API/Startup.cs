using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using InformationSystems.API.Middleware;
using InformationSystems.API.Models;
using InformationSystems.API.Services;

namespace InformationSystems.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Sets up thread safe datalayer all units of work
            var dictionary = PrepareDictionary();
            static XPDictionary PrepareDictionary()
            {
                var dict = new ReflectionDictionary();
                dict.GetDataStoreSchema(ConnectionHelper.GetPersistentTypes());
                return dict;
            }

            var connectionString = string.Empty;

#if DEBUG
            connectionString = Configuration.GetConnectionString("DbConnectionString");
#endif
#if !DEBUG
            connectionString = Configuration.GetConnectionString("Release");
#endif

            IDataStore store = XpoDefault.GetConnectionProvider(
                XpoDefault.GetConnectionPoolString(connectionString, 5, 100),
                AutoCreateOption.DatabaseAndSchema);
            XpoDefault.DataLayer = new ThreadSafeDataLayer(dictionary, store);

            services.AddCors();
            services.AddScoped<UnitOfWork>();
            // configure strongly typed settings object
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // configure DI for application services
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            services.AddControllers().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

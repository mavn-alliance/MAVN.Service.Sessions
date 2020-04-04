using System;
using System.Text.RegularExpressions;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Logs.Loggers.LykkeSanitizing;
using Lykke.Sdk;
using MAVN.Service.Sessions.MappingProfiles;
using MAVN.Service.Sessions.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MAVN.Service.Sessions
{
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "Session Service",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Extend = (serviceCollection, settings) =>
                {
                    serviceCollection.AddAutoMapper(typeof(AutoMapperProfile));
                };

                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "LogSession";
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.SessionsService.Db.LogsConnString;

                    logs.Extended = logBuilder =>
                    {
                        logBuilder.ConfigureConsole = consoleLoggerOptions =>
                        {
                            // TODO: remove when sanitizing would be available in scopes.
                            // For now sessionToken is exposed in RequestPath if scopes are enabled.
                            consoleLoggerOptions.IncludeScopes = false;
                        };

                        // sessionToken filter.
                        logBuilder.AddSanitizingFilter(new Regex("[a-z0-9]{64}", RegexOptions.Compiled), "[SessionToken]");
                    };
                };
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IMapper mapper)
        {
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            app.UseLykkeConfiguration(options => { options.SwaggerOptions = _swaggerOptions; });
        }
    }
}
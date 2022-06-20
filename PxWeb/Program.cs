using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PxWeb.Code.Api2;
using PxWeb.Config.Api2;
using System.Collections.Generic;
using Px.Abstractions.Interfaces;
using PxWeb.Code.DataSource.Cnmm;
using PxWeb.Code.DataSource.PxFile;

namespace PxWeb
{
    public class Program
    {
        private static ILogger<Program> _logger;

        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            _logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

            // needed to load configuration from appsettings.json
            builder.Services.AddOptions();

            // needed to store rate limit counters and ip rules
            builder.Services.AddMemoryCache();

            //load general configuration from appsettings.json
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            builder.Services.AddInMemoryRateLimiting();

            // configuration (resolvers, counter key builders)
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            //TODO: Get datasource from appsetting 
            builder.Services.AddTransient<IDataSource, PxFileDataSource>();
            builder.Services.AddTransient<IItemSelectionResolver, ItemSelectionResolverCnmm>();


            // Add configuration
            builder.Services.Configure<PxApiConfigurationOptions>(builder.Configuration.GetSection("PxApiConfiguration"));
            builder.Services.AddTransient<IPxApiConfigurationService, PxApiConfigurationService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // Handle CORS configuration from appsettings.json
            bool corsEnbled = builder.Services.ConfigurePxCORS(builder, _logger);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            if (corsEnbled)
            {
                app.UseCors();
            }

            app.UseAuthorization();

            app.MapControllers();

            if (!app.Environment.IsDevelopment())
            {
                app.UseIpRateLimiting();
            }

            app.Run();
        }
    }
}
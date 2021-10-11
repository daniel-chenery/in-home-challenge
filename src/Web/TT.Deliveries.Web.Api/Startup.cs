using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using TT.Deliveries.Business;
using TT.Deliveries.Business.Providers;
using TT.Deliveries.Business.Services;
using TT.Deliveries.Core.Configuration;
using TT.Deliveries.Data;
using TT.Deliveries.Data.Repositories;

namespace TT.Deliveries.Web.Api
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TT.Deliveries.Web.Api", Version = "v1" });
                c.ResolveConflictingActions(api => api.First());
                c.DescribeAllParametersInCamelCase();

                var jwtScheme = new OpenApiSecurityScheme()
                {
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtScheme, Array.Empty<string>() }
                });
            });

            services
                .AddAuthorization(opts =>
                {
                    opts.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                    //opts.AddPolicy("User", p => p.RequireClaim(ClaimTypes.Role, "user", "User"));
                });

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(opts =>
                    {
                        var configuration = Configuration.GetSection(AuthenticationOptions.AppSettingsSection).Bind<AuthenticationOptions>();

                        opts.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = configuration.Issuer,
                            ValidateAudience = true,
                            ValidAudience = configuration.Audience,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = configuration.ToSecurityKey()
                        };

                        opts.IncludeErrorDetails = true;
                        opts.SaveToken = true;
                    });

            services.Configure<AuthenticationOptions>(Configuration.GetSection(AuthenticationOptions.AppSettingsSection));
            services.Configure<DatabaseOptions>(Configuration.GetSection(DatabaseOptions.AppSettingsSection));

            services.AddSingleton<IJwtSecurityTokenHandlerFactory, JwtSecurityTokenHandlerFactory>();
            services.AddSingleton<IConnectionProvider, SqliteConnectionProvider>();

            services.AddTransient<IAuthenticationService, SimpleAuthenticationService>();
            services.AddTransient<IDeliveryService, DeliveryService>();
            services.AddTransient<IOrderNumberService, OrderNumberService>();
            services.AddTransient(typeof(IRepository<,>), typeof(Repository<,>));

            services.AddHostedService<DeliveryExpirationService>();

            services.AddLogging();
            services.AddAutoMapper(typeof(MappingProfile));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ConfigureDatabase(app.ApplicationServices.GetRequiredService<IConnectionProvider>());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger(c => c.RouteTemplate = "/swagger/{documentName}/swagger.json");
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TT.Deliveries.Web.Api v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(e => e.MapControllers());
        }

        /// <summary>
        /// Instantiates the in-memory database
        /// It should be done via external scripts and not part of start-up
        /// True Foreign keys are not setup
        /// </summary>
        /// <param name="connectionProvider"></param>
        public void ConfigureDatabase(IConnectionProvider connectionProvider)
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());

            using var connection = connectionProvider.GetDbConnection();

            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "CREATE TABLE IF NOT EXISTS Deliverys (" +
                "Id VARCHAR(36) primary key, " +
                "State TEXT" +
                "); " +
                "CREATE TABLE IF NOT EXISTS AccessWindows(" +
                "Id VARCHAR(36) primary key, " +
                "DeliveryId VARCHAR(36), " +
                "StartTime DateTime, " +
                "EndTime DateTime" +
                "); " +
                "CREATE TABLE IF NOT EXISTS Recipients(" +
                "Id VARCHAR(36) primary key, " +
                "Name TEXT, " +
                "Address TEXT," +
                "Email TEXT," +
                "PhoneNumber TEXT" +
                "); " +
                "CREATE TABLE IF NOT EXISTS RecipientDelieverys(" +
                "Id VARCHAR(36) primary key, " +
                "DeliveryId VARCHAR(36), " +
                "RecipientId VARCHAR(36)" +
                ");" +
                "CREATE TABLE IF NOT EXISTS Orders(" +
                "Id VARCHAR primary key, " +
                "DeliveryId VARCHAR(36), " +
                "Sender TEXT" +
                ");" +
                "INSERT OR IGNORE INTO Recipients VALUES('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'Mr John Smith', '123 Sample St.', 'j.smith@example.com', '+44 123 456 789');";

            command.ExecuteNonQuery();
        }
    }
}
using System;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Mega.Has.Commons;
using Mega.Has.Instrumentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

[assembly: SupportedOSPlatform("windows7.0")]
[assembly: TargetPlatform("windows7.0")]

namespace Hopex.WebService.API
{
    public class Program
    {
        private static ModuleConfiguration _moduleConfiguration;

        public static async Task Main(string[] args)
        {
            try
            {

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // Получаем пути и конфигурируем Serilog
                string assemblyPath = Assembly.GetEntryAssembly()?.Location ?? "";
                string appFolder = Path.GetDirectoryName(assemblyPath) ?? "";
                //string loggerPath = Path.Combine(appFolder, "logs", "logfil881288e.log");
                //string loggerPath = Path.Combine("C:\\LogSerilog", "logwapmodulee.log");
                //Log.Error("start0");
                //Log.Logger = new LoggerConfiguration()
                //    .MinimumLevel.Debug()
                //    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //    .Enrich.FromLogContext()
                //    .WriteTo.File(loggerPath, rollingInterval: RollingInterval.Day)
                //    .CreateLogger();
                _moduleConfiguration = await ModuleConfiguration.CreateAsync(args, null);
                await Task.Delay(15000);
                var builder = WebApplication.CreateBuilder(args);

                // Настройка Kestrel
                builder.WebHost
                    .UseHASInstrumentation(_moduleConfiguration)
                    .UseUrls(_moduleConfiguration.ServerInstanceUrl)
                    .UseContentRoot(_moduleConfiguration.Folder)
                    .UseKestrel(options =>
                    {
                        options.AddServerHeader = false;
                    });

                // Настройка сервисов
                ConfigureServices(builder.Services);

                var app = builder.Build();

                // Настройка middleware
                ConfigureMiddleware(app, _moduleConfiguration);

                await app.RunAsync();
            }
            catch (Exception ex)
            {
               // Log.Error("UAS - " + ex.Message);
                PreloadLogger.LogError("UAS - " + ex.Message);
                Log.CloseAndFlush();
                throw;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IFileSystem>(new FileSystem());

            services.AddHASModule(options =>
            {
                options.AuthenticationMode = AuthenticationMode.HopexSession;
            });

            services.AddControllersWithViews()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization()
                    .AddNewtonsoftJson();

            //services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddScoped<IHeaderCollector, HeaderCollector>();
        }

        private static void ConfigureMiddleware(WebApplication app, IModuleConfiguration moduleConfiguration)
        {
            // Получаем сервис трассировки
            var traceInstrumentation = app.Services.GetRequiredService<ITraceInstrumentation>();

            app.UseSwagger();
            app.UseSwaggerUI();
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/nncall/swagger/v1/swagger.json", "My API V1");
            //    c.RoutePrefix = "swagger"; // Swagger будет доступен по /nncall/swagger
            //});
            app.UseHASModule(moduleConfiguration, traceInstrumentation);

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
        }
    }
}
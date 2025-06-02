////using Mega.Has.Commons;
////using Mega.Has.Instrumentation;
////using System.IO.Abstractions;

////internal class Program
////{
////    private static ModuleConfiguration _moduleConfiguration;

////    private static void Main(string[] args)
////    {
////        try
////        {
////            ModuleConfiguration moduleConfiguration = ModuleConfiguration.CreateAsync(args, null).GetAwaiter().GetResult();
////            Program._moduleConfiguration = moduleConfiguration;
////            //Program.CreateHostBuilder(args).Build().RunAsync(default(CancellationToken));
////        }
////        catch (Exception ex)
////        {
////            PreloadLogger.LogError("UAS -" + ex.Message);
////          //  Log.CloseAndFlush();
////            throw;
////        }






////        var builder = WebApplication.CreateBuilder(args);
////        var services = builder.Services;
////        // Add services to the container
////        builder.Services.AddControllers();
////        builder.Services.AddEndpointsApiExplorer();
////        builder.Services.AddSwaggerGen();
////       // builder.Services.AddDataAnnotationsLocalization()();

////        services.AddSingleton(new FileSystem());
////        services.AddHASModule(delegate (HASSecurityOptions options)
////        {
////            options.AuthenticationMode = AuthenticationMode.HopexSession;
////        });
////        //IMvcBuilder mvcBuilder = services
////        //    .AddControllersWithViews()
////        //    .AddViewLocalization()
////        //    .AddDataAnnotationsLocalization();
////        //    //.AddNewtonsoftJson();



////        var app = builder.Build();

////        // Configure the HTTP request pipeline
////        if (app.Environment.IsDevelopment())
////        {
////            app.UseSwagger();
////            app.UseSwaggerUI();
////        }
////        app.UseHASModule(moduleConfiguration, traceInstrumentation);
////        app.UseHttpsRedirection();
////        app.UseAuthorization();

////        // Настройка маршрутизации
////        app.MapControllers();

////        // Добавление специфических маршрутов
////        app.MapControllerRoute(
////            name: "DefaultApi",
////            pattern: "api/{controller}/{id?}");

////        app.MapControllerRoute(
////            name: "DefaultApi1",
////            pattern: "api1",
////            defaults: new { controller = "MainQuery", action = "PostExecAll10" });

////        app.MapControllerRoute(
////            name: "DefaultApiTest",
////            pattern: "api_test",
////            defaults: new { controller = "MainQuery", action = "PostExecAll10" });

////        app.MapControllerRoute(
////            name: "DefaultApi0",
////            pattern: "api0",
////            defaults: new { controller = "MainQuery", action = "userInfo0" });

////        app.Run();
////    }
////}
////=======================\
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Mega.Has.Commons;
//using Mega.Has.Instrumentation;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Server.Kestrel.Core;
//using Microsoft.Extensions.Hosting;
//using Serilog;
//using System;
//using System.IO.Abstractions;
//using Mega.Has.Commons;
//using Mega.Has.Instrumentation;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Routing;
//using Microsoft.Extensions.DependencyInjection;
//using Serilog.Events;
//using System.Reflection;
//using System.Text;

//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Server.Kestrel.Core;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Serilog;
//using Serilog.Events;
//using System.Reflection;
//using System.Text;
//using Mega.Has.WebSite;
////susing Hopex.WebService.APICustom; // Предположим, что ваши расширения и классы тут

//Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

//var builder = WebApplication.CreateBuilder(args);

//// Получаем пути и конфигурируем Serilog
//string assemblyPath = Assembly.GetEntryAssembly()?.Location ?? "";
//string appFolder = Path.GetDirectoryName(assemblyPath) ?? "";
////string loggerPath = Path.Combine(appFolder, "logs", "logfil881288e.log");
//string loggerPath = Path.Combine("C:\\LogSerilog", "logwapmodulee.log");

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Debug()
//    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
//    .Enrich.FromLogContext()
//    .WriteTo.File(loggerPath, rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Host.UseSerilog();

//try
//{
//    Log.Error("step1");
//    await Task.Delay(15000);
//    // Загружаем конфигурацию модуля
//    var moduleConfig = await ModuleConfiguration.CreateAsync(args, null);

//    // Настройка Kestrel и URL
//    builder.WebHost
//        .UseContentRoot(moduleConfig.Folder)
//        .UseUrls(moduleConfig.ServerInstanceUrl)
//        .ConfigureKestrel(options =>
//        {
//            options.AddServerHeader = false;
//        });

//    // Добавляем сервисы
//    builder.Services.AddControllers();
//    builder.Services.AddEndpointsApiExplorer();
//    builder.Services.AddSwaggerGen();
//    builder.Services.AddSingleton(new FileSystem());
//   // builder.Services.AddSingleton(HopexInstrumentation, ITraceInstrumentation);
//    builder.Services.AddHostedService<ModuleHostedService>();
//    builder.Services.AddHASModule(options =>
//    {
//        options.AuthenticationMode = AuthenticationMode.HopexSession;
//    });
//    builder.Services.AddControllersWithViews()
//        .AddViewLocalization()
//        .AddDataAnnotationsLocalization()
//        .AddNewtonsoftJson();

//    Log.Error("step5321");

//    var app = builder.Build();

//    // Middleware pipeline
//    app.UseSwagger();
//    app.UseSwaggerUI();
//    try
//    {
//        app.UseHASModule(moduleConfig, app.Services.GetRequiredService<ITraceInstrumentation>());

//    }
//    catch (Exception ex)
//    {
//        Log.Error("ITraceInstrumentation : " + ex.Message);
//        throw;
//    }

//    app.UseStaticFiles();
//    app.UseRouting();
//    app.UseAuthentication();
//    app.UseAuthorization();

//    app.MapControllerRoute(
//        name: "default",
//        pattern: "{controller=Home}/{action=Index}/{id?}");

//    Log.Error("ster333p1");

//    await app.RunAsync();
//    Log.Error("step10");
//}
//catch (Exception ex)
//{
//    Log.Error("UAS - " + ex.Message);
//    PreloadLogger.LogError("UAS - " + ex.Message);
//}
//finally
//{
//    Log.CloseAndFlush();
//}
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
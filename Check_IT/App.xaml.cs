using Check_IT.Data;
using Check_IT.Interfaces;
using Check_IT.Services;
using Check_IT.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Serilog;

namespace Check_IT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }
        private IServiceScope? _mainScope;

        static App()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
                Log.CloseAndFlush();
            };
        }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddEnvironmentVariables(prefix: "CHECKIT_");
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddScoped<MainWindow>();
                    services.AddScoped<RegisterWindow>();
                    services.AddScoped<LoginWindow>();

                    services.AddScoped<LoginViewModel>();
                    services.AddScoped<RegisterViewModel>();
                    services.AddScoped<HomeViewModel>();

                    var builder = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables(prefix: "CHECKIT_");

                    var configuration = builder.Build();
                    var connStr = configuration.GetConnectionString("DefaultConnection");

                    // EF DbContext
                    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connStr));

                    // Register UserService as interface
                    services.AddScoped<IUserService, UserService>();
                    // Register AuthService
                    services.AddSingleton<IAuthService, AuthService>();

                    // Logging
                    services.AddAppLogging();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Log.Information("Application starting");
            await AppHost.StartAsync();

            try
            {
                using var initScope = AppHost.Services.CreateScope();
                var db = initScope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database ensure/create failed");
                MessageBox.Show($"Database ensure/create failed: {ex.Message}", "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _mainScope = AppHost.Services.CreateScope();

            var mainWindow = _mainScope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            Log.Information("Application exiting");
            await AppHost.StopAsync();

            _mainScope?.Dispose();

            AppHost.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}

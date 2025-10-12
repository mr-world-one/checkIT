using Check_IT.Data;
using Check_IT.Models;
using Check_IT.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace Check_IT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }
        private IServiceScope? _mainScope;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // MainWindow and dialog windows are scoped so they get scoped services properly
                    services.AddScoped<MainWindow>();
                    services.AddScoped<RegisterWindow>();
                    services.AddScoped<LoginWindow>();
                    ConfigureServices(services);
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            // Create a scope for the main window and keep it for the app lifetime
            _mainScope = AppHost.Services.CreateScope();

            var mainWindow = _mainScope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();

            // Dispose the scope that holds MainWindow and scoped services
            _mainScope?.Dispose();

            AppHost.Dispose();
            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<UserService>();
        }
    }
}

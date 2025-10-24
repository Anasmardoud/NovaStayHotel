using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NovaStayHotel.Views;
using System.Windows;

namespace NovaStayHotel;

/// <summary>
/// Application entry point with dependency injection setup
/// </summary>
public partial class App : Application
{
    private readonly IHost host;

    public App()
    {
        host = Host.CreateDefaultBuilder()
            .AddNovaStayHotelServices()
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await host.StartAsync();

        // Initialize navigation
        var navigationStore = host.Services.GetRequiredService<NavigationStore>();
        var homeNavigationService = host.Services.GetRequiredService<NavigationService<HomeViewModel>>();
        homeNavigationService.Navigate();
        MainWindow = new MainWindow
        {
            DataContext = host.Services.GetRequiredService<MainViewModel>()
        };
        MainWindow.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (MainWindow?.DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        using (host)
        {
            await host.StopAsync();
        }

        base.OnExit(e);
    }

    /// <summary>
    /// Helper method to get services from the DI container
    /// </summary>
    public static T? GetService<T>() where T : class
    {
        var app = Current as App;
        return app?.host.Services.GetService<T>();
    }

    /// <summary>
    /// Helper method to get required services from the DI container
    /// </summary>
    public static T? GetRequiredService<T>() where T : class
    {
        var app = Current as App;
        return app?.host.Services.GetRequiredService<T>();
    }
}
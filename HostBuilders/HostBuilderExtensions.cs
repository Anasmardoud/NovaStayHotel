using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NovaStayHotel.Views;

namespace NovaStayHotel;

public static class HostBuilderExtensions
{
    /// <summary>
    /// Registers all application services (DB, business, navigation, viewModels, views).
    /// </summary>
    public static IHostBuilder AddNovaStayHotelServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, services) =>
        {
            AddDatabaseServices(services, context.Configuration);
            AddBusinessServices(services);
            AddNavigationServices(services);
            AddViewModels(services);
            AddViews(services);
        });
    }

    #region Database
    static void AddDatabaseServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<NovaStayHotelDataContext>(options =>
            options.UseSqlServer(connectionString));
    }
    #endregion

    #region Business Services
    static void AddBusinessServices(IServiceCollection services)
    {
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IReservationService, ReservationService>();
    }
    #endregion

    #region Navigation
    static void AddNavigationServices(IServiceCollection services)
    {
        services.AddSingleton<NavigationStore>();
        services.AddTransient(serviceProvider =>
            new NavigationService<HomeViewModel>(
                serviceProvider.GetRequiredService<NavigationStore>(),
                () => serviceProvider.GetRequiredService<HomeViewModel>()));

        services.AddTransient(serviceProvider =>
            new NavigationService<GuestListViewModel>(
                serviceProvider.GetRequiredService<NavigationStore>(),
                () => serviceProvider.GetRequiredService<GuestListViewModel>()));

        services.AddTransient(serviceProvider =>
            new NavigationService<ReservationListViewModel>(
                serviceProvider.GetRequiredService<NavigationStore>(),
                () => serviceProvider.GetRequiredService<ReservationListViewModel>()));

        services.AddTransient(serviceProvider =>
            new NavigationService<RoomListViewModel>(
                serviceProvider.GetRequiredService<NavigationStore>(),
                () => serviceProvider.GetRequiredService<RoomListViewModel>()));
    }
    #endregion

    #region ViewModels
    static void AddViewModels(IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<GuestListViewModel>();
        services.AddTransient<ReservationListViewModel>();
        services.AddTransient<RoomListViewModel>();
        services.AddTransient<AddEditRoomViewModel>();
        services.AddTransient<AddEditGuestViewModel>();
        services.AddTransient<AddEditReservationViewModel>();

        services.AddTransient<Func<AddEditRoomViewModel>>(provider =>
            () => new AddEditRoomViewModel(provider.GetRequiredService<IRoomService>()));

        services.AddTransient<Func<RoomDetail, AddEditRoomViewModel>>(provider =>
            roomDetail => new AddEditRoomViewModel(provider.GetRequiredService<IRoomService>(), roomDetail));

        services.AddTransient<Func<AddEditGuestViewModel>>(provider =>
            () => new AddEditGuestViewModel(provider.GetRequiredService<IGuestService>()));

        services.AddTransient<Func<GuestDetail, AddEditGuestViewModel>>(provider =>
            guestDetail => new AddEditGuestViewModel(provider.GetRequiredService<IGuestService>(), guestDetail));

        services.AddTransient<Func<AddEditReservationViewModel>>(provider =>
            () => new AddEditReservationViewModel(
                provider.GetRequiredService<IReservationService>(),
                provider.GetRequiredService<IGuestService>(),
                provider.GetRequiredService<IRoomService>()));

        services.AddTransient<Func<ReservationDetail, AddEditReservationViewModel>>(provider =>
            reservationDetail => new AddEditReservationViewModel(
                provider.GetRequiredService<IReservationService>(),
                provider.GetRequiredService<IGuestService>(),
                provider.GetRequiredService<IRoomService>(),
                reservationDetail));
    }
    #endregion

    #region Views
    static void AddViews(IServiceCollection services)
    {
        services.AddTransient<MainWindow>();
        services.AddTransient<RoomListView>();
        services.AddTransient<AddEditRoomWindow>();
        services.AddTransient<GuestListView>();
        services.AddTransient<AddEditGuestWindow>();
        services.AddTransient<ReservationListView>();
        services.AddTransient<AddEditReservationWindow>();
    }
    #endregion
}
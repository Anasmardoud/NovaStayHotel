using System.Windows.Input;

namespace NovaStayHotel;

public class HomeViewModel(
    NavigationService<GuestListViewModel> guestListNavService,
    NavigationService<ReservationListViewModel> reservationListNavService,
    NavigationService<RoomListViewModel> roomListNavService) : BaseViewModel
{
    public ICommand NavigateToGuestListCommand { get; } = new NavigateCommand<GuestListViewModel>(guestListNavService);
    public ICommand NavigateToReservationListCommand { get; } = new NavigateCommand<ReservationListViewModel>(reservationListNavService);
    public ICommand NavigateToRoomListCommand { get; } = new NavigateCommand<RoomListViewModel>(roomListNavService);
}
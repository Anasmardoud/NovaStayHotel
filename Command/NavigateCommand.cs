namespace NovaStayHotel;

public class NavigateCommand<TViewModel>(NavigationService<TViewModel> navigationService) : CommandBase
    where TViewModel : BaseViewModel
{
    private readonly NavigationService<TViewModel> _navigationService = navigationService;

    public override void Execute(object? parameter)
    {
        _navigationService.Navigate();
    }
}

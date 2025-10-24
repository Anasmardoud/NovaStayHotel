namespace NovaStayHotel;
public class NavigationStore
{
    public event Action? CurrentViewModelChanged;
    private BaseViewModel? currentViewModel;
    public BaseViewModel? CurrentViewModel
    {
        get => currentViewModel;
        set
        {
            currentViewModel = value;
            OnCurrentViewModelChanged();
        }
    }
    private void OnCurrentViewModelChanged()
    {
        CurrentViewModelChanged?.Invoke();
    }
}

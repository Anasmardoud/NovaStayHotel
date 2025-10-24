namespace NovaStayHotel;

internal class MainViewModel : BaseViewModel, IDisposable
{
    readonly NavigationStore navigationStore;
    bool disposed;

    public MainViewModel(NavigationStore navigationStore)
    {
        this.navigationStore = navigationStore ?? throw new ArgumentNullException(nameof(navigationStore));
        this.navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
    }
    public BaseViewModel? CurrentViewModel => navigationStore.CurrentViewModel;

    void OnCurrentViewModelChanged() => OnPropertyChanged(nameof(CurrentViewModel));
    public void Dispose()
    {
        if (!disposed)
        {
            navigationStore.CurrentViewModelChanged -= OnCurrentViewModelChanged;
            disposed = true;
        }
    }
}
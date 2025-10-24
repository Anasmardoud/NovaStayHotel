namespace NovaStayHotel;

public abstract class AsyncCommandBase : CommandBase
{
    bool isExecuting;
    bool IsExecuting
    {
        get => isExecuting;
        set
        {
            isExecuting = value;
            OnCanExecuteChanged();
        }
    }
    public override bool CanExecute(object? parameter) => !IsExecuting && base.CanExecute(parameter);
    public override async void Execute(object? parameter)
    {
        IsExecuting = true;
        try
        {
            await ExecuteAsync(parameter);
        }
        finally
        {
            IsExecuting = false;
        }
    }
    public abstract Task ExecuteAsync(object? parameter);
}

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NovaStayHotel;

/// <summary>
/// Base class for Add/Edit ViewModels providing common validation and save functionality
/// </summary>
/// <typeparam name="TEntity">The entity type (e.g., RoomDetail, GuestDetail)</typeparam>
/// <typeparam name="TService">The service interface type (e.g., IRoomService, IGuestService)</typeparam>
public abstract class BaseAddEditViewModel<TEntity, TService> : BaseViewModel
{
    #region Construction
    protected readonly TService service;
    protected readonly bool isEditMode;
    protected readonly long originalEntityId;
    protected readonly Dictionary<string, List<string>> propertyErrors = [];

    protected BaseAddEditViewModel(TService service)
    {
        this.service = service;
        isEditMode = false;
        originalEntityId = 0;
        SaveCommand = new SaveEntityDialogCommand<TEntity, TService>(this);
        CancelCommand = new CancelDialogCommand(this);
        SetDefaultsWithoutValidation();
        ValidateAll();
        UpdateCanSave();
    }

    protected BaseAddEditViewModel(TService service, TEntity entityToEdit)
    {
        this.service = service;
        isEditMode = true;
        originalEntityId = GetEntityId(entityToEdit);
        SaveCommand = new SaveEntityDialogCommand<TEntity, TService>(this);
        CancelCommand = new CancelDialogCommand(this);
        LoadExistingDataWithoutValidation(entityToEdit);
        ValidateAll();
        UpdateCanSave();
    }
    #endregion

    #region Abstract Methods
    protected abstract void SetDefaultsWithoutValidation();
    protected abstract void LoadExistingDataWithoutValidation(TEntity entity);
    protected abstract void ValidateProperty(object? value, string propertyName);
    protected abstract void ValidateAll();
    protected abstract Task<TEntity> CreateEntityAsync();
    protected abstract Task UpdateEntityAsync(TEntity entity);
    protected abstract Task AddEntityAsync(TEntity entity);
    protected abstract long GetEntityId(TEntity entity);
    protected abstract string EntityName { get; }
    #endregion

    #region Common Properties
    public virtual string WindowTitle => isEditMode ? $"Edit {EntityName}" : $"Add New {EntityName}";
    public virtual string WindowSubtitle => isEditMode ? $"Update {EntityName.ToLower()} information" : $"Enter {EntityName.ToLower()} details";
    public virtual string SaveButtonText => isEditMode ? "Update" : "Create";
    public virtual string LoadingText => isEditMode ? $"Updating {EntityName.ToLower()}..." : $"Creating {EntityName.ToLower()}...";

    bool isSaving;
    public bool IsSaving
    {
        get => isSaving;
        set
        {
            if (isSaving == value) return;
            isSaving = value;
            OnPropertyChanged();
            UpdateCanSave();
        }
    }

    bool canSave;
    public bool CanSave
    {
        get => canSave;
        set
        {
            if (canSave == value) return;
            canSave = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> ValidationErrors { get; } = [];
    public bool HasValidationErrors => ValidationErrors.Count > 0;
    #endregion

    #region Commands
    public AsyncCommandBase SaveCommand { get; }
    public ICommand CancelCommand { get; }
    #endregion

    #region Validation Helper Methods
    protected void ClearPropertyErrors(string propertyName)
    {
        if (propertyErrors.ContainsKey(propertyName))
        {
            propertyErrors.Remove(propertyName);
        }
    }

    protected void AddPropertyErrors(string propertyName, List<string> errors)
    {
        if (errors.Count > 0)
        {
            propertyErrors[propertyName] = errors;
        }
        UpdateValidationErrorsCollection();
    }

    protected void UpdateValidationErrorsCollection()
    {
        ValidationErrors.Clear();

        foreach (var propertyErrorList in propertyErrors.Values)
        {
            foreach (var error in propertyErrorList)
            {
                ValidationErrors.Add(error);
            }
        }

        OnPropertyChanged(nameof(HasValidationErrors));
        OnPropertyChanged(nameof(ValidationErrors));
    }

    protected void UpdateCanSave()
    {
        CanSave = !IsSaving && !HasValidationErrors;
    }

    protected void ValidatePropertyBase(object? value, string propertyName, Action<List<string>> validator)
    {
        ClearPropertyErrors(propertyName);
        var errors = new List<string>();

        try
        {
            validator(errors);
        }
        catch (Exception ex)
        {
            errors.Add($"Validation error in {propertyName}: {ex.Message}");
        }

        AddPropertyErrors(propertyName, errors);
        UpdateCanSave();
    }
    #endregion

    #region Save Method
    public async Task<bool> SaveAsync()
    {
        try
        {
            IsSaving = true;
            ValidateAll();

            if (HasValidationErrors)
                return false;

            var entity = await CreateEntityAsync();

            if (isEditMode)
                await UpdateEntityAsync(entity);
            else
                await AddEntityAsync(entity);

            return true;
        }
        catch (Exception ex)
        {
            if (!propertyErrors.ContainsKey("General"))
                propertyErrors["General"] = [];
            propertyErrors["General"].Add($"Error saving {EntityName.ToLower()}: {ex.Message}");
            UpdateValidationErrorsCollection();
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }
    #endregion
}

#region Command Implementations
public class SaveEntityDialogCommand<TEntity, TService> : AsyncCommandBase
{
    readonly BaseAddEditViewModel<TEntity, TService> viewModel;
    public SaveEntityDialogCommand(BaseAddEditViewModel<TEntity, TService> viewModel) => this.viewModel = viewModel;
    public override bool CanExecute(object? parameter) =>
        base.CanExecute(parameter) && viewModel.CanSave;
    public override async Task ExecuteAsync(object? parameter)
    {
        if (await viewModel.SaveAsync())
        {
            if (parameter is System.Windows.Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}
public class CancelDialogCommand : ICommand
{
    readonly BaseViewModel viewModel;
    public CancelDialogCommand(BaseViewModel viewModel) => this.viewModel = viewModel;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        if (parameter is System.Windows.Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}

#endregion
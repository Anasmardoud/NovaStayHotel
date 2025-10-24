using System.Windows;
using System.Windows.Controls;

namespace NovaStayHotel.Views;

public partial class GuestListView : UserControl
{
    public GuestListView()
    {
        InitializeComponent();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is GuestDetailViewModel guest)
        {
            // Set the selected guest and execute the edit command
            if (this.DataContext is GuestListViewModel viewModel)
            {
                viewModel.SelectedGuest = guest;
                if (viewModel.EditGuestCommand.CanExecute(null))
                    viewModel.EditGuestCommand.Execute(null);
            }
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is GuestDetailViewModel guest)
        {
            // Set the selected guest and execute the delete command
            if (this.DataContext is GuestListViewModel viewModel)
            {
                viewModel.SelectedGuest = guest;
                if (viewModel.DeleteGuestCommand.CanExecute(null))
                    viewModel.DeleteGuestCommand.Execute(null);
            }
        }
    }
}
using System.Windows;
using System.Windows.Controls;

namespace NovaStayHotel.Views
{
    public partial class RoomListView : UserControl
    {
        public RoomListView()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RoomDetailViewModel room)
            {
                if (DataContext is RoomListViewModel viewModel)
                {
                    viewModel.SelectedRoom = room;
                    if (viewModel.EditRoomCommand.CanExecute(null))
                    {
                        viewModel.EditRoomCommand.Execute(null);
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RoomDetailViewModel room)
            {
                if (DataContext is RoomListViewModel viewModel)
                {
                    viewModel.SelectedRoom = room;
                    if (viewModel.DeleteRoomCommand.CanExecute(null))
                    {
                        viewModel.DeleteRoomCommand.Execute(null);
                    }
                }
            }
        }
    }
}
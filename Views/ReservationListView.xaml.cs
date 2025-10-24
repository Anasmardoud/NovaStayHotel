
using System.Windows;
using System.Windows.Controls;

namespace NovaStayHotel.Views
{
    public partial class ReservationListView : UserControl
    {
        public ReservationListView()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ReservationDetailViewModel reservation)
            {
                var viewModel = DataContext as ReservationListViewModel;
                viewModel.SelectedReservation = reservation;
                viewModel.EditReservationCommand.Execute(null);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ReservationDetailViewModel reservation)
            {
                var viewModel = DataContext as ReservationListViewModel;
                viewModel.SelectedReservation = reservation;
                viewModel.DeleteReservationCommand.Execute(null);
            }
        }

        private void CheckInButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ReservationDetailViewModel reservation)
            {
                var viewModel = DataContext as ReservationListViewModel;
                viewModel.SelectedReservation = reservation;
                viewModel.CheckInCommand.Execute(null);
            }
        }

        private void CheckOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ReservationDetailViewModel reservation)
            {
                var viewModel = DataContext as ReservationListViewModel;
                viewModel.SelectedReservation = reservation;
                viewModel.CheckOutCommand.Execute(null);
            }
        }
    }
}
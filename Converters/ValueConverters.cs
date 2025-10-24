using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NovaStayHotel.Converters
{

    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? "Yes" : "No";
            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString()?.ToLower() == "yes";
        }
    }

    public class EditButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditing)
                return isEditing ? "Save" : "Edit";
            return "Edit";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EditButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditing)
                return new SolidColorBrush(isEditing ? Colors.Green : Colors.Orange);
            return new SolidColorBrush(Colors.Orange);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RoomStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RoomStatus status)
            {
                return status switch
                {
                    RoomStatus.Available => new SolidColorBrush(Colors.Green),
                    RoomStatus.Occupied => new SolidColorBrush(Colors.Red),
                    RoomStatus.Reserved => new SolidColorBrush(Colors.Orange),
                    RoomStatus.UnderMaintenance => new SolidColorBrush(Colors.Yellow),
                    RoomStatus.OutOfService => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PriceToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal price)
                return $"${price:F2}";
            return "$0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value?.ToString()?.Replace("$", "");
            if (decimal.TryParse(stringValue, out decimal result))
                return result;
            return 0m;
        }
    }

    public class DateTimeToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
                return dateTime.ToString("yyyy-MM-dd");
            return "Never";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value?.ToString();
            if (DateTime.TryParse(stringValue, out DateTime result))
                return result;
            return (DateTime?)null;
        }
    }

    public class NullableBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? "With Balcony" : "Without Balcony";
            return "All";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "With Balcony" => true,
                "Without Balcony" => false,
                _ => (bool?)null
            };
        }
    }

    public class EnumToDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                // Convert PascalCase to readable format
                var name = enumValue.ToString();
                return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }



    public class BoolToGenderIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMale)
                return isMale ? 0 : 1; // 0 = Male, 1 = Female
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
                return index == 0; // 0 = Male (true), 1 = Female (false)
            return true;
        }
    }

    public class NationalityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Nationality nationality)
                return nationality.ToString();
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && Enum.TryParse<Nationality>(stringValue, out var nationality))
                return nationality;
            return Nationality.Lebanon;
        }
    }

    public class DateOnlyToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateOnly dateOnly)
                return dateOnly.ToDateTime(TimeOnly.MinValue);
            return DateTime.Today;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
                return DateOnly.FromDateTime(dateTime);
            return DateOnly.FromDateTime(DateTime.Today);
        }
    }

    public class AgeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int age)
                return $"{age} years old";
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GenderToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMale)
            {
                return isMale
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue)
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightPink);
            }
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
                return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Collapsed : Visibility.Visible;
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReservationStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReservationStatus status)
            {
                return status switch
                {
                    ReservationStatus.Created => new SolidColorBrush(Colors.Blue),
                    ReservationStatus.CheckedIn => new SolidColorBrush(Colors.Green),
                    ReservationStatus.CheckedOut => new SolidColorBrush(Colors.Gray),
                    ReservationStatus.Canceled => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PaymentMethodToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PaymentMethod method)
            {
                return method switch
                {
                    PaymentMethod.EWallet => "E-Wallet",
                    PaymentMethod.BankTransfer => "Bank Transfer",
                    PaymentMethod.CryptoCurrency => "Cryptocurrency",
                    _ => method.ToString()
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue switch
                {
                    "E-Wallet" => PaymentMethod.EWallet,
                    "Bank Transfer" => PaymentMethod.BankTransfer,
                    "Cryptocurrency" => PaymentMethod.CryptoCurrency,
                    "Cash" => PaymentMethod.Cash,
                    "Voucher" => PaymentMethod.Voucher,
                    "Other" => PaymentMethod.Other,
                    _ => PaymentMethod.Cash
                };
            }
            return PaymentMethod.Cash;
        }
    }

    public class ReservationStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ReservationStatus status)
            {
                return status switch
                {
                    ReservationStatus.CheckedIn => "Checked In",
                    ReservationStatus.CheckedOut => "Checked Out",
                    _ => status.ToString()
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue switch
                {
                    "Checked In" => ReservationStatus.CheckedIn,
                    "Checked Out" => ReservationStatus.CheckedOut,
                    "Created" => ReservationStatus.Created,
                    "Canceled" => ReservationStatus.Canceled,
                    _ => ReservationStatus.Created
                };
            }
            return ReservationStatus.Created;
        }
    }
}
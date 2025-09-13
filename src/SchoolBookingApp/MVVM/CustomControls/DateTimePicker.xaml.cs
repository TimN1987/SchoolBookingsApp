using System.Windows;
using System.Windows.Controls;

namespace SchoolBookingApp.MVVM.CustomControls
{
    /// <summary>
    /// Interaction logic for DateTimePicker.xaml
    /// </summary>
    public partial class DateTimePicker : UserControl
    {
        public DateTimePicker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DateProperty = 
            DependencyProperty.Register(
                nameof(Date), 
                typeof(DateTime), 
                typeof(DateTimePicker), 
                new PropertyMetadata(DateTime.Now, OnPartChanged));

        public DateTime Date
        {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public static readonly DependencyProperty HoursProperty = 
            DependencyProperty.Register(
                nameof(Hours), 
                typeof(string), 
                typeof(DateTimePicker), 
                new PropertyMetadata("07", OnPartChanged));

        public string Hours
        {
            get => (string)GetValue(HoursProperty);
            set => SetValue(HoursProperty, value);
        }

        public static readonly DependencyProperty MinutesProperty = 
            DependencyProperty.Register(
                nameof(Minutes), 
                typeof(string), 
                typeof(DateTimePicker), 
                new PropertyMetadata("00", OnPartChanged));

        public string Minutes
        {
            get => (string)GetValue(MinutesProperty);
            set => SetValue(MinutesProperty, value);
        }

        public static readonly DependencyProperty BookingDateTimeProperty = 
            DependencyProperty.Register(
                nameof(BookingDateTime), 
                typeof(DateTime), 
                typeof(DateTimePicker), 
                new FrameworkPropertyMetadata(
                    DateTime.Now, 
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                    OnDateTimeChanged));

        public DateTime BookingDateTime
        {
            get => (DateTime)(GetValue(BookingDateTimeProperty));
            set => SetValue(BookingDateTimeProperty, value);
        }

        /// <summary>
        /// Updates the UI elements date, hours and minutes when the <see cref="BookingDateTime"/> property is changed.
        /// </summary>
        private static void OnDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DateTimePicker picker && e.NewValue is DateTime dateTime)
            {
                picker.Date = dateTime.Date;
                picker.Hours = dateTime.Hour.ToString("D2");
                picker.Minutes = (dateTime.Minute - (dateTime.Minute % 5)).ToString("D2");
            }
        }

        /// <summary>
        /// Updates the <see cref="BookingDateTime"/> property when a UI element is changed.
        /// </summary>
        private static void OnPartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DateTimePicker picker)
            {
                if (!int.TryParse(picker.Hours, out int hours))
                    return;
                if (!int.TryParse(picker.Minutes, out int minutes))
                    return;
                
                picker.SetCurrentValue(
                    BookingDateTimeProperty, 
                    new DateTime(
                        picker.Date.Year,
                        picker.Date.Month, 
                        picker.Date.Day, 
                        hours, 
                        minutes, 
                        0));
            }
        }
    }
}

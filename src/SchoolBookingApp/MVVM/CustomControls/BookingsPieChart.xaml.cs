using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SchoolBookingApp.MVVM.CustomControls
{
    /// <summary>
    /// Interaction logic for BookingsPieChart.xaml
    /// </summary>
    public partial class BookingsPieChart : UserControl
    {
        public BookingsPieChart()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty BookedColorProperty = 
            DependencyProperty.Register(nameof(BookedColor), typeof(Brush),  typeof(BookingsPieChart), new PropertyMetadata(Brushes.Green));

        public Brush BookedColor
        {
            get => (Brush)GetValue(BookedColorProperty);
            set => SetValue(BookedColorProperty, value);
        }

        public static readonly DependencyProperty UnbookedColorProperty =
            DependencyProperty.Register(nameof(UnbookedColor), typeof(Brush), typeof(BookingsPieChart), new PropertyMetadata(Brushes.LightGray));

        public Brush UnbookedColor
        {
            get => (Brush)GetValue(UnbookedColorProperty);
            set => SetValue(UnbookedColorProperty, value);
        }

        public static readonly DependencyProperty BookedPercentageProperty =
            DependencyProperty.Register(nameof(BookedPercentage), typeof(double), typeof(BookingsPieChart), new PropertyMetadata(0d));

        public double BookedPercentage
        {
            get => (double)GetValue(BookedPercentageProperty);
            set => SetValue(BookedPercentageProperty, value);
        }

        public static readonly DependencyProperty UnbookedPercentageProperty =
            DependencyProperty.Register(nameof(UnbookedPercentage), typeof(double), typeof(BookingsPieChart), new PropertyMetadata(0d));

        public double UnbookedPercentage
        {
            get => (double)GetValue(UnbookedPercentageProperty);
            set => SetValue(UnbookedPercentageProperty, value);
        }
    }
}

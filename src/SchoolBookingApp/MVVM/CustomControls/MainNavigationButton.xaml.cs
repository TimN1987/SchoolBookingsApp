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
    /// Interaction logic for MainNavigationButton.xaml
    /// </summary>
    public partial class MainNavigationButton : UserControl
    {
        public MainNavigationButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IconProperty = 
            DependencyProperty.Register(nameof(Icon), typeof(Uri), typeof(MainNavigationButton), new PropertyMetadata(null));

        public Uri Icon
        {
            get => (Uri)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty TargetViewProperty =
            DependencyProperty.Register(nameof(TargetView), typeof(string), typeof(MainNavigationButton), new PropertyMetadata(string.Empty));

        public string TargetView
        {
            get => (string)GetValue(TargetViewProperty);
            set => SetValue(TargetViewProperty, value);
        }
    }
}

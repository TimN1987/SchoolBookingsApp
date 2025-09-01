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
    /// Interaction logic for DataBox.xaml
    /// </summary>
    public partial class DataBox : UserControl
    {
        public DataBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(DataBox), new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register(nameof(Number), typeof(string), typeof(DataBox), new PropertyMetadata(string.Empty));

        public string Number
        {
            get => (string)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }
    }
}

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
using SchoolBookingApp.MVVM.Viewmodel;

namespace SchoolBookingApp.MVVM.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private readonly HomeViewModel _homeViewModel;
        public HomeView(HomeViewModel viewModel)
        {
            InitializeComponent();
            _homeViewModel = viewModel;
            DataContext = _homeViewModel;
        }
    }
}

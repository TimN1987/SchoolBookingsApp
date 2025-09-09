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
using Serilog;

namespace SchoolBookingApp.MVVM.View
{
    /// <summary>
    /// Interaction logic for ViewBookingView.xaml
    /// </summary>
    public partial class ViewBookingView : UserControl
    {
        private readonly ViewBookingViewModel _viewModel;

        public ViewBookingView(ViewBookingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Log.Information("ViewBookingView initialized");
        }
    }
}

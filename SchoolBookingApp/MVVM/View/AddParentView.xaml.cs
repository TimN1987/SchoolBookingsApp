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
    /// Interaction logic for AddParentView.xaml
    /// </summary>
    public partial class AddParentView : UserControl
    {
        private readonly AddParentViewModel _viewModel;

        public AddParentView(AddParentViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel 
                ?? throw new ArgumentNullException(nameof(viewModel));

            DataContext = _viewModel;
        }
    }
}

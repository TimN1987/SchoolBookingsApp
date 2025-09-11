using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SchoolBookingApp.MVVM.Commands;
using SchoolBookingApp.MVVM.Factories;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.View;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    /// <summary>
    /// Provides the main view model for application, managing which <see cref="UserControl"/>s are displayed in the <see 
    /// cref="MainWindow"/>. Handles navigation by responding to <see cref="NavigateToViewEvent"/>s and uses an <see 
    /// cref="IViewFactory"/> to crwate and supply the requested views.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IViewFactory _viewFactory;
        private readonly IEventAggregator _eventAggregator;
        private UserControl? _currentView;

        //Commands
        private ChangePageCommand<HomeView>? _homeViewCommand;
        private ChangePageCommand<AddStudentView>? _addStudentViewCommand;
        private ChangePageCommand<AddParentView>? _addParentViewCommand;
        private ChangePageCommand<AddBookingView>? _addBookingViewCommand;

        /// <summary>
        /// The current <see cref="UserControl"/> that is displayed in the <see cref="MainWindow"/>'s <see 
        /// cref="ContentControl"/>. Used to bind to the <see cref="MainWindow"/> and enable the user to change views.
        /// </summary>
        public UserControl? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        //Commands
        public ChangePageCommand<HomeView>? HomeViewCommand
        {
            get
            {
                return _homeViewCommand 
                    ?? new ChangePageCommand<HomeView>(_eventAggregator);
            }
        }

        public ChangePageCommand<AddStudentView>? AddStudentViewCommand
        {
            get
            {
                return _addStudentViewCommand 
                    ?? new ChangePageCommand<AddStudentView>(_eventAggregator);
            }
        }

        public ChangePageCommand<AddParentView>? AddParentViewCommand
        {
            get
            {
                return _addParentViewCommand 
                    ?? new ChangePageCommand<AddParentView>(_eventAggregator);
            }
        }

        public ChangePageCommand<AddBookingView>? AddBookingViewCommand
        {
            get
            {
                return _addBookingViewCommand 
                    ?? new ChangePageCommand<AddBookingView>(_eventAggregator);
            }
        }

        //Text properties for binding to Button content in the MainWindow.
        public static string HomeButtonText => "Home";
        public static string AddStudentButtonText => "Add Student";
        public static string AddParentButtonText => "Add Parent";
        public static string AddBookingButtonText => "Add Booking";


        //Image properties for binding to Button content in the MainWindow.
        public static ImageSource HomeButtonImage => new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/MainWindow/home.png", UriKind.Absolute));
        public static ImageSource AddStudentButtonImage => new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/MainWindow/addstudent.png", UriKind.Absolute));
        public static ImageSource AddParentButtonImage => new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/MainWindow/addparent.png", UriKind.Absolute));
        public static ImageSource AddBookingButtonImage => new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/MainWindow/addbooking.png", UriKind.Absolute));

        public MainViewModel(IEventAggregator eventAggregator, IViewFactory viewFactory)
        {
            _viewFactory = viewFactory
                ?? throw new ArgumentNullException(nameof(viewFactory));
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));

            _currentView = _viewFactory.CreateView<HomeView>();
            _eventAggregator.GetEvent<NavigateToViewEvent>()
                .Subscribe(viewType => ChangeView(viewType));

            _homeViewCommand = null;
        }

        /// <summary>
        /// Changes the displayed <see cref="UserControl"/> to the requested type of view. Creates a version of the 
        /// <see cref="ViewFactory.CreateView"/> generic method to resolve the requested view from the <see 
        /// cref="ServiceProvider"/> DI container and set this to the <see cref="MainViewModel.CurrentView"/>. Used to 
        /// display the requested view in the <see cref="MainWindow"/>.
        /// </summary>
        /// <param name="viewType">The type of view to be displayed.</param>
        /// <remarks>The desired view and its associated viewmodel must be registered with the DI container.</remarks>
        private void ChangeView(Type viewType)
        {
            var method = _viewFactory.GetType()
                .GetMethod(nameof(IViewFactory.CreateView))!
                .MakeGenericMethod(viewType);

            CurrentView = (UserControl)method.Invoke(_viewFactory, null)!;
        }
    }
}

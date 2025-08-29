using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
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

        /// <summary>
        /// The current <see cref="UserControl"/> that is displayed in the <see cref="MainWindow"/>'s <see 
        /// cref="ContentControl"/>. Used to bind to the <see cref="MainWindow"/> and enable the user to change views.
        /// </summary>
        public UserControl? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        //Text properties for binding to Button content in the MainWindow.
        public string HomeButtonText => "Home";


        public MainViewModel(IEventAggregator eventAggregator, IViewFactory viewFactory)
        {
            _viewFactory = viewFactory
                ?? throw new ArgumentNullException(nameof(viewFactory));
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));

            _currentView = _viewFactory.CreateView<HomeView>();
            _eventAggregator.GetEvent<NavigateToViewEvent>()
                .Subscribe(viewType => ChangeView(viewType));
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

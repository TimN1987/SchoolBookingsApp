using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace SchoolBookingApp.MVVM.Factories
{
    /// <summary>
    /// Defines a contract for the <see cref="ViewFactory"/> <see langword="class"/>. Ensures that the <see cref="ViewFactory"/> 
    /// contains a method to create a view of type <see cref="UserControl"/>.
    /// </summary>
    public interface IViewFactory
    {
        T CreateView<T>() where T : UserControl;
    }

    /// <summary>
    /// A class for creating views using the <see cref="IServiceProvider"/>. Used to load <see cref="UserControl"/>s as 
    /// views in the application.
    /// </summary>
    /// <param name="serviceProvider">The instance of the <see cref="IServiceProvider"/> created on startup and used to 
    /// register the views and viewmodels.</param>
    /// <remarks>All views and their associated viewmodels must be correctly registered with the <see cref="IServiceProvider"/>
    /// DI container. The viewmodels should be set as the DataContext for their associated view through constructor 
    /// injection.</remarks>
    public class ViewFactory(IServiceProvider serviceProvider) : IViewFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider 
            ?? throw new ArgumentNullException(nameof(serviceProvider));
        
        /// <summary>
        /// Creates an instance of a view that is registered with the <see cref="IServiceProvider"/> DI container. Used to 
        /// load views for UI display.
        /// </summary>
        /// <typeparam name="T">A <see cref="UserControl"/> that contains the view to be displayed.</typeparam>
        /// <returns>An instance of the requested <see cref="UserControl"/> with the constructor injection of its 
        /// viewmodel resolved by the <see cref="IServiceProvider"/> DI container.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the required <see cref="UserControl"/> cannot be 
        /// retrieved from the DI container. Suggests that the view or viewmodel has not be correctly registered.</exception>
        /// <remarks>Both the view and its associated viewmodel must be registered with the <see cref="IServiceProvider"/> 
        /// DI container.</remarks>
        /// <example>
        /// An exmaple of how to load a view using a <see cref="ViewFactory"/> instance:
        /// <code>
        /// private readonly ViewFactory _viewFactory = new ViewFactory(serviceProvider);
        /// CurrentView = _viewFactory.CreateView<HomeView>();
        /// </code>
        /// </example>
        public T CreateView<T>() where T : UserControl
        {
            try
            {
                return _serviceProvider.GetRequiredService<T>();
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, $"An error occurred creating instance of view: {typeof(T).FullName}. Check DI registration.");
                throw;
            }
        }
    }
}

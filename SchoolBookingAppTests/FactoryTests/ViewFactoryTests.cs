using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using SchoolBookingAppTests.Mocks;
using SchoolBookingApp.Factories;

namespace SchoolBookingAppTests.FactoryTests;

public class ViewFactoryTests
{
    private readonly IServiceCollection _serviceCollection;
    private readonly IServiceProvider _emptyServiceProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventAggregator _eventAggregator;
    private readonly ViewFactory _emptyFactory;
    private readonly ViewFactory _viewFactory;

    public ViewFactoryTests()
    {
        _serviceCollection = new ServiceCollection();
        _emptyServiceProvider = _serviceCollection.BuildServiceProvider();

        _serviceCollection.AddTransient<MockView>();
        _serviceProvider = _serviceCollection.BuildServiceProvider();

        _eventAggregator = new EventAggregator();

        _emptyFactory = new ViewFactory(_emptyServiceProvider, _eventAggregator);
        _viewFactory = new ViewFactory(_serviceProvider, _eventAggregator);
    }

    //Constructor tests.

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown if a <see langword="null"/> parameter is passed 
    /// to the <see cref="ViewFactory"/> constructor. Ensures that a <see cref="ViewFactory"/> instance cannot be 
    /// created with no valid <see cref="ServiceProvider"/> to retrieve the required <see cref="UserControl"/>s;
    /// </summary>
    [Fact]
    public void Constructor_NullParameter_ThrowsArgumentNullException()
    {
        //Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ViewFactory(null!, _eventAggregator));
    }

    /// <summary>
    /// Verifies that a valid instance of the <see cref="ViewFactory"/> <see langword="class"/> is created when a 
    /// valid <see cref="ServiceProvider"/> is passed as the parameter.
    /// </summary>
    [Fact]

    public void Constructor_ValidServiceProviderParameter_CreatesInstance()
    {
        //Arrange & Act
        var emptyFactory = new ViewFactory(_emptyServiceProvider, _eventAggregator);
        var viewFactory = new ViewFactory(_serviceProvider, _eventAggregator);

        //Assert
        Assert.NotNull(emptyFactory);
        Assert.NotNull(viewFactory);

        Assert.IsType<ViewFactory>(emptyFactory);
        Assert.IsType<ViewFactory>(viewFactory);

    }

    //CreateView tests.

    /// <summary>
    /// Verifies that an <see cref="InvalidOperationException"/> is thrown if the <see cref="ViewFactory.CreateView"/> 
    /// method is called with a view that is not registered with the <see cref="ServiceProvider"/>. Ensures that the 
    /// issue can be diagnosed quickly and resolved through correct registration if this has been overlooked.
    /// </summary>
    [Fact]
    public void CreateView_NoRegisteredView_ThrowsInvalidOperationException()
    {
        //Arrange, Act & Assert
        Assert.Throws<InvalidOperationException>(() => _emptyFactory.CreateView<MockView>());
    }

    /// <summary>
    /// Verifies that the expected type of <see cref="UserControl"/> is resolved from the <see cref="ServiceProvider"/> 
    /// when the <see cref="ViewFactory.CreateView"/> method is called. Ensures that the method works as expected to 
    /// provide the requested view for display in the application.
    /// </summary>
    [StaFact]
    public void CreateView_CorrectlyRegisteredView_ReturnsInstanceOfView()
    {
        //Arrange & Act
        var view = _viewFactory.CreateView<MockView>();

        //Assert
        Assert.NotNull(view);
        Assert.IsType<MockView>(view);
    }

    /// <summary>
    /// Verifies that an error message is logged when the <see cref="ViewFactory.CreateView"/> method attempts to 
    /// create an instance of a <see cref="UserControl"/> that is not registered with the <see cref="ServiceProvider"/>. 
    /// Checks that the logged message has the correct event level and contains the correct text.
    /// </summary>
    [StaFact]
    public void CreateView_UnregisteredView_LogsErrorMessage()
    {
        //Arrange
        Log.Logger = new LoggerConfiguration()
            .WriteTo.InMemory()
            .CreateLogger();
        var logEvents = InMemorySink.Instance.LogEvents;

        //Act & Assert
        Assert.Throws<InvalidOperationException>(() => _emptyFactory.CreateView<MockView>());

        Assert.Contains(logEvents, e => 
            e.Level == LogEventLevel.Error && 
            e.MessageTemplate.Text.Contains("An error occurred creating instance of view"));
    }
}

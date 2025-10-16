using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.Factories;
using SchoolBookingApp.MVVM.View;
using SchoolBookingApp.MVVM.Viewmodel;
using SchoolBookingApp.Services;
using SchoolBookingAppTests.Mocks;

namespace SchoolBookingAppTests.ViewModelTests;

public class MainViewModelTests
{
    private readonly Mock<IViewFactory> _viewFactoryMock;
    private readonly IEventAggregator _eventAggregator;
    private readonly MainViewModel _mainViewModel;

    public MainViewModelTests()
    {
        _viewFactoryMock = new Mock<IViewFactory>();
        _eventAggregator = new EventAggregator();

        _viewFactoryMock
            .Setup(x => x.CreateView<MockView>())
            .Returns(new MockView());

        _mainViewModel = new MainViewModel(_eventAggregator, _viewFactoryMock.Object);
    }

    //Constructor tests.

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown if the <see cref="IEventAggregator"/> parameter 
    /// is null. Ensures that an instance is not created where there is not a valid <see cref="EventAggregator"/> to 
    /// subscribe to navigation events and display the requested views.
    /// </summary>
    [StaFact]
    public void Constructor_NullEventAggregator_ThrowsArgumentNullException()
    {
        //Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(null!, _viewFactoryMock.Object));
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown if the <see cref="IViewFactory"/> parameter 
    /// is null. Ensures that an instance is not created where there is not a valid <see cref="ViewFactory"/> to 
    /// create and display views.
    /// </summary>
    [StaFact]
    public void Constructor_NullViewFactory_ThrowsArgumentNullException()
    {
        //Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(_eventAggregator, null!));
    }

    /// <summary>
    /// Verifies that an instance of the <see cref="MainViewModel"/> class is created when the constructor is called 
    /// with valid parameters. Ensures that the <see cref="MainViewModel"/> can be successfully created.
    /// </summary>
    [StaFact]
    public void Constructor_ValidParameters_CreatesInstanceSuccessfully()
    {
        //Arrange & Act
        var mainViewModel = new MainViewModel(_eventAggregator, _viewFactoryMock.Object);

        //Assert
        Assert.NotNull(mainViewModel);
        Assert.IsType<MainViewModel>(mainViewModel);
    }

    //NavigateToViewEvent subscription tests.

    /// <summary>
    /// Verifies that the correct view is set to the <see cref="MainViewModel.CurrentView"/> property when a <see 
    /// cref="NavigateToViewEvent"/> is published with a given type of view. Ensures that <see cref="MainViewModel"/> 
    /// subscription to the <see cref="NavigateToViewEvent"/> works as expected.
    /// </summary>
    [StaFact]
    public void NavigateToViewEvent_MockViewPublished_CurrentViewSetToMockView()
    {
        //Arrange & Act
        _eventAggregator
            .GetEvent<NavigateToViewEvent>()
            .Publish(typeof(MockView));

        //Assert
        Assert.IsType<MockView>(_mainViewModel.CurrentView);
    }
}

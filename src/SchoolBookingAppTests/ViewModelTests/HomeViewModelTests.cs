using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Viewmodel;

namespace SchoolBookingAppTests.ViewModelTests
{
    public class HomeViewModelTests
    {
        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly Mock<IBookingManager> _bookingManagerMock;
        private readonly Mock<IReadOperationService> _readOperationServiceMock;

        public HomeViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _bookingManagerMock = new Mock<IBookingManager>();
            _readOperationServiceMock = new Mock<IReadOperationService>();

            _bookingManagerMock
                .Setup(x => x.ListBookings())
                .ReturnsAsync([]);
            _readOperationServiceMock
                .Setup(x => x.GetStudentList())
                .ReturnsAsync([]);
        }

        //Constructor tests.

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown if any of the parameters for the <see 
        /// cref="HomeViewModel"/> are null. Ensures that the unexpected errors do not occur as a result of invalid 
        /// constructor parameters.
        /// </summary>
        /// <param name="eventAggregator">An <see cref="EventAggregator"/> instance injected by the <see 
        /// cref="ServiceProvider"/>.</param>
        /// <param name="bookingManager">A <see cref="BookingManager"/> instance injected by the <see 
        /// cref="ServiceProvider"/>.</param>
        /// <param name="readOperationService">A <see cref="ReadOperationService"/> instance injected by the <see 
        /// cref="ServiceProvider"/>.</param>
        [StaTheory]
        [MemberData(nameof(ConstructorNullParameterMemberData))]
        public void Constructor_NullParameter_ThrowsArgumentNullException(
            IEventAggregator eventAggregator, IBookingManager bookingManager, IReadOperationService readOperationService)
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new HomeViewModel(eventAggregator, bookingManager, readOperationService));
        }

        /// <summary>
        /// Verifies that when valid parameters are passed to the <see cref="HomeViewModel"/> constructor, an instance 
        /// of the class is successfully created with the expected starting conditions. Ensures that the class can be 
        /// successfully instantiated without unexpected errors occurring.
        /// </summary>
        [StaFact]
        public void Constructor_ValidParameters_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var homeViewModel = new HomeViewModel(
                _eventAggregatorMock.Object, _bookingManagerMock.Object, _readOperationServiceMock.Object);

            //Assert
            Assert.NotNull(homeViewModel);
            Assert.IsType<HomeViewModel>(homeViewModel);
            Assert.NotNull(homeViewModel.Bookings);
            Assert.NotNull(homeViewModel.Students);
        }

        //Member data.

        /// <summary>
        /// Provides member data with different combinations of null parameters for constructor tests to ensure that when 
        /// a null parameter is passed to the <see cref="HomeViewModel"/> constructor, an <see cref="ArgumentNullException"/> 
        /// is thrown.
        /// </summary>
        public static IEnumerable<object[]> ConstructorNullParameterMemberData()
        {
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var bookingManager = new Mock<IBookingManager>().Object;
            var readOperationService = new Mock<IReadOperationService>().Object;
            
            yield return new object[] { null!, bookingManager, readOperationService };
            yield return new object[] { eventAggregator, null!, readOperationService };
            yield return new object[] { eventAggregator, bookingManager, null! };
            yield return new object[] { eventAggregator, null!, null! };
            yield return new object[] { null!, bookingManager, null! };
            yield return new object[] { null!, null!, readOperationService };
            yield return new object[] { null!, null!, null! };
        }
    }
}

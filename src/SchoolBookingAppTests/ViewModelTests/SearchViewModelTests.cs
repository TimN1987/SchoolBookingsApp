using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Viewmodel;

namespace SchoolBookingAppTests.ViewModelTests
{
    public class SearchViewModelTests
    {
        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly Mock<IReadOperationService> _readOperationServiceMock;

        public SearchViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _readOperationServiceMock = new Mock<IReadOperationService>();
        }

        //Constructor tests.

        [Fact]
        public void Constructor_NullEventAggregator_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SearchViewModel(null!, _readOperationServiceMock.Object));
        }

        [Fact]
        public void Constructor_NullReadOperationService_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SearchViewModel(_eventAggregatorMock.Object, null!));
        }

        [Fact]
        public void Constructor_ValidParameters_InstantiatesViewModel()
        {
            // Arrange & Act
            var viewModel = new SearchViewModel(_eventAggregatorMock.Object, _readOperationServiceMock.Object);

            // Assert
            Assert.NotNull(viewModel);
            Assert.IsType<SearchViewModel>(viewModel);
        }

        [Fact]
        public void Constructor_ValidParameters_InitializesProperties()
        {
            // Arrange & Act
            var viewModel = new SearchViewModel(_eventAggregatorMock.Object, _readOperationServiceMock.Object);

            // Assert
            Assert.NotNull(viewModel);
            Assert.Equal(string.Empty, viewModel.SearchText);
            Assert.Equal(string.Empty, viewModel.TableName);
            Assert.Equal(string.Empty, viewModel.Field);
            Assert.Equal(string.Empty, viewModel.MainParameter);
            Assert.Equal(string.Empty, viewModel.SecondaryParameter);
            Assert.Equal(string.Empty, viewModel.StatusMessage);
            Assert.False(viewModel.IsAdvancedStudentSearch);
        }
    }
}

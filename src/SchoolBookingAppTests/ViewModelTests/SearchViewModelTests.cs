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
        private const string InvalidFieldMessage = "Ensure that a search property is selected.";
        private const string InvalidTableMessage = "Ensure that a search category is selected.";
        private const string DataMissingMessage = "Ensure that all required fields are filled.";
        private const string SearchErrorMessage = "An error occurred during the search. Please try again.";
        private const string InvalidInputMessage = "Invalid character attempted in input.";

        private const string ValidTableName = "Parents";
        private const string ValidFieldName = "FirstName";
        private const string ValidSearchText = "John";

        private const string WhiteSpace = "    ";
        private const string EmptyString = "";

        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly Mock<IReadOperationService> _readOperationServiceMock;
        private readonly SearchViewModel _viewModel;

        public SearchViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _readOperationServiceMock = new Mock<IReadOperationService>();
            _viewModel = new SearchViewModel(_eventAggregatorMock.Object, _readOperationServiceMock.Object);
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

        //KeywordSearch tests.

        /// <summary>
        /// Verifies that when TableName is null, empty, or whitespace, the StatusMessage is set to InvalidTableMessage. 
        /// Ensures that the correct status message is displayed for invalid table names.
        /// </summary>
        /// <param name="tableName">The invalid table name.</param>
        [Theory]
        [InlineData(null!)]
        [InlineData(EmptyString)]
        [InlineData(WhiteSpace)]
        public async Task KeywordSearch_InvalidTableName_SetsInvalidTableMessage(string tableName)
        {
            //Arrange
            _viewModel.TableName = tableName;
            _viewModel.Field = ValidFieldName;
            _viewModel.SearchText = ValidSearchText;

            //Act
            await _viewModel.KeywordSearch();

            //Assert
            Assert.Equal(InvalidTableMessage, _viewModel.StatusMessage);
        }
    }
}

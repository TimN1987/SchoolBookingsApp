using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Enums;
using SchoolBookingApp.MVVM.Viewmodel;

namespace SchoolBookingAppTests.ViewModelTests
{
    public class SearchViewModelTests
    {
        private const string InvalidFieldMessage = "Ensure that a search property is selected.";
        private const string InvalidTableMessage = "Ensure that a search category is selected.";
        private const string DataMissingMessage = "Ensure that all required fields are filled.";
        private const string NoResultsMessage = "No results found for the given search criteria.";
        private const string SearchErrorMessage = "An error occurred during the search. Please try again.";
        private const string InvalidInputMessage = "Invalid character attempted in input.";
        private const string MissingCriteriaMessage = "Ensure all search criteria are entered.";
        private const string IntegerRequiredMessage = "Ensure that you have entered valid numbers for your search.";

        private const string ValidTableName = "Parents";
        private const string ValidFieldName = "FirstName";
        private const string ValidSearchText = "John";

        private const SQLOperator ValidSQLOperator = SQLOperator.Equals;
        private const DatabaseField ValidDatabaseField = DatabaseField.GeneralComments;

        private const string WhiteSpace = "    ";
        private const string EmptyString = "";

        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly Mock<IReadOperationService> _readOperationServiceMock;
        private readonly SearchViewModel _viewModel;

        public SearchViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _readOperationServiceMock = new Mock<IReadOperationService>();

            _readOperationServiceMock.Setup(s => s.GetAllSearchData())
                .ReturnsAsync([]);
            _readOperationServiceMock.Setup(s => s.SearchByKeyword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync([]);
            _readOperationServiceMock.Setup(s => s.SearchByCriteria(It.IsAny<List<SearchCriteria>>()))
                .ReturnsAsync([]);

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

        /// <summary>
        /// Verifies that when FieldName is null, empty, or whitespace, the StatusMessage is set to InvalidFieldMessage. 
        /// Ensures that the correct status message is displayed for invalid field names.
        /// </summary>
        /// <param name="fieldName">The invalid field name.</param>
        [Theory]
        [InlineData(EmptyString)]
        [InlineData(WhiteSpace)]
        public async Task KeywordSearch_InvalidFieldName_SetsInvalidFieldMessage(string fieldName)
        {
            //Arrange
            _viewModel.TableName = ValidTableName;
            _viewModel.Field = fieldName;
            _viewModel.SearchText = ValidSearchText;

            //Act
            await _viewModel.KeywordSearch();

            //Assert
            Assert.Equal(InvalidFieldMessage, _viewModel.StatusMessage);
        }

        /// <summary>
        /// Verifies that the expected error message is displayed if a whitespace keyword is entered and so no results are 
        /// returned. Ensures that the user is clear why no results have been displayed.
        /// </summary>
        [Fact]
        public async Task KeywordSearch_WhiteSpaceKeyword_NoResultsMessageDisplayed()
        {
            //Arrange
            _viewModel.TableName = ValidTableName;
            _viewModel.Field = ValidFieldName;
            _viewModel.SearchText = WhiteSpace;

            //Act
            await _viewModel.KeywordSearch();

            //Assert
            Assert.Equal(NoResultsMessage, _viewModel.StatusMessage);
        }

        /// <summary>
        /// Verifies that the <see cref="ReadOperationService.SearchByKeyword"/> method is called exactly once if a valid 
        /// keyword (not null, whitespace or empty) is passed as the parameter of the <see cref="SearchViewModel.
        /// KeywordSearch"/> method.
        /// </summary>
        [Fact]
        public async Task KeywordSearch_ValidKeyword_SearchByKeywordCalledOnce()
        {
            //Arrange
            _viewModel.TableName = ValidTableName;
            _viewModel.Field = ValidFieldName;
            _viewModel.SearchText = ValidSearchText;
            _readOperationServiceMock.Setup(s => s.SearchByKeyword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<List<SearchResult>>());

            //Act
            await _viewModel.KeywordSearch();

            //Assert
            _readOperationServiceMock.Verify(s => s.SearchByKeyword(ValidSearchText, ValidTableName, ValidFieldName), Times.Once);
        }

        //AddSearchCriteria tests.

        [Fact]
        public void AddSearchCriteria_NullSearchField_MissingCriteriaMessageDisplayed()
        {
            //Arrange
            _viewModel.SqlOperator = ValidSQLOperator;
            _viewModel.SearchField = null!;
            _viewModel.MainParameter = ValidSearchText;
            _viewModel.IsSecondaryParameterVisible = false;

            //Act
            _viewModel.AddSearchCriteria();

            //Assert
            Assert.Equal(MissingCriteriaMessage, _viewModel.StatusMessage);
        }

        [Fact]
        public void AddSearchCriteria_NullSQLOperator_MissingCriteriaMessageDisplayed()
        {
            //Arrange
            _viewModel.SqlOperator = null;
            _viewModel.SearchField = ValidDatabaseField;
            _viewModel.MainParameter = ValidSearchText;
            _viewModel.IsSecondaryParameterVisible = false;

            //Act
            _viewModel.AddSearchCriteria();

            //Assert
            Assert.Equal(MissingCriteriaMessage, _viewModel.StatusMessage);
        }

        [Theory]
        [InlineData(EmptyString)]
        [InlineData(WhiteSpace)]
        public void AddSearchCriteria_InvalidMainParameter_MissingCriteriaMessageDisplayed(string mainParameter)
        {
            //Arrange
            _viewModel.SqlOperator = ValidSQLOperator;
            _viewModel.SearchField = ValidDatabaseField;
            _viewModel.MainParameter = mainParameter;
            _viewModel.IsSecondaryParameterVisible = false;

            //Act
            _viewModel.AddSearchCriteria();

            //Assert
            Assert.Equal(MissingCriteriaMessage, _viewModel.StatusMessage);
        }

        [Theory]
        [InlineData(EmptyString)]
        [InlineData(WhiteSpace)]
        public void AddSearchCriteria_InvalidSecondaryParameter_MissingCriteriaMessageDisplayed(string parameter)
        {
            //Arrange
            _viewModel.SqlOperator = ValidSQLOperator;
            _viewModel.SearchField = ValidDatabaseField;
            _viewModel.MainParameter = ValidSearchText;
            _viewModel.IsSecondaryParameterVisible = true;
            _viewModel.SecondaryParameter = parameter;

            //Act
            _viewModel.AddSearchCriteria();

            //Assert
            Assert.Equal(MissingCriteriaMessage, _viewModel.StatusMessage);
        }

        //AdvancedStudentSearch tests.

        [Fact(Skip = "Incomplete test")]
        public async Task AdvancedStudentSearch_EmptyCriteriaList_Calls() //Think more
        {
            //Arrange & Act
            _viewModel.CriteriaToBeApplied = [];
        }
    }
}

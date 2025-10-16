using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.Database;
using SchoolBookingApp.Enums;
using SchoolBookingApp.MVVM.Viewmodel;

namespace SchoolBookingAppTests.ViewModelTests;

public class SearchViewModelTests
{
    private const string InvalidFieldMessage = "Ensure that a search property is selected.";
    private const string InvalidTableMessage = "Ensure that a search category is selected.";
    private const string DataMissingMessage = "Ensure that all required fields are filled.";
    private const string InvalidDataMessage = "Ensure that all required fields are filled correctly.";
    private const string NoResultsMessage = "No results found for the given search criteria.";
    private const string SearchErrorMessage = "An error occurred during the search. Please try again.";
    private const string InvalidInputMessage = "Invalid character attempted in input.";
    private const string MissingCriteriaMessage = "Ensure all search criteria are entered.";
    private const string IntegerRequiredMessage = "Ensure that you have entered valid numbers for your search.";
    private const string NoCriteriaMessage = "Ensure criteria are added before searching.";

    private const string ValidTableName = "Parents";
    private const string ValidFieldName = "FirstName";
    private const string ValidSearchText = "John";

    private const SQLOperator ValidSQLOperator = SQLOperator.Equals;
    private const DatabaseField ValidDatabaseField = DatabaseField.GeneralComments;

    private const DatabaseField TextDatabaseField = DatabaseField.GeneralComments;
    private const DatabaseField IntegerDatabaseField = DatabaseField.Math;
    private const SQLOperator TextOperator = SQLOperator.Like;
    private const SQLOperator IntegerOperator = SQLOperator.GreaterThan;
    private const string TextString = "Text string";
    private const string IntegerString = "12345";

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

    /// <summary>
    /// Verifies that the expected error message is displayed if a null <see cref="SearchViewModel.SearchField"/> is 
    /// passed to the <see cref="SearchViewModel.AddSearchCriteria"/> method. Ensures that an invalid criteria cannot 
    /// be added to the criteria list before searching.
    /// </summary>
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

    /// <summary>
    /// Verifies that the expected error message is displayed if a null <see cref="SearchViewModel.SqlOperator"/> is 
    /// passed to the <see cref="SearchViewModel.AddSearchCriteria"/> method. Ensures that an invalid criteria cannot 
    /// be added to the criteria list before searching.
    /// </summary>
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

    /// <summary>
    /// Verifies that the expected error message is displayed if a invalid <see cref="SearchViewModel.MainParameter"/> 
    /// is passed to the <see cref="SearchViewModel.AddSearchCriteria"/> method. Ensures that an invalid criteria cannot 
    /// be added to the criteria list before searching.
    /// </summary>
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

    /// <summary>
    /// Verifies that the expected error message is displayed if a invalid <see cref="SearchViewModel.SecondaryParameter"/> 
    /// is passed to the <see cref="SearchViewModel.AddSearchCriteria"/> method. Ensures that an invalid criteria cannot 
    /// be added to the criteria list before searching.
    /// </summary>
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

    /// <summary>
    /// Verifies that an invalid data type cannot be added to a search criteria and that the correct error message is 
    /// displayed. Ensures that invalid searches cannot be created.
    /// </summary>
    [Theory]
    [InlineData(IntegerDatabaseField, IntegerOperator, TextString, IntegerRequiredMessage)]
    [InlineData(TextDatabaseField, TextOperator, IntegerString, InvalidDataMessage)]
    public void AddSearchCriteria_InvalidMainParameterType_CorrectErrorMessageDisplayed(
        DatabaseField fieldType,
        SQLOperator operatorType,
        string parameterType,
        string expectedMessage)
    {
        //Arrange
        _viewModel.SearchField = fieldType;
        _viewModel.SqlOperator = operatorType;
        _viewModel.MainParameter = parameterType;
        _viewModel.IsSecondaryParameterVisible = false;

        //Act
        _viewModel.AddSearchCriteria();

        //Assert
        Assert.Equal(expectedMessage, _viewModel.StatusMessage);
    }

    /// <summary>
    /// Verifies that an invalid data type cannot be added to a search criteria and that the correct error message is 
    /// displayed. Ensures that invalid searches cannot be created.
    /// </summary>
    [Theory]
    [InlineData(IntegerDatabaseField, IntegerOperator, IntegerString, TextString, IntegerRequiredMessage)]
    [InlineData(TextDatabaseField, TextOperator, TextString, IntegerString, InvalidDataMessage)]
    public void AddSearchCriteria_InvalidSecondaryParameterType_CorrectErrorMessageDisplayed(
        DatabaseField fieldType,
        SQLOperator operatorType,
        string mainParameterType,
        string secondaryParameterType,
        string expectedMessage)
    {
        //Arrange
        _viewModel.SearchField = fieldType;
        _viewModel.SqlOperator = operatorType;
        _viewModel.MainParameter = mainParameterType;
        _viewModel.IsSecondaryParameterVisible = true;
        _viewModel.SecondaryParameter = secondaryParameterType;

        //Act
        _viewModel.AddSearchCriteria();

        //Assert
        Assert.Equal(expectedMessage, _viewModel.StatusMessage);
    }

    [Fact]
    public void AddSearchCriteria_ValidCriteria_CriteriaAddedToList()
    {
        //Arrange
        _viewModel.CriteriaToBeApplied = [];
        _viewModel.SearchField = ValidDatabaseField;
        _viewModel.SqlOperator = ValidSQLOperator;
        _viewModel.MainParameter = ValidSearchText;
        _viewModel.IsSecondaryParameterVisible = false;

        //Act
        _viewModel.AddSearchCriteria();

        //Assert
        Assert.NotEmpty(_viewModel.CriteriaToBeApplied);
        Assert.Single(_viewModel.CriteriaToBeApplied);
    }

    //AdvancedStudentSearch tests.

    /// <summary>
    /// Verifies that the no criteria message is displayed if the <see cref="SearchViewModel.AdvancedStudentSearch"/> 
    /// method is called with no criteria added to the <see cref="SearchViewModel.CriteriaToBeApplied"/> list. Ensures 
    /// that criteria are selected before a search is run.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AdvancedStudentSearch_EmptyCriteriaList_NoCriteriaMessageDisplayed()
    {
        //Arrange & Act
        _viewModel.CriteriaToBeApplied = [];

        //Act
        await _viewModel.AdvancedStudentSearch();

        //Assert
        Assert.Equal(NoCriteriaMessage, _viewModel.StatusMessage);
    }

    /// <summary>
    /// Verifies that the <see cref="ReadOperationService.SearchByCriteria"/> method is called exactly once if valid 
    /// criteria are added before the <see cref="SearchViewModel.AdvancedStudentSearch"/> method is called. Ensures that 
    /// the business logic works as expected.
    /// </summary>
    [Fact]
    public async Task AdvancedStudentSearch_ValidCriteria_SearchByCriteriaCalledOnce()
    {
        //Arrange
        _viewModel.CriteriaToBeApplied = [];
        _viewModel.SearchField = ValidDatabaseField;
        _viewModel.SqlOperator = ValidSQLOperator;
        _viewModel.MainParameter = ValidSearchText;
        _viewModel.IsSecondaryParameterVisible = false;
        _viewModel.AddSearchCriteria();

        //Act
        await _viewModel.AdvancedStudentSearch();

        //Assert
        _readOperationServiceMock.Verify(s => s.SearchByCriteria(It.IsAny<List<SearchCriteria>>()), Times.Once);
    }

    /// <summary>
    /// Verifies that an empty list is displayed if no results were returned by the <see cref="ReadOperationService.
    /// SearchByCriteria"/> method. Ensures that the user is not shown misleading or erroneous results.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AdvancedStudentSearch_SearchByCriteriaReturnsEmptyList_EmptyResultsListDisplayed()
    {
        //Arrange
        _viewModel.CriteriaToBeApplied = [];
        _viewModel.SearchField = ValidDatabaseField;
        _viewModel.SqlOperator = ValidSQLOperator;
        _viewModel.MainParameter = ValidSearchText;
        _viewModel.IsSecondaryParameterVisible = false;
        _viewModel.AddSearchCriteria();

        //Act
        await _viewModel.AdvancedStudentSearch();

        //Assert
        Assert.Empty(_viewModel.AdvancedStudentSearchResults);
    }

    /// <summary>
    /// Verifies that the correct status update is displayed if no search results were found. Ensures that the user is 
    /// informed of the reason that no results are displayed.
    /// </summary>
    [Fact]
    public async Task AdvancedStudentSearch_SearchByCriteriaReturnsEmptyList_NoResultsMessageDisplayed()
    {
        //Arrange
        _viewModel.CriteriaToBeApplied = [];
        _viewModel.SearchField = ValidDatabaseField;
        _viewModel.SqlOperator = ValidSQLOperator;
        _viewModel.MainParameter = ValidSearchText;
        _viewModel.IsSecondaryParameterVisible = false;
        _viewModel.AddSearchCriteria();

        //Act
        await _viewModel.AdvancedStudentSearch();

        //Assert
        Assert.Equal(NoResultsMessage, _viewModel.StatusMessage);
    }

    /// <summary>
    /// Verifies that the correct error message is displayed if the search is unable to complete. Ensures that the 
    /// user is aware an error has occurred.
    /// </summary>
    [Fact]
    public async Task AdvancedStudentSearch_ErrorOccurs_SearchErrorMessageDisplayed()
    {
        //Arrange
        _viewModel.CriteriaToBeApplied = [];
        _viewModel.SearchField = ValidDatabaseField;
        _viewModel.SqlOperator = ValidSQLOperator;
        _viewModel.MainParameter = ValidSearchText;
        _viewModel.IsSecondaryParameterVisible = false;
        _viewModel.AddSearchCriteria();

        _readOperationServiceMock.Setup(s => s.SearchByCriteria(It.IsAny<List<SearchCriteria>>()))
            .Throws<Exception>();

        //Act
        await _viewModel.AdvancedStudentSearch();

        //Assert
        Assert.Equal(SearchErrorMessage, _viewModel.StatusMessage);
    }
}

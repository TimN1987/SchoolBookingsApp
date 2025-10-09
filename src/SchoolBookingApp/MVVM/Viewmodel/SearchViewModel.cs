using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Commands;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Enums;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.View;
using SchoolBookingApp.MVVM.Viewmodel.Base;
using Serilog;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class SearchViewModel : ViewModelBase
    {
        //Constant status messages
        private const int StatusMessageDisplayTime = 2500; //in milliseconds
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
        private const string CannotViewResultMessage = "This result is invalid and cannot be viewed.";

        //UI label properties
        public string SearchTitle => IsAdvancedStudentSearch ? "Student Search" : "Keyword Search";
        public static string KeywordSearchLabel => "Keyword search";
        public static string AdvancedStudentSearchLabel => "Student search";
        public static string SearchFieldLabel => "Field";
        public static string SearchOperatorLabel => "Search type";
        public string SearchParameterLabel => IsSecondaryParameterVisible ? "Search parameters" : "Search parameter";
        public static string AddSearchCriteriaButtonLabel => "Add";
        public static string SearchButtonLabel => "Search";
        public static string ClearFormsButtonLabel => "Clear Forms";
        public static string ViewResultButtonLabel => "View Result";

        //Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly IReadOperationService _readOperationService;

        //Keyword search fields
        private string _searchText;
        private List<SearchResult> _searchResults;
        private SearchResult? _selectedKeywordResult;
        private string _tableName;
        private readonly List<string> _tablesList;
        private string _field;
        private readonly Dictionary<string, List<string>> _fieldsDictionary;
        private List<string> _fieldsList;

        //Advanced student search fields
        private DatabaseField? _searchField;
        private readonly List<DatabaseField> _databaseFieldsList;
        private SQLOperator? _sqlOperator;
        private List<SQLOperator> _sqlOperatorsList;
        private string _mainParameter;
        private string _secondaryParameter;
        private bool _isSecondaryParameterVisible;
        private List<SearchCriteria> _criteriaToBeApplied;
        private List<Student> _advancedStudentSearchResults;
        private Student? _selectedStudentResult;

        private string _statusMessage;
        private bool _isAdvancedStudentSearch;
        private readonly HashSet<DatabaseField> _fieldsRequiringIntegerParameters;
        private readonly HashSet<DatabaseField> _fieldsRequiringTextParameters;
        private readonly HashSet<SQLOperator> _operatorsRequiringTwoParameters;
        private readonly List<SQLOperator> _operatorsForText;
        private readonly List<SQLOperator> _operatorsForIntegers;

        //Command fields
        private RelayCommand? _addSearchCriteriaCommand;
        private RelayCommand? _keywordSearchCommand;
        private RelayCommand? _advancedStudentSearchCommand;
        private RelayCommand? _viewResultCommand;
        private RelayCommand? _clearFormsCommand;

        //Keyword search properties
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (IsSqlInjectionSafe(value))
                    SetProperty(ref _searchText, value);
                else
                {
                    OnPropertyChanged(nameof(SearchText));
                    StatusMessage = InvalidInputMessage;
                }
            }
        }
        public List<SearchResult> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }
        public SearchResult? SelectedKeywordResult
        {
            get => _selectedKeywordResult;
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedKeywordResult, null);
                    return;
                }

                SetProperty(ref _selectedKeywordResult, value);
                SelectedStudentResult = null; //Ensure that only one search result can be selected at any time.
            }
        }
        public string TableName
        {
            get => _tableName;
            set
            {
                SetProperty(ref _tableName, value);

                //Ensure the correct fields are avaialble to choose from for the selected table.
                if (_fieldsDictionary.TryGetValue(_tableName, out List<string>? fields))
                {
                    FieldsList = fields;
                }
            }
        }
        public string Field
        {
            get => _field;
            set => SetProperty(ref _field, value);
        }
        public List<string> TablesList => _tablesList;
        public List<string> FieldsList
        {
            get => _fieldsList;
            set => SetProperty(ref _fieldsList, value);
        }

        //Advanced student search properties
        public DatabaseField? SearchField
        {
            get => _searchField;
            set
            {
                SetProperty(ref _searchField, value);
                DisplayValidSQLOperators();
                SqlOperator = null; //Ensures that no invalid operator can be selected when the search field is changed.
            }
        }
        public List<DatabaseField> DatabaseFieldsList => _databaseFieldsList;
        public SQLOperator? SqlOperator
        {
            get => _sqlOperator;
            set
            {
                if (value == null)
                {
                    SetProperty(ref _sqlOperator, null);
                    return;
                }

                SetProperty(ref _sqlOperator, value);

                if (_operatorsRequiringTwoParameters.Contains((SQLOperator)value))
                    IsSecondaryParameterVisible = true;
                else
                    IsSecondaryParameterVisible = false;
            }
        }
        public List<SQLOperator> SqlOperatorsList
        {
            get => _sqlOperatorsList;
            set => SetProperty(ref _sqlOperatorsList, value);
        }
        public string MainParameter
        {
            get => _mainParameter;
            set
            {
                if (IsSqlInjectionSafe(value))
                    SetProperty(ref _mainParameter, value);
                else //Do not allow invalid characters to be added - refresh the UI to the last valid string.
                {
                    OnPropertyChanged(nameof(MainParameter));
                    StatusMessage = InvalidInputMessage;
                }
            }
        }
        public string SecondaryParameter
        {
            get => _secondaryParameter;
            set
            {
                if (IsSqlInjectionSafe(value))
                    SetProperty(ref _secondaryParameter, value);
                else //Do not allow invalid characters to be added - refresh the UI to the last valid string.
                {
                    OnPropertyChanged(nameof(SecondaryParameter));
                    StatusMessage = InvalidInputMessage;
                }
            }
        }
        public bool IsSecondaryParameterVisible
        {
            get => _isSecondaryParameterVisible;
            set
            {
                SetProperty(ref _isSecondaryParameterVisible, value);
                OnPropertyChanged(nameof(SearchParameterLabel));
            }
        }
        public List<SearchCriteria> CriteriaToBeApplied
        {
            get => _criteriaToBeApplied;
            set => SetProperty(ref _criteriaToBeApplied, value);
        }
        public List<Student> AdvancedStudentSearchResults
        {
            get => _advancedStudentSearchResults;
            set => SetProperty(ref _advancedStudentSearchResults, value);
        }
        public Student? SelectedStudentResult
        {
            get => _selectedStudentResult;
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedStudentResult, null);
                    return;
                }

                SetProperty(ref _selectedStudentResult, value);
                SelectedKeywordResult = null; //Ensure that only one search result can be selected at any time.
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                SetProperty(ref _statusMessage, value);
                Task.Run(async () => await ClearStatusMessageAfterDelay());
            }
        }
        public bool IsAdvancedStudentSearch
        {
            get => _isAdvancedStudentSearch;
            set
            {
                SetProperty(ref _isAdvancedStudentSearch, value);
                OnPropertyChanged(nameof(SearchTitle));
                ClearForms(); //Ensure that the search fields are cleared ready for a new search.
            }
        }

        //Command properties

        public RelayCommand? AddSearchCriteriaCommand => _addSearchCriteriaCommand
            ??= new RelayCommand(param => AddSearchCriteria());
        public RelayCommand? SearchCommand => IsAdvancedStudentSearch
            ? _advancedStudentSearchCommand ??= new RelayCommand(async param => await AdvancedStudentSearch())
            : _keywordSearchCommand ??= new RelayCommand(async param => await KeywordSearch());
        public RelayCommand? ViewResultCommand => _viewResultCommand
            ??= new RelayCommand(param => ViewResult());
        public RelayCommand? ClearFormsCommand => _clearFormsCommand
            ??= new RelayCommand(param => ClearForms());

        //Constructor
        public SearchViewModel(IEventAggregator eventAggregator, IReadOperationService readOperationService)
        {
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _readOperationService = readOperationService 
                ?? throw new ArgumentNullException(nameof(readOperationService));

            _searchText = string.Empty;
            _searchResults = _readOperationService.GetAllSearchData().GetAwaiter().GetResult();
            _selectedKeywordResult = null;
            _tableName = string.Empty;
            _tablesList = ["All", "Students", "Parents"];
            _field = string.Empty;
            _fieldsDictionary = new Dictionary<string, List<string>>
            {
                { "All", new List<string> { "All", "FirstName", "LastName" } },
                { "Students", new List<string> { "All", "FirstName", "LastName", "ClassName" } },
                { "Parents", new List<string> { "All", "FirstName", "LastName" } }
            };
            _fieldsList = [];

            _searchField = null;
            _databaseFieldsList = Enum.GetValues(typeof(DatabaseField))
                .Cast<DatabaseField>()
                .ToList();
            _sqlOperator = null;
            _sqlOperatorsList = Enum.GetValues(typeof(SQLOperator))
                .Cast<SQLOperator>()
                .ToList();
            _mainParameter = string.Empty;
            _secondaryParameter = string.Empty;
            _isSecondaryParameterVisible = false;
            _criteriaToBeApplied = [];
            _advancedStudentSearchResults = [];
            _selectedStudentResult = null;

            _statusMessage = string.Empty;
            _isAdvancedStudentSearch = false;
            _fieldsRequiringIntegerParameters =
                [
                DatabaseField.Math,
                DatabaseField.Reading,
                DatabaseField.Writing,
                DatabaseField.Science,
                DatabaseField.History,
                DatabaseField.Geography,
                DatabaseField.MFL,
                DatabaseField.PE,
                DatabaseField.Art,
                DatabaseField.Music,
                DatabaseField.DesignTechnology,
                DatabaseField.RE,
                DatabaseField.Computing
                ];
            _fieldsRequiringTextParameters = 
                [
                DatabaseField.FirstName,
                DatabaseField.LastName,
                DatabaseField.Class,
                DatabaseField.ParentFirstName,
                DatabaseField.ParentLastName,
                DatabaseField.MathComments,
                DatabaseField.ReadingComments,
                DatabaseField.WritingComments,
                DatabaseField.GeneralComments,
                DatabaseField.PupilComments,
                DatabaseField.ParentComments,
                DatabaseField.BehaviorNotes,
                DatabaseField.AttendanceNotes,
                DatabaseField.HomeworkNotes,
                DatabaseField.ExtraCurricularNotes,
                DatabaseField.SpecialEducationalNeedsNotes,
                DatabaseField.SafeguardingNotes,
                DatabaseField.OtherNotes
                ];
            _operatorsRequiringTwoParameters =
                [
                SQLOperator.Between,
                SQLOperator.NotBetween
                ];
            _operatorsForIntegers = 
                [
                SQLOperator.Between,
                SQLOperator.NotBetween,
                SQLOperator.GreaterThan,
                SQLOperator.GreaterThanOrEqual,
                SQLOperator.LessThan,
                SQLOperator.LessThanOrEqual,
                SQLOperator.Equals,
                SQLOperator.NotEquals
                ];
            _operatorsForText = 
                [
                SQLOperator.Equals,
                SQLOperator.NotEquals,
                SQLOperator.In,
                SQLOperator.NotIn,
                SQLOperator.Like,
                SQLOperator.NotLike
                ];
        }

        //Search methods

        /// <summary>
        /// Searches the given table and field using the <see cref="SearchText"/> input and updates the <see 
        /// cref="SearchResults"/> with the results of the search. Enables the user to perform quick searches by keyword.
        /// </summary>
        public async Task KeywordSearch()
        {
            if (string.IsNullOrWhiteSpace(_tableName))
            {
                StatusMessage = InvalidTableMessage;
                return;
            }

            if (string.IsNullOrWhiteSpace(_field))
            {
                StatusMessage = InvalidFieldMessage;
                return;
            }

            try
            {
                List<SearchResult> results = await _readOperationService.SearchByKeyword(_searchText, _tableName, _field);
                SearchResults = results;

                if (results.Count == 0)
                    StatusMessage = NoResultsMessage;
            }
            catch (ArgumentNullException ex)
            {
                Log.Information(ex, "A null keyword was entered to the SearchByKeyword method.");
                StatusMessage = DataMissingMessage;
            }
            catch (ArgumentException ex)
            {
                Log.Information(ex, "An invalid table or field name was entered to the SearchByKeyword method.");
                StatusMessage = InvalidTableMessage;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred during keyword search.");
                StatusMessage = SearchErrorMessage;
            }
        }

        /// <summary>
        /// Searches the <see cref="Student"/>s in the database to find all that match the given list of <see 
        /// cref="CriteriaToBeApplied"/>. Contains validation to ensure that the criteria list is not <see langword="null"/> 
        /// or empty. Allows the user to search for students based on multiple criteria.
        /// </summary>
        public async Task AdvancedStudentSearch()
        {
            if (_criteriaToBeApplied == null || _criteriaToBeApplied.Count == 0)
            {
                StatusMessage = NoCriteriaMessage;
                return;
            }

            try
            {
                List<Student> results = await _readOperationService.SearchByCriteria(_criteriaToBeApplied);
                AdvancedStudentSearchResults = results;

                if (results.Count == 0)
                    StatusMessage = NoResultsMessage;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while searching for students by criteria.");
                AdvancedStudentSearchResults = [];
                StatusMessage = SearchErrorMessage;
            }
        }

        /// <summary>
        /// Adds the currently selected search criteria to the <see cref="CriteriaToBeApplied"/> list ready for a search to 
        /// be called with the given criteria. Contains validation to ensure that searches can run as expected. Allows the 
        /// user to add to a list of criteria to customise their student search.
        /// </summary>
        public void AddSearchCriteria()
        {
            //Ensure that valid search criteria have been entered.
            if (_sqlOperator == null || _searchField == null
                || string.IsNullOrWhiteSpace(_mainParameter)
                || (_isSecondaryParameterVisible && string.IsNullOrWhiteSpace(_secondaryParameter)))
            {
                StatusMessage = MissingCriteriaMessage;
                return;
            }

            //Ensure that the parameters are of the correct type.
            if (!IsValidParameterForOperator(_mainParameter) 
                || (_isSecondaryParameterVisible && !IsValidParameterForOperator(_secondaryParameter)))
            {
                bool isIntegerExpected = _fieldsRequiringIntegerParameters.Contains((DatabaseField)_searchField);
                StatusMessage = isIntegerExpected ? IntegerRequiredMessage : InvalidDataMessage;
                return;
            }

            object[] parameters = _isSecondaryParameterVisible 
                ? [_mainParameter, _secondaryParameter] 
                : [_mainParameter];
            var criteria = new SearchCriteria((DatabaseField)_searchField, (SQLOperator)_sqlOperator, parameters);

            _criteriaToBeApplied.Add(criteria);
            OnPropertyChanged(nameof(CriteriaToBeApplied));
            ResetSearchCriteria();
        }

        /// <summary>
        /// Navigates to the appropriate view to display the search result data fully and publishes an <see 
        /// cref="EventAggregator"/> message containing the <see cref="Student"/> or <see cref="Parent"/> id to allow the 
        /// relevant data to be loaded. Allows the user to see their selected search result in full detail.
        /// </summary>
        public void ViewResult()
        {
            //Retrieve the id for the selected student or parent as well as the category for a keyword search.
            int id = _isAdvancedStudentSearch ? _selectedStudentResult?.Id ?? 0 : _selectedKeywordResult?.Id ?? 0;
            string category = _selectedKeywordResult?.Category ?? string.Empty;

            if (id == 0 || (!_isAdvancedStudentSearch && category == string.Empty))
            {
                StatusMessage = CannotViewResultMessage;
                return;
            }

            //Navigate to the appropriate view to display the selected student or parent data.
            if (_isAdvancedStudentSearch)
                _eventAggregator.GetEvent<NavigateToViewEvent>().Publish(typeof(AddBookingView));
            if (category == "Student")
                _eventAggregator.GetEvent<NavigateToViewEvent>().Publish(typeof(AddStudentView));
            if (category == "Parent")
                _eventAggregator.GetEvent<NavigateToViewEvent>().Publish(typeof(AddParentView));

            //Publish the id to the selected view to enable loading of the relevant data.
            _eventAggregator.GetEvent<LoadFromIdEvent>().Publish(id);
        }

        //Display property methods

        /// <summary>
        /// Clears all information from the search forms and resets the view to its default state. Ensures that the user 
        /// has a simple reset option and can start again quickly in the case of mistakes.
        /// </summary>
        public void ClearForms()
        {
            ResetKeywordSearchProperties();
            ResetAdvancedStudentSearchProperties();

            StatusMessage = string.Empty;
        }

        /// <summary>
        /// Resets the keyword search properties to their default values. Enables simple clearing of searches and resetting 
        /// of the search form.
        /// </summary>
        private void ResetKeywordSearchProperties()
        {
            SearchText = string.Empty;
            SearchResults = _readOperationService.GetAllSearchData().GetAwaiter().GetResult();
            SelectedKeywordResult = null;
            TableName = string.Empty;
            Field = string.Empty;
            FieldsList = [];
        }

        /// <summary>
        /// Resets the advanced student search properties to their default values. Enables simple clearing of searches
        /// and resetting of the search form.
        /// </summary>
        private void ResetAdvancedStudentSearchProperties()
        {
            SearchField = null;
            SqlOperator = null;
            SqlOperatorsList = [];
            MainParameter = string.Empty;
            SecondaryParameter = string.Empty;
            IsSecondaryParameterVisible = false;
            CriteriaToBeApplied = [];
            AdvancedStudentSearchResults = [];
            SelectedKeywordResult = null;
        }

        /// <summary>
        /// Resets the search criteria fields ready to start a new search criteria. Used after adding a search criteria to 
        /// the list to ensure that the fields are cleared for the new criteria.
        /// </summary>
        private void ResetSearchCriteria()
        {
            SearchField = null;
            SqlOperator = null;
            SqlOperatorsList = [];
            MainParameter = string.Empty;
            SecondaryParameter = string.Empty;
            IsSecondaryParameterVisible = false;
        }

        /// <summary>
        /// Updates the <see cref="SqlOperatorsList"/> to the valid operators for the selected <see cref="SearchField"/>. 
        /// Ensures that the user can only select a valid operator to search the selected field. If an invalid field 
        /// is selected or no field is selected, the operators list will be empty.
        /// </summary>
        private void DisplayValidSQLOperators()
        {
            DatabaseField selectedField = _searchField ?? DatabaseField.Invalid;

            //Ensure no SQLOperators are available for selection is a field has not been selected.
            if (selectedField == DatabaseField.Invalid)
            {
                SqlOperatorsList = [];
                return;
            }

            if (_fieldsRequiringTextParameters.Contains(selectedField))
            {
                SqlOperatorsList = _operatorsForText;
                return;
            }

            if (_fieldsRequiringIntegerParameters.Contains(selectedField))
            {
                SqlOperatorsList = _operatorsForIntegers;
                return;
            }

            SqlOperatorsList = [];
        }

        /// <summary>
        /// Resets the status message after a given delay to ensure that messages do not remain displayed indefinitely.
        /// </summary>
        private async Task ClearStatusMessageAfterDelay()
        {
            await Task.Delay(StatusMessageDisplayTime);
            StatusMessage = string.Empty;
        }

        //Validation methods

        /// <summary>
        /// Checks if a string is safe from SQL injection by allowing only alphanumeric characters, whitespace, hyphens, 
        /// and underscores. Used to validate user input before processing.
        /// </summary>
        /// <param name="input">The string to be validated.</param>
        /// <returns><see langword="true"/> if the <paramref name="input"/> <see langword="string"/> is safe from 
        /// SQL injection. <see langword="false"/> if there is a SQL injection risk.</returns>
        private static bool IsSqlInjectionSafe(string input)
        {
            return input.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '_');
        }

        /// <summary>
        /// Checks that a <see langword="string"/> input for a <see cref="AdvancedStudentSearch"/> parameter is valid for 
        /// the selected search operator, i.e. an integer value has been entered where this is required. Ensures that the 
        /// search will not fail due to an invalid parameter.
        /// </summary>
        /// <param name="parameter">The input <see langword="string"/> to be validated.</param>
        /// <returns><see langword="true"/> if the input is of the correct type. <see langword="false"/> if the input is 
        /// not of the correct type.</returns>
        private bool IsValidParameterForOperator(string parameter)
        {
            bool isIntegerParameter = int.TryParse(parameter, out _);
            SQLOperator selectedOperator = _sqlOperator ?? SQLOperator.Invalid;

            if (selectedOperator == SQLOperator.Invalid) //Cannot check if a parameter if valid for a null operator.
                return false;

            return (isIntegerParameter && _operatorsForIntegers.Contains(selectedOperator)) 
                || (!isIntegerParameter && _operatorsForText.Contains(selectedOperator));
        }
    }
}

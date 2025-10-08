using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Enums;
using SchoolBookingApp.MVVM.Model;
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
        private const string SearchErrorMessage = "An error occurred during the search. Please try again.";
        private const string InvalidInputMessage = "Invalid character attempted in input.";

        //Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly IReadOperationService _readOperationService;

        //Keyword search fields
        private string _searchText;
        private List<SearchResult> _searchResults;
        private List<Student> _advancedStudentSearchResults;
        private string _tableName;
        private readonly List<string> _tablesList;
        private string _field;
        private readonly Dictionary<string, List<string>> _fieldsDictionary;
        private readonly List<string> _fieldsList;

        //Advanced student search fields
        private DatabaseField? _databaseField;
        private readonly List<DatabaseField> _databaseFieldsList;
        private SQLOperator? _sqlOperator;
        private readonly List<SQLOperator> _sqlOperatorsList;
        private string _mainParameter;
        private string _secondaryParameter;

        private string _statusMessage;
        private bool _isAdvancedStudentSearch;

        //Command fields

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
        public List<Student> AdvancedStudentSearchResults
        {
            get => _advancedStudentSearchResults;
            set => SetProperty(ref _advancedStudentSearchResults, value);
        }
        public string TableName
        {
            get => _tableName;
            set => SetProperty(ref _tableName, value);
        }
        public string Field
        {
            get => _field;
            set => SetProperty(ref _field, value);
        }
        public List<string> TablesList => _tablesList;
        public List<string> FieldsList => _fieldsList;

        //Advanced student search properties
        public DatabaseField? DatabaseField
        {
            get => _databaseField;
            set => SetProperty(ref _databaseField, value);
        }
        public List<DatabaseField> DatabaseFieldsList => _databaseFieldsList;
        public SQLOperator? SqlOperator
        {
            get => _sqlOperator;
            set => SetProperty(ref _sqlOperator, value);
        }
        public List<SQLOperator> SqlOperatorsList => _sqlOperatorsList;
        public string MainParameter
        {
            get => _mainParameter;
            set
            {
                if (IsSqlInjectionSafe(value))
                    SetProperty(ref _mainParameter, value);
                else
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
                else
                {
                    OnPropertyChanged(nameof(SecondaryParameter));
                    StatusMessage = InvalidInputMessage;
                }
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
            }
        }

        //Constructor
        public SearchViewModel(IEventAggregator eventAggregator, IReadOperationService readOperationService)
        {
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _readOperationService = readOperationService 
                ?? throw new ArgumentNullException(nameof(readOperationService));

            _searchText = string.Empty;
            _searchResults = _readOperationService.GetAllSearchData().GetAwaiter().GetResult();
            _advancedStudentSearchResults = [];
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

            _databaseField = null;
            _databaseFieldsList = Enum.GetValues(typeof(DatabaseField))
                .Cast<DatabaseField>()
                .ToList();
            _sqlOperator = null;
            _sqlOperatorsList = Enum.GetValues(typeof(SQLOperator))
                .Cast<SQLOperator>()
                .ToList();
            _mainParameter = string.Empty;
            _secondaryParameter = string.Empty;

            _statusMessage = string.Empty;
            _isAdvancedStudentSearch = false;
        }

        //Search methods

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


        //Display property methods

        private void ResetKeywordSearchProperties()
        {

        }

        private void ResetAdvancedStudentSearchProperties()
        {

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

        private bool IsValidParameterForOperator(string parameter)
        {
            return true; // Placeholder for actual validation logic
        }
    }
}

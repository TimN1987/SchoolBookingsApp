using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Enums;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class SearchViewModel : ViewModelBase
    {
        //Constant status messages
        private const int StatusMessageDisplayTime = 2500; //in milliseconds
        private const string InvalidFieldMessage = "Ensure that a ";
        private const string InvalidTableMessage = "Ensure that a search category is selected.";
        private const string DataMissingMessage = "Ensure that all required fields are filled.";

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

        //Advanced student search properties

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                SetProperty(ref _statusMessage, value);
                Task.Run(async () => await ClearStatusMessageAfterDelay());
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
                StatusMessage = InvalidFieldMessage + "search field is selected.";
                return;
            }

            try
            {
                List<SearchResult> results = await _readOperationService.SearchByKeyword(_searchText, _tableName, _field);
            }
            catch (Exception ex)
            {
                StatusMessage = DataMissingMessage;
            }
        }


        //Display property methods


        /// <summary>
        /// Resets the status message after a given delay to ensure that messages do not remain displayed indefinitely.
        /// </summary>
        private async Task ClearStatusMessageAfterDelay()
        {
            await Task.Delay(StatusMessageDisplayTime);
            StatusMessage = string.Empty;
        }

        //Validation methods

        private static bool IsSqlInjectionSafe(string input)
        {
            return input.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '_');
        }
    }
}

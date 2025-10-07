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
        //Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly IReadOperationService _readOperationService;

        //Search fields
        private string _searchText;
        private List<SearchResult> _searchResults;
        private List<Student> _advancedStudentSearchResults;
        private string _tableName;
        private string _field;
        private DatabaseField? _databaseField;
        private readonly List<DatabaseField> _databaseFieldsList;
        private SQLOperator? _sqlOperator;
        private readonly List<SQLOperator> _sqlOperatorsList;
        private object _mainParameter;
        private object _secondaryParameter;
        private bool _isAdvancedStudentSearch;

        //Constructor
        public SearchViewModel(IEventAggregator eventAggregator, IReadOperationService readOperationService)
        {
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _readOperationService = readOperationService 
                ?? throw new ArgumentNullException(nameof(readOperationService));

            _searchText = string.Empty;
            _searchResults = new List<SearchResult>();
            _advancedStudentSearchResults = new List<Student>();
            _tableName = string.Empty;
            _field = string.Empty;
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
            _isAdvancedStudentSearch = false;
        }
    }
}

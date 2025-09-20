using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddParentViewModel : ViewModelBase
    {
        //Constants
        private const int StatusMessageDisplayTime = 2500;
        private const string InvalidFirstNameMessage = "First name cannot contain invalid characters.";
        private const string InvalidLastNameMessage = "Last name cannot contain invalid characters.";
        private const string InvalidRelationshipMessage = "Relationship cannot contain invalid characters.";

        //UI Label Properties
        public string AddParentTitle => IsNewParent ? "Add Parent" : "Update Parent";
        public static string SelectParentLabel => "Select Parent:";
        public static string SelectStudentLabel => "Select Student:";
        public static string FirstNameLabel => "First Name:";
        public static string LastNameLabel => "Last Name:";
        public static string AddChildHeading => "Add Child:";
        public static string ChildNameLabel => "Child Name:";
        public static string RelationshipLabel => "Relationship:";
        public static string AssignedChildrenLabel => "Assigned Children:";
        public string AddChildButtonLabel => IsAssignedStudentSelected ? "Update Child" : "Add Child";
        public string AddUpdateParentButtonLabel => IsNewParent ? "Add Parent" : "Update Parent";
        public static string ClearFormsButtonLabel => "Clear Forms";
        public static string DeleteParentButtonLabel => "Delete Parent";

        //Fields
        private readonly IReadOperationService _readOperationService;
        private readonly ICreateOperationService _createOperationService;
        private readonly IUpdateOperationService _updateOperationService;
        private readonly IDeleteOperationService _deleteOperationService;

        private bool _isNewParent;
        private bool _isAssignedStudentSelected;
        private string _statusMessage;

        //Fields for parent details.
        private List<SearchResult> _allParents;
        private SearchResult? _selectedParent;
        private List<SearchResult> _allStudents;
        private SearchResult? _selectedUnassignedStudent;

        private string _firstName;
        private string _lastName;
        private string _childName;
        private string _relationship;
        private List<(Student child, string relationship)> _children;
        private Student? _selectedAssignedChild;

        //Commands
        private ICommand? _addUpdateParentCommand;
        private ICommand? _deleteParentCommand;
        private ICommand? _clearFormsCommand;
        private ICommand? _addRelationshipCommand;

        //Properties
        public bool IsNewParent
        {
            get => _isNewParent;
            set
            {
                SetProperty(ref _isNewParent, value);
                OnPropertyChanged(nameof(AddParentTitle));
            }
        }
        public bool IsAssignedStudentSelected
        {
            get => _isAssignedStudentSelected;
            set
            {
                SetProperty(ref _isAssignedStudentSelected, value);
                OnPropertyChanged(nameof(AddChildButtonLabel));
            }
        }
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                SetProperty(ref _statusMessage, value);

                if (value != string.Empty)
                    Task.Run(async () => await RemoveMessageAfterDelay());
            }
        }

        //Properties for parent details.

        public List<SearchResult> AllParents
        {
            get => _allParents;
            set => SetProperty(ref _allParents, value);
        }
        public SearchResult? SelectedParent
        {
            get => _selectedParent;
            set => SetProperty(ref _selectedParent, value);
        }
        public List<SearchResult> AllStudents
        {
            get => _allStudents;
            set => SetProperty(ref _allStudents, value);
        }
        public SearchResult? SelectedUnassignedStudent
        {
            get => _selectedUnassignedStudent;
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedUnassignedStudent, null);
                    return;
                }
                
                SetProperty(ref _selectedUnassignedStudent, value);
                //CHECK - is this student already assigned ??? add new method
                DisplayStudentDetails();
                IsAssignedStudentSelected = false;
                SelectedAssignedChild = null;
            }
        }
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (IsSqlInjectionSafe(value))
                    SetProperty(ref _firstName, value);
                else
                {
                    StatusMessage = InvalidFirstNameMessage;
                    OnPropertyChanged(nameof(FirstName));
                }
            }
        }
        public string LastName
        {
            get => _lastName;
            set
            {
                if (IsSqlInjectionSafe(value))
                    SetProperty(ref _lastName, value);
                else
                {
                    StatusMessage = InvalidLastNameMessage;
                    OnPropertyChanged(nameof(LastName));
                }
            }
        }
        public string ChildName
        {
            get => _childName;
            set => SetProperty(ref _childName, value);
        }
        public string Relationship
        {
            get => _relationship;
            set
            {
                if (IsSqlInjectionSafe(value))
                    SetProperty(ref _relationship, value);
                else
                {
                    StatusMessage = InvalidRelationshipMessage;
                    OnPropertyChanged(nameof(Relationship));
                }
            }
        }
        public List<(Student child, string relationship)> Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }
        public Student? SelectedAssignedChild
        {
            get => _selectedAssignedChild;
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedAssignedChild, null);
                    return;
                }

                SetProperty(ref _selectedAssignedChild, value);

                DisplayStudentDetails();
                IsAssignedStudentSelected = true;
                SelectedAssignedChild = null;
            }
        }

        //Constructor

        public AddParentViewModel(
            IReadOperationService readOperationService, 
            ICreateOperationService createOperationService, 
            IUpdateOperationService updateOperationService, 
            IDeleteOperationService deleteOperationService)
        {
            _readOperationService = readOperationService 
                ?? throw new ArgumentNullException(nameof(readOperationService));
            _createOperationService = createOperationService 
                ?? throw new ArgumentNullException(nameof(createOperationService));
            _updateOperationService = updateOperationService 
                ?? throw new ArgumentNullException (nameof(updateOperationService));
            _deleteOperationService = deleteOperationService 
                ?? throw new ArgumentNullException(nameof(deleteOperationService));

            _isNewParent = true;
            _isAssignedStudentSelected = false;
            _statusMessage = string.Empty;

            _allParents = [];
            _selectedParent = null;
            _allStudents = [];
            _selectedUnassignedStudent = null;

            _firstName = string.Empty;
            _lastName = string.Empty;
            _childName = string.Empty;
            _relationship = string.Empty;
            _children = [];
            _selectedAssignedChild = null;
        }

        //Methods

        //Private helper methods

        /// <summary>
        /// Clears the <see cref="StatusMessage"/> after a preset delay. Ensures that messages do not remain visible 
        /// indefinitely.
        /// </summary>
        private async Task RemoveMessageAfterDelay()
        {
            await Task.Delay(StatusMessageDisplayTime);
            StatusMessage = string.Empty;
        }

        /// <summary>
        /// Reloads the parent and student lists from the database to ensure that the information is correct and up-to-date. 
        /// Used on start up and after making changes to a parent to update information after changes.
        /// </summary>
        private async Task RefreshParentStudentLists()
        {
            AllParents = await _readOperationService.GetParentList();
            AllStudents = await _readOperationService.GetStudentList();

        }

        /// <summary>
        /// Ensures that the UI properties displaying parent details are updated when a new student is selected.
        /// </summary>
        private void DisplayStudentDetails()
        {
            if (_isAssignedStudentSelected && _selectedAssignedChild != null)
            {
                ChildName = $"{_selectedAssignedChild.FirstName} {_selectedAssignedChild.LastName}";
                //find relationship information ???
            }
            else if (!_isAssignedStudentSelected && _selectedUnassignedStudent != null)
            {
                ChildName = $"{_selectedUnassignedStudent?.FirstName} {_selectedUnassignedStudent?.LastName}";
                Relationship = string.Empty; 
            }
        }

        //Private static methods

        /// <summary>
        /// Verifies that a given string input is safe from SQL injection attacks by checking that it only contains 
        /// letters, digits, whitespace, hyphens, and underscores. Allows verification of user input before it is used to 
        /// add or update database records.
        /// </summary>
        /// <param name="input">The <see langword="string"/> to be checked.</param>
        /// <returns><c>true</c> if the <paramref name="input"/> is SQL injection safe. <c>false</c> if invalid characters 
        /// that could allow SQL injection attacks.</returns>
        private static bool IsSqlInjectionSafe(string input)
        {
            return input.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '_');
        }
    }
}

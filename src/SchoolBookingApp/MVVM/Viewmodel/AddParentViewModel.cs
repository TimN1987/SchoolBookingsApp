using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SchoolBookingApp.MVVM.Commands;
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
        private const string InvalidCharactersMessage = "Cannot add or update parent. Invalid characters found in information.";
        private const string InvalidParentSelectionMessage = "Invalid parent selected. Cannot retrieve data.";
        private const string MissingParentInformationMessage = "No information could be found for the selected parent.";
        private const string EmptyFieldsMessage = "Complete all information before adding or updating a parent.";
        private const string InvalidParentUpdateMessage = "Cannot update - the selected parent is invalid.";
        private const string RecordAddedMessage = "Parent added successfully.";
        private const string RecordUpdatedMessage = "Parent updated successfully.";
        private const string RecordDeletedMessage = "Parent deleted successfully.";
        private const string FailedToAddMessage = "Failed to add parent.";
        private const string FailedToUpdateMessage = "Failed to update parent.";
        private const string FailedToDeleteMessage = "Failed to delete parent.";

        //UI Label Properties
        public string AddParentTitle => IsNewParent ? "Add Parent" : "Update Parent";
        public static string SelectParentLabel => "Select Parent:";
        public static string SelectStudentLabel => "Select Student:";
        public static string FirstNameLabel => "First Name:";
        public static string LastNameLabel => "Last Name:";
        public string AddChildHeading => IsAssignedStudentSelected ? "Update Child" : "Add Child:";
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
                OnPropertyChanged(nameof(AddUpdateParentButtonLabel));
            }
        }
        public bool IsAssignedStudentSelected
        {
            get => _isAssignedStudentSelected;
            set
            {
                SetProperty(ref _isAssignedStudentSelected, value);
                OnPropertyChanged(nameof(AddChildHeading));
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
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedParent, null);
                    return;
                }

                SetProperty(ref _selectedParent, value);
                Task.Run(async () => await DisplayParentDetails());
            }
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

                if (IsAssignedChild(_selectedUnassignedStudent?.Id ?? 0))
                {
                    SetAssignedChild(_selectedUnassignedStudent?.Id ?? 0);
                    IsAssignedStudentSelected = true;
                    SetProperty(ref _selectedUnassignedStudent, null);
                }
                else
                {
                    IsAssignedStudentSelected = false;
                    SelectedAssignedChild = null;
                }

                DisplayStudentDetails();
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
                SelectedUnassignedStudent = null;
            }
        }

        //Commands
        public ICommand? AddUpdateParentCommand => _addUpdateParentCommand
            ??= new RelayCommand(async param => await AddUpdateParent());

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

        /// <summary>
        /// Adds or updates the current parent with the given information. Called from the <see cref="AddUpdateParentCommand"/> 
        /// to allow easy saving of parent information.
        /// </summary>
        /// <remarks>Contains validation to ensure that inputs are SQL injection safe and are not null, empty or 
        /// whitespace.</remarks>
        public async Task AddUpdateParent()
        {
            if (string.IsNullOrWhiteSpace(_firstName)
                || string.IsNullOrWhiteSpace(_lastName))
            {
                StatusMessage = EmptyFieldsMessage;
                return;
            }

            if (!IsSqlInjectionSafe(_firstName)
                || !IsSqlInjectionSafe(_lastName))
            {
                StatusMessage = InvalidCharactersMessage;
                return;
            }

            if (IsNewParent)
                await AddNewParent();
            else
                await UpdateCurrentParent();
        }

        //Private helper methods

        /// <summary>
        /// Creates a new record in the database with the current informaiton. Allows the user to add a new parent with a 
        /// button click.
        /// </summary>
        private async Task AddNewParent()
        {
            List<(int id, string relationship)> children = PrepareChildrenListForDatabase();

            bool updatedSuccessfully = await _createOperationService.AddParent(
                _firstName, _lastName, children);

            StatusMessage = updatedSuccessfully ? RecordAddedMessage : FailedToAddMessage;
            ResetChildrenSelection();
        }

        /// <summary>
        /// Updates the database record for the selected parent with the current information. Allows the user to update a 
        /// database record for a parent with corrected or updated information.
        /// </summary>
        private async Task UpdateCurrentParent()
        {
            int id = _selectedParent?.Id ?? 0;
            
            if (_selectedParent == null || id <= 0)
            {
                StatusMessage = InvalidParentUpdateMessage;
                return;
            }

            List<(int id, string relationship)> children = PrepareChildrenListForDatabase();

            bool updatedSuccessfully = await _updateOperationService.UpdateParent(
                id, _firstName, _lastName, children);

            StatusMessage = updatedSuccessfully ? RecordUpdatedMessage : FailedToUpdateMessage;
            ResetChildrenSelection();
        }

        /// <summary>
        /// Extracts the key data from the children list for storing in the database. Returns a list in the form that is 
        /// needed for the update method parameter.
        /// </summary>
        /// <returns>A list of Tuples containing the id number for each child with the relationship.</returns>
        private List<(int, string)> PrepareChildrenListForDatabase()
        {
            List<(int id, string relationship)> children = _children
                .Select(child => (child.child.Id, child.relationship))
                .ToList();

            return children;
        }

        /// <summary>
        /// Ensures that no child is selected or displayed after a child is added or updated, or a parent is updated. 
        /// Avoids the focus remaining on child selection and addition.
        /// </summary>
        private void ResetChildrenSelection()
        {
            SelectedAssignedChild = null;
            SelectedUnassignedStudent = null;
            ChildName = string.Empty;
            Relationship = string.Empty;
        }

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
        /// Displays the parent information for a selected <see cref="Parent"/>. Used to update the UI Properties when a 
        /// new parent is selected to ensure correct information is displayed.
        /// </summary>
        private async Task DisplayParentDetails()
        {
            Parent? parentInformation = await GetParentInformation();

            if (parentInformation == null)
            {
                StatusMessage = MissingParentInformationMessage;
                SelectedParent = null;
                IsNewParent = true;
                return;
            }

            FirstName = parentInformation?.FirstName ?? string.Empty;
            LastName = parentInformation?.LastName ?? string.Empty;

            Children = await GetChildrenList(parentInformation);

            SelectedAssignedChild = null;
            SelectedUnassignedStudent = null;
            IsNewParent = false;
        }

        /// <summary>
        /// Retrieves the parent information stored in the database for the <see cref="SelectedParent"/>. Used to update 
        /// the UI Properties with the correct information when the user selects a pre-existing parent to display.
        /// </summary>
        /// <returns>A nullable <see cref="Parent"/> instance containing the information for the <see cref="SelectedParent"/>. 
        /// Returns <see langword="null"/> if the <see cref="SelectedParent"/> is <see langword="null"/> or has an 
        /// invalid id number that cannot be retrieved from the database.</returns>
        private async Task<Parent?> GetParentInformation()
        {
            int id = _selectedParent?.Id ?? 0;

            if (_selectedParent == null || id <= 0)
            {
                StatusMessage = InvalidParentSelectionMessage;
                return null;
            }

            return await _readOperationService.GetParentInformation(id);
        }

        /// <summary>
        /// Retrieves the <see cref="Student"/> information for each child on the children list for a <see cref="Parent"/>. 
        /// Used to retrieve necessary <see cref="Student"/> information to set the UI properties and display the correct 
        /// data for each child.
        /// </summary>
        /// <param name="parentInformation">The <see cref="Parent"/> instance containing information about the given 
        /// parent.</param>
        /// <returns>A list of <see cref="Student"/> objects representing each child on the list, with a <see 
        /// langword="string"/> representation of the relationship to the parent.</returns>
        private async Task<List<(Student, string)>> GetChildrenList(Parent? parentInformation)
        {
            List<(Student, string)> children = [];

            foreach ((int id, string relationship) in parentInformation?.Children ?? [])
            {
                if (id <= 0)
                    continue;

                Student student = await _readOperationService.GetStudentData(id);

                if (student.FirstName == "No student found")
                    continue;

                children.Add((student, relationship));
            }

            return children;
        }

        /// <summary>
        /// Ensures that the UI properties displaying parent details are updated when a new student is selected.
        /// </summary>
        private void DisplayStudentDetails()
        {
            if (_isAssignedStudentSelected && _selectedAssignedChild != null)
            {
                ChildName = $"{_selectedAssignedChild.FirstName} {_selectedAssignedChild.LastName}";

                //Find the relationship for the selected child from the Children list.
                string relationship = _children
                    .Where(child => child.child.Id == _selectedAssignedChild.Id)
                    .Select(child => child.relationship)
                    .FirstOrDefault()
                    ?? string.Empty;

                Relationship = relationship;
            }
            else if (!_isAssignedStudentSelected && _selectedUnassignedStudent != null)
            {
                ChildName = $"{_selectedUnassignedStudent?.FirstName} {_selectedUnassignedStudent?.LastName}";
                Relationship = string.Empty; 
            }
        }

        /// <summary>
        /// Checks if a given student id number is contained in the <see cref="Children"/> list for the currently selected 
        /// <see cref="Parent"/>. Used to check if the <see cref="SelectedUnassignedStudent"/> has already been added to 
        /// the <see cref="Children"/> list. Ensures that a child is treated as assigned if it already has been, avoiding 
        /// the risk of adding the same child to the <see cref="Children"/> list multiple times.
        /// </summary>
        /// <param name="id">The id number for the <see cref="SelectedUnassignedStudent"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="SelectedUnassignedStudent"/> has already been assigned to the 
        /// selected <see cref="Parent"/>. <see langword="false"/> if the <see cref="SelectedUnassignedStudent"/> has not 
        /// been assigned yet.</returns>
        private bool IsAssignedChild(int id)
        {
            var assignedIdHashSet = _children
                .Select(child => child.child.Id)
                .ToHashSet();
            return assignedIdHashSet.Contains(id);
        }

        /// <summary>
        /// Sets the <see cref="SelectedAssignedChild"/> to the <see cref="Student"/> with the given <paramref name="id"/>. 
        /// Used to set the correct student as the <see cref="SelectedAssignedChild"/> when a <see 
        /// cref="SelectedUnassignedStudent"/> is selected that already appears on the <see cref="Children"/> list. Ensures 
        /// that only one student can be selected at a time to avoid UI confusion.
        /// </summary>
        /// <param name="id">The id number of the selected student.</param>
        private void SetAssignedChild(int id)
        {
            var assignedStudent = _children
                .Where(child => child.child.Id == id)
                .Select(child => child.child)
                .FirstOrDefault();
            SelectedAssignedChild = assignedStudent;
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

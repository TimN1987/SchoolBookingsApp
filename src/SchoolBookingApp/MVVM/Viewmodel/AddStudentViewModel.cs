using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddStudentViewModel : ViewModelBase
    {
        private readonly IReadOperationService _readOperationService;
        private readonly ICreateOperationService _createOperationService;
        private readonly IUpdateOperationService _updateOperationService;
        private readonly IDeleteOperationService _deleteOperationService;

        private bool _isNewStudent;
        private bool _isCurrentParent;

        // Fields for student details.
        private List<SearchResult> _allStudents;
        private readonly List<SearchResult> _allParents;

        private SearchResult? _selectedStudent;
        private string _firstName;
        private string _lastName;
        private DateTime _dateOfBirth;
        private string _className;
        private List<(Parent, string)> _parents;
        private string _parentName;
        private string _relationship;
        private SearchResult? _selectedNewParent;
        private (Parent parentDetails, string relationship)? _selectedCurrentParent;

        //Properties
        public bool IsNewStudent
        {
            get => _isNewStudent;
            set => SetProperty(ref _isNewStudent, value);
        }
        public bool IsCurrentParent
        {
            get => _isCurrentParent;
            set => SetProperty(ref _isCurrentParent, value);
        }

        // Properties for student details.
        public List<SearchResult> AllStudents
        {
            get => _allStudents;
            set => SetProperty(ref _allStudents, value);
        }
        public List<SearchResult> AllParents => _allParents;
        public SearchResult? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (value.Equals(_selectedStudent))
                    return;

                SetProperty(ref _selectedStudent, value);
                IsNewStudent = false;
                Task.Run(async () => await DisplayStudentDetails());
            }
        }
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }
        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }
        public string ClassName
        {
            get => _className;
            set => SetProperty(ref _className, value);
        }
        public List<(Parent, string)> Parents
        {
            get => _parents;
            set => SetProperty(ref _parents, value);
        }
        public string ParentName
        {
            get => _parentName;
            set => SetProperty(ref _parentName, value);
        }
        public string Relationship
        {
            get => _relationship;
            set => SetProperty(ref _relationship, value);
        }
        public SearchResult? SelectedNewParent
        {
            get => _selectedNewParent;
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedNewParent, null);
                    return;
                }

                if (value.Equals(_selectedNewParent))
                    return;

                SetProperty(ref _selectedNewParent, value);
                IsCurrentParent = IsParentAssignedToCurrentStudent();

                if (_isCurrentParent)
                    SetSelectedCurrentParent();
                else
                    SelectedCurrentParent = null; //Ensures that only one parent is selected to avoid UI confusion.
            }
        }
        public (Parent parentDetails, string relationship)? SelectedCurrentParent
        {
            get => _selectedCurrentParent;
            set
            {
                if (value == null)
                {
                    SetProperty(ref _selectedCurrentParent, null);
                    return;
                }

                if (value.Equals(_selectedCurrentParent))
                    return;

                SetProperty(ref _selectedCurrentParent, value);
                IsCurrentParent = true;
                DisplayCurrentParentDetails();
                SelectedNewParent = null; //Ensures that only one parent is selected to avoid UI confusion.
            }
        }


        //Constructor
        public AddStudentViewModel(
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
                ?? throw new ArgumentNullException(nameof(updateOperationService));
            _deleteOperationService = deleteOperationService 
                ?? throw new ArgumentNullException(nameof(deleteOperationService));

            _isNewStudent = true;
            _isCurrentParent = true;

            _firstName = string.Empty;
            _lastName = string.Empty;
            _dateOfBirth = DateTime.MinValue;
            _className = string.Empty;
            _parentName = string.Empty;
            _relationship = string.Empty;
            _parents = [];

            _allStudents = _readOperationService.GetStudentList().GetAwaiter().GetResult();
            _allParents = _readOperationService.GetParentList().GetAwaiter().GetResult();
        }

        //Methods

        /// <summary>
        /// Updates the UI properties to display the student information when a new student is selected from the list of 
        /// previously added student. Allows for simple editing of exisiting students.
        public async Task DisplayStudentDetails()
        {
            if (_selectedStudent == null || _selectedStudent?.Id <= 0)
                return;

            Student? selectedStudent = await GetStudentDetails();

            if (selectedStudent == null)
                return;

            FirstName = selectedStudent.FirstName;
            LastName = selectedStudent.LastName;
            Parents = selectedStudent.Parents;
        }

        /// <summary>
        /// Updates the UI properties to display the name and relationship for a selected parent who is currently assigned 
        /// to the selected student.
        /// </summary>
        public void DisplayCurrentParentDetails()
        {
            if (_selectedCurrentParent == null || _selectedCurrentParent?.parentDetails.Id <= 0)
                return;

            ParentName = 
                $"{_selectedCurrentParent?.parentDetails.FirstName ?? ""} {_selectedCurrentParent?.parentDetails.LastName ?? ""}";
            Relationship = _selectedCurrentParent?.relationship ?? "";
        }

        /// <summary>
        /// Checks if the list of parents already assigned to the currently selected student contains the new parent 
        /// selected from the <see cref="AllParents"/> list. Used to avoid the option to add the same parent multiple times.
        /// </summary>
        /// <returns><c>true</c> if the parent has already been assigned to the selected student. <c>false</c> if the 
        /// parent has not yet been assigned to the selected student.</returns>
        public bool IsParentAssignedToCurrentStudent()
        {
            int selectedParentId = _selectedNewParent?.Id ?? 0;
            List<int> assignedParentIds = _parents
                .Select(parent => parent.Item1.Id)
                .ToList();

            return assignedParentIds.Contains(selectedParentId);
        }

        /// <summary>
        /// Updates the <see cref="SelectedCurrentParent"/> property given the parent id for a newly selected parent who 
        /// has already been assigned to the selected student. Used to ensure that a parent cannot be assigned to a 
        /// student multiple times, and that the parent details are available for editing when selected.
        /// </summary>
        public void SetSelectedCurrentParent()
        {
            int selectedParentId = _selectedNewParent?.Id ?? 0;
            (Parent, string) selectedParent = _parents
                .Where(parent => parent.Item1.Id == selectedParentId)
                .FirstOrDefault();

            SelectedCurrentParent = selectedParent;
        }

        /// <summary>
        /// Clears the current entries and resets the selected student to allow a new student to be added.
        /// </summary>
        public void ClearForms()
        {
            IsNewStudent = true;
            
            FirstName = string.Empty;
            LastName = string.Empty;
            DateOfBirth = DateTime.MinValue;
            ClassName = string.Empty;
            ParentName = string.Empty;
            Relationship = string.Empty;
            Parents = [];
        }

        //Private helper methods

        /// <summary>
        /// Gets the student information for the selected student to ensure that the user interface is updated to reflect 
        /// the currently selected student.
        /// </summary>
        /// <returns>A <see cref="Student"/> object containing the relevant student data from the database.</returns>
        private async Task<Student?> GetStudentDetails()
        {
            if (_selectedStudent == null)
                return null;

            int studentId = _selectedStudent?.Id ?? 0;

            if (studentId == 0)
                return null;

            return await _readOperationService.GetStudentData(studentId);
        }



        // Private static helper methods

        /// <summary>
        /// Converts a <see cref="DateTime"/> into an integer representation. Used to store the date of birth in the expected 
        /// form for the database.
        /// </summary>
        /// <param name="dateOfBirth">The date of birth in <see cref="DateTime"/> form.</param>
        /// <returns>The date of birth in <see langword="int"/> form.</returns>
        
        private static int DateTimeToInt(DateTime dateOfBirth)
        {
            return int.Parse(dateOfBirth.ToString("yyyyMMdd"));
        }
        /// <summary>
        /// Converts an <see langword="int"/> into a <see cref="DateTime"/> representation. Used to retrieve a date of birth 
        /// stored in the database in integer form for ease of use in the viewmodel.
        /// </summary>
        /// <param name="dateOfBirth">The date of birth in integer form.</param>
        /// <returns>The date of birth as a <see cref="DateTime"/> object.</returns>
        private static DateTime IntToDateTime(int dateOfBirth)
        {
            var stringDateForm = dateOfBirth.ToString();
            return DateTime.ParseExact(stringDateForm, "yyyyMMdd", null);
        }
    }
}

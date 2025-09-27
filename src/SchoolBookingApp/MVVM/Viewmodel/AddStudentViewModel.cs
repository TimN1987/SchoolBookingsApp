using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using SchoolBookingApp.MVVM.Commands;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddStudentViewModel : ViewModelBase
    {
        //Constants
        private const string InvalidFirstNameMessage = "First name contains invalid characters.";
        private const string InvalidLastNameMessage = "Last name contains invalid characters.";
        private const string InvalidClassNameMessage = "Class name contains invalid characters.";
        private const string RecordAddedMessage = "Student added successfully.";
        private const string RecordUpdatedMessage = "Student updated successfully.";
        private const string RecordDeletedMessage = "Student deleted successfully.";
        private const string FailedToAddMessage = "Failed to add student.";
        private const string FailedToUpdateMessage = "Failed to update student.";
        private const string FailedToDeleteMessage = "Failed to delete student.";
        private const string MissingFieldsMessage = "Not all student information is set.";
        private const string InvalidStudentSelectedMessage = "The selected student cannot be updated.";
        private const int MessageDisplayTime = 2000;
        private const string StudentTableName = "Students";

        //UI Label Properties
        public string AddStudentTitle => IsNewStudent ? "Add Student" : "Update Student";
        public static string SelectStudentLabel => "Select Student:";
        public static string FirstNameLabel => "First Name:";
        public static string LastNameLabel => "Last Name:";
        public static string ClassNameLabel => "Class Name:";
        public static string DateOfBirthLabel => "Date of Birth:";
        public static string ParentsLabel => "Parents:";
        public string AddUpdateButtonLabel => IsNewStudent ? "Add Student" : "Update Student";
        public static string ClearFormsButtonLabel => "Clear Forms";
        public static string DeleteButtonLabel => "Delete Student";

        //Fields
        private readonly IReadOperationService _readOperationService;
        private readonly ICreateOperationService _createOperationService;
        private readonly IUpdateOperationService _updateOperationService;
        private readonly IDeleteOperationService _deleteOperationService;

        private bool _isNewStudent;
        private string _statusMessage;

        // Fields for student details.
        private List<SearchResult> _allStudents;
        private SearchResult? _selectedStudent;
        private string _firstName;
        private string _lastName;
        private DateTime _dateOfBirth;
        private string _className;
        private List<(Parent, string)> _parents;

        //Commands
        private ICommand? _addUpdateStudentCommand;
        private ICommand? _deleteStudentCommand;
        private ICommand? _clearFormsCommand;

        //Properties
        public bool IsNewStudent
        {
            get => _isNewStudent;
            set
            {
                SetProperty(ref _isNewStudent, value);
                OnPropertyChanged(nameof(AddStudentTitle));
                OnPropertyChanged(nameof(AddUpdateButtonLabel));
            }
        } 
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                SetProperty(ref _statusMessage, value);
                
                if (value != string.Empty)
                    Task.Run(async () => await DelayMessageRemoval());
            }
        }

        // Properties for student details.
        public List<SearchResult> AllStudents
        {
            get => _allStudents;
            set
            {
                List<SearchResult> orderedStudents = value
                    .OrderBy(student => student.LastName)
                    .ThenBy(student => student.FirstName)
                    .ToList();
                SetProperty(ref _allStudents, orderedStudents);
            }
        }
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
                    OnPropertyChanged(LastName);
                }
            }
        }
        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }
        public string ClassName
        {
            get => _className;
            set
            {
                if (IsSqlInjectionSafe(value))
                    SetProperty(ref _className, value);
                else
                {
                    StatusMessage = InvalidClassNameMessage;
                    OnPropertyChanged(nameof(ClassName));
                }
            }
        }
        public List<(Parent, string)> Parents
        {
            get => _parents;
            set => SetProperty(ref _parents, value);
        }

        //Commands

        public ICommand? AddUpdateStudentCommand
        {
            get => _addUpdateStudentCommand ??= new RelayCommand(async param => await AddUpdateStudent());
        }
        public ICommand? DeleteStudentCommand
        {
            get => _deleteStudentCommand ??= new RelayCommand(async param => await DeleteStudent());
        }
        public ICommand? ClearFormsCommand
        {
            get => _clearFormsCommand ??= new RelayCommand(param => ClearForms());
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
            _statusMessage = string.Empty;

            _firstName = string.Empty;
            _lastName = string.Empty;
            _dateOfBirth = DateTime.Now;
            _className = string.Empty;
            _parents = [];
            _allStudents = [];

            Task.Run(async () => await SetStudentSelectionList());
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
            DateOfBirth = IntToDateTime(selectedStudent.DateOfBirth);
            ClassName = selectedStudent.ClassName;
            Parents = selectedStudent.Parents;
        }

        /// <summary>
        /// Adds or updates a student record in the database based on the current entries in the user interface. Validates 
        /// that the entries are complete and safe from SQL injection before attempting to add or update the record. USed 
        /// to easily handle the creation of new students and the updating of existing students through a single command.
        /// </summary>
        public async Task AddUpdateStudent()
        {
            if (string.IsNullOrWhiteSpace(FirstName)
                || string.IsNullOrWhiteSpace(LastName)
                || string.IsNullOrWhiteSpace(ClassName))
            {
                StatusMessage = MissingFieldsMessage;
                return;
            }

            if (!IsSqlInjectionSafe(FirstName)
                || !IsSqlInjectionSafe(LastName)
                || !IsSqlInjectionSafe(ClassName))
            {
                StatusMessage = MissingFieldsMessage;
                return;
            }

            if (DateOfBirth == DateTime.MinValue)
            {
                StatusMessage = MissingFieldsMessage;
                return;
            }

            if (!IsNewStudent && (_selectedStudent == null || _selectedStudent?.Id <= 0))
            {
                StatusMessage = InvalidStudentSelectedMessage;
                return;
            }

            int id = _selectedStudent?.Id ?? 0;
            int dateOfBirthInt = DateTimeToInt(DateOfBirth);

            bool operationSuccess;

            if (IsNewStudent)
            {
                operationSuccess = 
                    await _createOperationService.AddStudent(FirstName, LastName, dateOfBirthInt, ClassName);
            }
            else
            {
                operationSuccess = 
                    await _updateOperationService.UpdateStudent(id, FirstName, LastName, dateOfBirthInt, ClassName);
            }

            StatusMessage = operationSuccess
                ? (IsNewStudent ? RecordAddedMessage : RecordUpdatedMessage)
                : (IsNewStudent ? FailedToAddMessage : FailedToUpdateMessage);
            await SetStudentSelectionList(); //Ensure the student selection list is up to date after changes.
        }

        /// <summary>
        /// Deletes the currently selected student from the database. Validates that a student is selected and that it is 
        /// not <see langword="null"/>, not a new student, and has a valid ID before attempting to delete the record. 
        /// Refreshes the list of students and clears the forms after deletion to ensure that the user interface is up to 
        /// date. Allows for easy removal of students from the database.
        /// </summary>
        public async Task DeleteStudent()
        {
            if (IsNewStudent || _selectedStudent == null || _selectedStudent?.Id <= 0)
                return;

            int studentId = _selectedStudent?.Id ?? 0;

            if (studentId == 0)
                return;

            bool operationSuccess = await _deleteOperationService.DeleteRecord(StudentTableName, studentId);

            StatusMessage = operationSuccess ? RecordDeletedMessage : FailedToDeleteMessage;

            // Refresh the student list and clear the forms.
            await SetStudentSelectionList();
            ClearForms();
        }

        /// <summary>
        /// Clears the current entries and resets the selected student to allow a new student to be added.
        /// </summary>
        public void ClearForms()
        {
            IsNewStudent = true;
            
            FirstName = string.Empty;
            LastName = string.Empty;
            DateOfBirth = DateTime.Now;
            ClassName = string.Empty;
            Parents = [];
        }

        //Private helper methods

        /// <summary>
        /// Loads the student list and sets the <see cref="AllStudents"/> property to this updated list of students. Used 
        /// to ensure that the list is loaded lazily on startup and for refreshing the student list after changes are made.
        /// </summary>
        private async Task SetStudentSelectionList()
        {
            AllStudents = await _readOperationService.GetStudentList();
        }

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

        /// <summary>
        /// Creates a delay before clearing the status message to allow the user time to read it without displaying the 
        /// message indefinitely.
        /// </summary>
        private async Task DelayMessageRemoval()
        {
            await Task.Delay(MessageDisplayTime);
            StatusMessage = string.Empty;
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

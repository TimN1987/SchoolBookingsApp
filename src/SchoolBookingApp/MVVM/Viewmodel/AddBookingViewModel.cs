using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32.SafeHandles;
using Prism.Common;
using SchoolBookingApp.MVVM.Commands;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.Viewmodel.Base;
using Serilog;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddBookingViewModel : ViewModelBase
    {
        //Constant error messages
        private const string NoBookingDataMessage = "Complete all fields before adding booking.";
        private const string BookingAddedMessage = "Booking added successfully.";
        private const string BookingFailedToAddMessage = "Failed to add booking. Please try again.";
        private const string BookingUpdatedMessage = "Booking updated successfully.";
        private const string BookingFailedToUpdateMessage = "Failed to update booking. Please try again.";
        private const string BookingDeletedMessage = "Booking deleted successfully.";
        private const string BookingFailedToDeleteMessage = "Failed to delete booking. Please try again.";
        private const string SqlInjectionMessage = "Invalid characters in name fields.";
        private const string InvalidStudentDataFailedMessage = "Data can only be added if a student is selected.";
        private const string InvalidStudentCommentsFailedMessage = "Comments can only be added if a student is selected.";
        private const string DataFailedToAddMessage = "Failed to add data. Please try again.";
        private const string CommentsFailedToAddMessage = "Failed to add comments. Please try again.";
        private const string DataAddedMessage = "Data added successfully.";
        private const string CommentsAddedMessage = "Comments added successfully.";

        //UI labels
        public string PageTitle => IsNewBooking ? "Add New Booking" : "Update Booking";
        public static string FirstNameLabel => "First Name:";
        public static string LastNameLabel => "Last Name:";
        public static string DateTimeLabel => "Booking Date and Time:";
        public static string ParentsLabel => "Responsible adults:";
        public string AddUpdateButtonLabel => IsNewBooking ? "Add Booking" : "Update Booking";
        public static string DeleteBookingButtonLabel => "Delete Booking";
        public static string ClearFormButtonLabel => "Clear Form";
        public string ShowHideDataButtonLabel => IsBookingDataVisible ? "Hide Data" : "Show Data";
        public string ShowHideCommentsButtonLabel => IsCommentsVisible ? "Hide Comments" : "Show Comments";

        //Data information UI labels
        public static string MathLabel => "Math:";
        public static string MathCommentsLabel => "Math Comments:";
        public static string ReadingLabel => "Reading:";
        public static string ReadingCommentsLabel => "Reading Comments:";
        public static string WritingLabel => "Writing:";
        public static string WritingCommentsLabel => "Writing Comments:";
        public static string ScienceLabel => "Science:";
        public static string HistoryLabel => "History:";
        public static string GeographyLabel => "Geography:";
        public static string MFLLabel => "MFL:";
        public static string PELabel => "PE:";
        public static string ArtLabel => "Art:";
        public static string MusicLabel => "Music:";
        public static string RELabel => "RE:";
        public static string DesignTechnologyLabel => "Design Technology:";
        public static string ComputingLabel => "Computing:";
        public static string UpdateDataButtonLabel => "Update Data";

        //Comments UI labels
        public static string GeneralCommentsLabel => "General Comments:";
        public static string PupilCommentsLabel => "Pupil Comments:";
        public static string ParentCommentLabel => "Parent Comments:";
        public static string BehaviorNotesLabel => "Behavior Notes:";
        public static string AttendanceNotesLabel => "Attendance Notes:";
        public static string HomeworkNotesLabel => "Homework Notes:";
        public static string ExtraCurricularNotesLabel => "Extra-Curricular Notes:";
        public static string SpecialEducationalNeedsNotesLabel => "Special Educational Needs Notes:";
        public static string SafeguardingNotesLabel => "Safeguarding Notes:";
        public static string OtherNotesLabel => "Other Notes:";
        public static string UpdateCommentsButtonLabel => "Update Comments";

        //Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly IBookingManager _bookingManager;
        private readonly IReadOperationService _readOperationService;
        private readonly ICreateOperationService _createOperationService;
        private readonly IUpdateOperationService _updateOperationService;
        private readonly IDeleteOperationService _deleteOperationService;

        private Booking _booking;
        private SearchResult _selectedStudent;
        private Student? _bookedStudent;
        private List<Booking> _allBookings;
        private List<SearchResult> _allStudents;
        private bool _isNewBooking;
        private string _updateMessage;
        private bool _isBookingDataVisible;
        private bool _isCommentsVisible;

        //Booking fields
        private int _studentId;
        private string _firstName;
        private string _lastName;
        private DateTime _dateTime;
        private List<(Parent, string)> _parents;

        //Data fields
        private int _math;
        private string _mathComments;
        private int _reading;
        private string _readingComments;
        private int _writing;
        private string _writingComments;
        private int _science;
        private int _history;
        private int _geography;
        private int _mfl;
        private int _pe;
        private int _art;
        private int _music;
        private int _re;
        private int _designTechnology;
        private int _computing;

        //Comments fields
        private string _generalComments;
        private string _pupilComments;
        private string _parentComments;
        private string _behaviorNotes;
        private string _attendanceNotes;
        private string _homeworkNotes;
        private string _extraCurricularNotes;
        private string _specialEducationalNeedsNotes;
        private string _safeguardingNotes;
        private string _otherNotes;
        private int _dateAdded;

        //Cmmmand backing fields
        private ICommand? _addUpdateBookingCommand;
        private ICommand? _deleteBookingCommand;
        private ICommand? _clearFormCommand;
        private ICommand? _loadBookingCommand;
        private ICommand? _updateStudentDataCommand;
        private ICommand? _updateStudentCommentsCommand;
        private ICommand? _toggleBookingDataVisibilityCommand;
        private ICommand? _toggleCommentsVisibilityCommand;

        //Properties
        public Booking Booking
        {
            get => _booking;
            set
            {
                if (!IsSqlInjectionSafe(value))
                {
                    UpdateMessage = SqlInjectionMessage;
                    return;
                }
                
                UpdateMessage = string.Empty;
                SetProperty(ref _booking, value);

                SetBookingProperties();
            }
        }
        public Student? BookedStudent
        {
            get => _bookedStudent;
            set
            {
                SetProperty(ref _bookedStudent, value);
            }
        }
        public SearchResult SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                SetProperty(ref _selectedStudent, value);
                Task.Run(async () => await OnStudentSelected());
            }
        }
        public List<Booking> AllBookings
        {
            get => _allBookings;
            set => SetProperty(ref _allBookings, value);
        }
        public List<SearchResult> AllStudents
        {
            get => _allStudents;
            set => SetProperty(ref _allStudents, value);
        }
        public string UpdateMessage
        {
            get => _updateMessage;
            set => SetProperty(ref _updateMessage, value);
        }
        public bool IsNewBooking
        {
            get => _isNewBooking;
            set
            {
                SetProperty(ref _isNewBooking, value);
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(AddUpdateButtonLabel));
            }
        }
        public bool IsBookingDataVisible
        {
            get => _isBookingDataVisible;
            set
            {
                SetProperty(ref _isBookingDataVisible, value);
                OnPropertyChanged(nameof(ShowHideDataButtonLabel));
            }
        }
        public bool IsCommentsVisible
        {
            get => _isCommentsVisible;
            set
            {
                SetProperty(ref _isCommentsVisible, value);
                OnPropertyChanged(nameof(ShowHideCommentsButtonLabel));
            }
        }

        //Booking properties
        public int StudentId
        {
            get => _studentId;
            set => SetProperty(ref _studentId, value);
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
        public DateTime DateTime
        {
            get => _dateTime;
            set => SetProperty(ref _dateTime, value);
        }
        public List<(Parent, string)> Parents
        {
            get => _parents;
            set => SetProperty(ref _parents, value);
        }

        //Data properties
        public int Math
        {
            get => _math;
            set => SetProperty(ref _math, value);
        }
        public string MathComments
        {
            get => _mathComments;
            set => SetProperty(ref _mathComments, value);
        }
        public int Reading
        {
            get => _reading;
            set => SetProperty(ref _reading, value);
        }
        public string ReadingComments
        {
            get => _readingComments;
            set => SetProperty(ref _readingComments, value);
        }
        public int Writing
        {
            get => _writing;
            set => SetProperty(ref _writing, value);
        }
        public string WritingComments
        {
            get => _writingComments;
            set => SetProperty(ref _writingComments, value);
        }
        public int Science
        {
            get => _science;
            set => SetProperty(ref _science, value);
        }
        public int History
        {
            get => _history;
            set => SetProperty(ref _history, value);
        }
        public int Geography
        {
            get => _geography;
            set => SetProperty(ref _geography, value);
        }
        public int MFL
        {
            get => _mfl;
            set => SetProperty(ref _mfl, value);
        }
        public int PE
        {
            get => _pe;
            set => SetProperty(ref _pe, value);
        }
        public int Art
        {
            get => _art;
            set => SetProperty(ref _art, value);
        }
        public int Music
        {
            get => _music;
            set => SetProperty(ref _music, value);
        }
        public int RE
        {
            get => _re;
            set => SetProperty(ref _re, value);
        }
        public int DesignTechnology
        {
            get => _designTechnology;
            set => SetProperty(ref _designTechnology, value);
        }
        public int Computing
        {
            get => _computing;
            set => SetProperty(ref _computing, value);
        }

        //Comments properties
        public string GeneralComments
        {
            get => _generalComments;
            set => SetProperty(ref _generalComments, value);
        }
        public string PupilComments
        {
            get => _pupilComments;
            set => SetProperty(ref _pupilComments, value);
        }
        public string ParentComments
        {
            get => _parentComments;
            set => SetProperty(ref _parentComments, value);
        }
        public string BehaviorNotes
        {
            get => _behaviorNotes;
            set => SetProperty(ref _behaviorNotes, value);
        }
        public string AttendanceNotes
        {
            get => _attendanceNotes;
            set => SetProperty(ref _attendanceNotes, value);
        }
        public string HomeworkNotes
        {
            get => _homeworkNotes;
            set => SetProperty(ref _homeworkNotes, value);
        }
        public string ExtraCurricularNotes
        {
            get => _extraCurricularNotes;
            set => SetProperty(ref _extraCurricularNotes, value);
        }
        public string SpecialEducationalNeedsNotes
        {
            get => _specialEducationalNeedsNotes;
            set => SetProperty(ref _specialEducationalNeedsNotes, value);
        }
        public string SafeguardingNotes
        {
            get => _safeguardingNotes;
            set => SetProperty(ref _safeguardingNotes, value);
        }
        public string OtherNotes
        {
            get => _otherNotes;
            set => SetProperty(ref _otherNotes, value);
        }
        public int DateAdded
        {
            get => _dateAdded;
            set => SetProperty(ref _dateAdded, value);
        }
        public int CurrentDateAdded => (int)DateTime.Now
                .ToString("yyyyMMdd")
                .ToCharArray().Select(c => c - '0')
                .Aggregate(0, (a, b) => a * 10 + b); //Stored as an int value in the database.

        //Commands

        public ICommand? AddUpdateBookingCommand => _addUpdateBookingCommand
            ??= new RelayCommand(async param =>
            {
                if (IsNewBooking)
                    await AddBooking();
                else
                    await UpdateBooking();
            });
        public ICommand? DeleteBookingCommand => _deleteBookingCommand
            ??= new RelayCommand(async param => await DeleteBooking());
        public ICommand? ClearFormCommand => _clearFormCommand
            ??= new RelayCommand(param => ClearForm());
        public ICommand? LoadBookingCommand => _loadBookingCommand
            ??= null;
        public ICommand? UpdateStudentDataCommand => _updateStudentDataCommand 
            ??= new RelayCommand(async param => await UpdateStudentData());
        public ICommand? UpdateStudentCommentsCommand => _updateStudentCommentsCommand
            ??= new RelayCommand(async param => await UpdateStudentComments());
        public ICommand? ToggleBookingDataVisibilityCommand => _toggleBookingDataVisibilityCommand
            ??= new RelayCommand(param => IsBookingDataVisible = !IsBookingDataVisible);
        public ICommand? ToggleCommentsVisibilityCommand => _toggleCommentsVisibilityCommand
            ??= new RelayCommand(param => IsCommentsVisible = !IsCommentsVisible);

        public AddBookingViewModel(
            IEventAggregator eventAggregator,
            IBookingManager bookingManager,
            IReadOperationService readOperationService, 
            ICreateOperationService createOperationService, 
            IUpdateOperationService updateOperationService, 
            IDeleteOperationService deleteOperationService)
        {
            _eventAggregator = eventAggregator 
                ?? throw new ArgumentNullException(nameof(eventAggregator));
            _bookingManager = bookingManager
                ?? throw new ArgumentNullException(nameof(bookingManager));
            _readOperationService = readOperationService 
                ?? throw new ArgumentNullException(nameof(readOperationService));
            _createOperationService = createOperationService 
                ?? throw new ArgumentNullException(nameof(createOperationService));
            _updateOperationService = updateOperationService 
                ??  throw new ArgumentNullException(nameof(updateOperationService));
            _deleteOperationService = deleteOperationService 
                ?? throw new ArgumentNullException(nameof(deleteOperationService));

            _booking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
            _bookedStudent = null;
            _allBookings = _bookingManager.ListBookings().GetAwaiter().GetResult();
            _allStudents = _readOperationService.GetStudentList().GetAwaiter().GetResult();
            _isNewBooking = true;
            _updateMessage = string.Empty;
            _isBookingDataVisible = false;
            _isCommentsVisible = false;

            _eventAggregator.GetEvent<DisplayBookingEvent>().Subscribe(param =>
            {
                if (param is Booking booking)
                {
                    Booking = booking;
                    IsNewBooking = false;
                }
            });
        }

        //Methods

        public async Task LoadBooking()
        {
            //logic to load a booking from the combo box selection

            await OnBookingLoaded(); //Update the booked student data.
        }

        /// <summary>
        /// Adds a new booking to the database using the data stored in the <see cref="_booking"/> field. Validates that 
        /// all necessary data has been entered before attempting to add the booking. If the booking is added successfully, 
        /// the <see cref="AllBookings"/> property is updated to include the new booking and the <see cref="_booking"/> 
        /// field is reset to a new instance of the <see cref="Booking"/> record with default data ready to add another 
        /// booking. A success or failure message is stored in the <see cref="UpdateMessage"/> property to inform the user 
        /// of the outcome.
        /// </summary>
        public async Task AddBooking()
        {
            UpdateMessage = string.Empty;

            if (!IsValidBooking())
            {
                UpdateMessage = NoBookingDataMessage;
                return;
            }

            bool bookingAdded = await _bookingManager.CreateBooking(_booking);

            if (bookingAdded)
            {
                AllBookings = await _bookingManager.ListBookings(); //Update bookings list with new booking.
                ResetBooking();
                UpdateMessage = BookingAddedMessage;
            }
            else 
            {                 
                UpdateMessage = BookingFailedToAddMessage;
            }
        }

        /// <summary>
        /// Updates an existing booking in the database using the data stored in the <see cref="_booking"/> field. 
        /// Validates that all necessary data has been entered before attempting to update the booking. Updates the 
        /// <see cref="AllBookings"/> property to include the updated booking data if the update is successful and reserts 
        /// the form to a new instance of the <see cref="Booking"/> record with default data ready to add a new booking. 
        /// The <see cref="UpdateMessage"/> property is updated with a success or failure message to inform the user of 
        /// the outcome.
        /// </summary>
        public async Task UpdateBooking()
        {
            UpdateMessage = string.Empty;

            if (!IsValidBooking())
            {
                UpdateMessage = NoBookingDataMessage;
                return;
            }

            bool bookingUpdated = await _bookingManager.UpdateBooking(_booking);

            if (bookingUpdated)
            {
                AllBookings = await _bookingManager.ListBookings(); //Update bookings list with updated booking data.
                ResetBooking();
                UpdateMessage = BookingUpdatedMessage;
            }
            else
            {
                UpdateMessage = BookingFailedToUpdateMessage;
            }
        }

        /// <summary>
        /// Deletes the selected booking from the database. Displays an <see cref="UpdateMessage"/> to inform the user of 
        /// the outcome of the deletion.
        /// </summary>
        public async Task DeleteBooking()
        {
            bool bookingDeleted = await _bookingManager.DeleteBooking(_booking.StudentId);
            UpdateMessage = bookingDeleted ? BookingDeletedMessage : BookingFailedToDeleteMessage;
        }

        /// <summary>
        /// Clears all information from the form to start a new booking without affecting the current booking.
        /// </summary>
        public void ClearForm()
        {
            UpdateMessage = string.Empty;
            ResetBooking();
        }

        /// <summary>
        /// Updates the data record for the <see cref="BookedStudent"/>. If there is not any data added, creates a new 
        /// record in the database. Called by the <see cref="UpdateStudentDataCommand"/> to allows the user to ensure that 
        /// the student data is saved.
        /// </summary>
        public async Task UpdateStudentData()
        {
            //Check if a valid student has been added.
            if (BookedStudent == null || BookedStudent.Id <= 0)
            {
                UpdateMessage = InvalidStudentDataFailedMessage;
                return;
            }

            bool dataAdded;

            //Check if valid data has already been added/loaded.
            if (BookedStudent.Data.StudentId > 0)
            {
                dataAdded = await _updateOperationService.UpdateData(BookedStudent.Data);
            }
            else
            {
                StudentId = BookedStudent.Id;
                dataAdded = await _createOperationService.AddData(BookedStudent.Data);
            }

            if (dataAdded)
                UpdateMessage = DataAddedMessage;
            else
                UpdateMessage = DataFailedToAddMessage;
        }

        /// <summary>
        /// Updates the comments record for the <see cref="BookedStudent"/>. If there are not any comments added, creates 
        /// a new record in the database. Called by the <see cref="UpdateStudentCommentsCommand"/> to allow the user to 
        /// ensure that the meeting comments are saved.
        /// </summary>
        public async Task UpdateStudentComments()
        {
            //Checks that a student has been selected.
            if (BookedStudent == null || BookedStudent.Id <= 0)
            {
                UpdateMessage = InvalidStudentCommentsFailedMessage;
                return;
            }

            bool commentsAdded;

            //Checks if comments have already been added before attempting to update.
            if (BookedStudent.Comments.StudentId > 0)
            {
                commentsAdded = await _updateOperationService.UpdateComments(BookedStudent.Comments);
            }
            else
            {
                StudentId = BookedStudent.Id;
                commentsAdded = await _createOperationService.AddComments(BookedStudent.Comments);
            }

            if (commentsAdded)
                UpdateMessage = CommentsAddedMessage;
            else
                UpdateMessage = CommentsFailedToAddMessage;
        }

        //Helper methods

        private void SetBookingProperties()
        {
            StudentId = _booking.StudentId;
            FirstName = _booking.FirstName;
            LastName = _booking.LastName;
            DateTime = _booking.BookingDate.HasValue && _booking.TimeSlot.HasValue
                ? _booking.BookingDate.Value.Date + _booking.TimeSlot.Value
                : DateTime.Now;
        }

        private void SetStudentProperties()
        {
            if (_bookedStudent == null)
                return;

            FirstName = _bookedStudent.FirstName ?? string.Empty;
            LastName = _bookedStudent.LastName ?? string.Empty;
            Parents = _bookedStudent.Parents ?? [];

            SetDataProperties();
            SetCommentsProperties();
        }

        private void SetDataProperties()
        {
            if (_bookedStudent == null)
                return;

            Math = _bookedStudent.Data.Math;
            MathComments = _bookedStudent.Data.MathComments;
            Reading = _bookedStudent.Data.Reading;
            ReadingComments = _bookedStudent.Data.ReadingComments;
            Writing = _bookedStudent.Data.Writing;
            WritingComments = _bookedStudent.Data.WritingComments;
            Science = _bookedStudent.Data.Science;
            History = _bookedStudent.Data.History;
            Geography = _bookedStudent.Data.Geography;
            MFL = _bookedStudent.Data.MFL;
            PE = _bookedStudent.Data.PE;
            Art = _bookedStudent.Data.Art;
            Music = _bookedStudent.Data.Music;
            RE = _bookedStudent.Data.RE;
            DesignTechnology = _bookedStudent.Data.DesignTechnology;
            Computing = _bookedStudent.Data.Computing;
        }

        private void SetCommentsProperties()
        {
            if (_bookedStudent == null)
                return;
            
            GeneralComments = _bookedStudent.Comments.GeneralComments;
            PupilComments = _bookedStudent.Comments.PupilComments;
            ParentComments = _bookedStudent.Comments.ParentComments;
            BehaviorNotes = _bookedStudent.Comments.BehaviorNotes;
            AttendanceNotes = _bookedStudent.Comments.AttendanceNotes;
            HomeworkNotes = _bookedStudent.Comments.HomeworkNotes;
            ExtraCurricularNotes = _bookedStudent.Comments.ExtraCurricularNotes;
            SpecialEducationalNeedsNotes = _bookedStudent.Comments.SpecialEducationalNeedsNotes;
            SafeguardingNotes = _bookedStudent.Comments.SafeguardingNotes;
            OtherNotes = _bookedStudent.Comments.OtherNotes;
            DateAdded = _bookedStudent.Comments.DateAdded;
        }

        /// <summary>
        /// Updates the <see cref="BookedStudent"/> property when a <see cref="Booking"/> property is loaded to ensure 
        /// that the correct student data is available for display in the view.
        /// </summary>
        private async Task OnBookingLoaded()
        {
            BookedStudent = _booking.StudentId > 0
                ? await _readOperationService.GetStudentData(_booking.StudentId)
                : null;
        }

        private async Task OnStudentSelected()
        {
            if (SelectedStudent.Id <= 0)
                return;

            BookedStudent = await _readOperationService.GetStudentData(SelectedStudent.Id);
        }

        /// <summary>
        /// Checks that all necessary fields in the <see cref="_booking"/> record contain valid data. Used to ensure that 
        /// a booking can be added to the database.
        /// </summary>
        /// <returns><c>true</c> if all the necessary data has been set for the <see cref="Booking"/> instance. <c>false</c> 
        /// if any data is missing or invalid.</returns>
        private bool IsValidBooking()
        {
            if (!IsValidDate() || !IsValidTime())
                return false;
            
            return _booking.StudentId > 0 &&
                   !string.IsNullOrWhiteSpace(_booking.FirstName) &&
                   !string.IsNullOrWhiteSpace(_booking.LastName) &&
                   !string.IsNullOrWhiteSpace(_booking.DateString) &&
                   !string.IsNullOrWhiteSpace(_booking.TimeString);
        }

        /// <summary>
        /// Checks that the first name and last name fields in the <see cref="_booking"/> record do not contain any 
        /// characters that could be used in a SQL injection attack. Only letters, hyphens, spaces and apostrophes are 
        /// allowed.
        /// </summary>
        /// <param name="booking">The <see cref="Booking"/> record to be checked for invalid characters.</param>
        /// <returns><see langword="true"/> if all the characters stored in the name properties of the <see cref="Booking"/> 
        /// are on the whitelist. <see langword="false"/> if any invalid characters are entered.</returns>
        private static bool IsSqlInjectionSafe(Booking booking)
        {
            return booking.FirstName.All(c => char.IsLetter(c) || c == '-' || c == ' ' || c == '\'') &&
                   booking.LastName.All(c => char.IsLetter(c) || c == '-' || c == ' ' || c == '\'');
        }

        /// <summary>
        /// Verifies that the date string in the <see cref="_booking"/> record is in the correct format of "yyyy-Mm-dd". 
        /// Ensures that the date can be parsed correctly when adding a booking to the database.
        /// </summary>
        /// <returns><see langword="true"/> if a valid date is stored in the <see cref="Booking"/>. <see langword="false"/> 
        /// if the date has not been stored in the correct format.</returns>
        private bool IsValidDate()
        {
            return DateTime.TryParseExact(
                _booking.DateString, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        /// <summary>
        /// Verifies that the time string in the <see cref="_booking"/> record is in the correct format of "hh:mm". Ensures 
        /// that the time can be parsed correctly when adding a booking to the database.
        /// </summary>
        /// <returns><see langword="true"/> if a valid time is stored in the <see cref="Booking"/>. <see langword="false"/> 
        /// if the time has not been stored in the correct format.</returns>
        private bool IsValidTime()
        {
            return TimeSpan.TryParseExact(
                _booking.TimeString, 
                "hh\\:mm", 
                CultureInfo.InvariantCulture, 
                out _);
        }

        /// <summary>
        /// Resets the <see cref="_booking"/> field to a new instance of the <see cref="Booking"/> record with default data 
        /// ready to add a new booking or view an existing booking. Sets the <see cref="_isNewBooking"/> flag to <c>true</c> 
        /// to indicate that the next data added will be for a new booking.
        /// </summary>
        private void ResetBooking()
        {
            Booking = new Booking(0, string.Empty, string.Empty, string.Empty, string.Empty);
            IsNewBooking = true;
        }
    }
}

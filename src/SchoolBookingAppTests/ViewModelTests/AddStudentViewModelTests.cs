using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Model;
using SchoolBookingApp.MVVM.Viewmodel;

namespace SchoolBookingAppTests.ViewModelTests
{
    public class AddStudentViewModelTests
    {
        private readonly Mock<IReadOperationService> _readOperationServiceMock;
        private readonly Mock<ICreateOperationService> _createOperationServiceMock;
        private readonly Mock<IUpdateOperationService> _updateOperationServiceMock;
        private readonly Mock<IDeleteOperationService> _deleteOperationServiceMock;

        private readonly AddStudentViewModel _viewModel;
        private readonly AddStudentViewModel _viewModelWithSelectedStudent;

        private readonly Student _testStudent;
        private readonly string _sqlInjectionString;
        private readonly Parent _testParent;
        private readonly SearchResult _testSearchResult;
        private readonly string _testString;
        private readonly int _testDateOfBirthInt;
        private readonly DateTime _testDateOfBirth;
        private readonly string _whiteSpaceString;
        public AddStudentViewModelTests()
        {
            _readOperationServiceMock = new Mock<IReadOperationService>();
            _createOperationServiceMock = new Mock<ICreateOperationService>();
            _updateOperationServiceMock = new Mock<IUpdateOperationService>();
            _deleteOperationServiceMock = new Mock<IDeleteOperationService>();

            _testString = "test string";
            _sqlInjectionString = "'; DROP TABLE Students;--";
            _testDateOfBirthInt = 20250911;
            _testDateOfBirth = new DateTime(2025, 9, 11);
            _whiteSpaceString = "   ";
            _testStudent = new Student(1, _testString, _testString, _testDateOfBirthInt, _testString, [], new(), new());
            _testParent = new Parent(1, _testString, _testString, []);
            _testSearchResult = new SearchResult()
            {
                Id = 1,
                FirstName = _testString,
                LastName = _testString,
                Category = _testString
            };

            _readOperationServiceMock
                .Setup(x => x.GetStudentList())
                .Returns(Task.FromResult<List<SearchResult>>([]));
            _readOperationServiceMock
                .Setup(x => x.GetStudentData(It.IsAny<int>()))
                .Returns(Task.FromResult<Student>(_testStudent));
            _createOperationServiceMock
                .Setup(x => x.AddStudent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            _updateOperationServiceMock
                .Setup(x => x.UpdateStudent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            _viewModel = new AddStudentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                _updateOperationServiceMock.Object,
                _deleteOperationServiceMock.Object
                );
        }

        //Constructor tests.

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when the <see cref="IReadOperationService"/> 
        /// parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullReadOperationService_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddStudentViewModel(
                null!,
                _createOperationServiceMock.Object,
                _updateOperationServiceMock.Object,
                _deleteOperationServiceMock.Object));
        }

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when the <see cref="ICreateOperationService"/> 
        /// parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullCreateOperationService_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddStudentViewModel(
                _readOperationServiceMock.Object,
                null!,
                _updateOperationServiceMock.Object,
                _deleteOperationServiceMock.Object));
        }

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when the <see cref="IUpdateOperationService"/> 
        /// parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullUpdateOperationService_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddStudentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                null!,
                _deleteOperationServiceMock.Object));
        }

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when the <see cref="IDeleteOperationService"/> 
        /// parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullDeleteOperationService_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddStudentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                _updateOperationServiceMock.Object,
                null!));
        }

        /// <summary>
        /// Verifies that a non-null instance of the <see cref="AddStudentViewModel"/> class is instantiated when the 
        /// constructor is called with valid parameters.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_CreatesInstanceSuccessfully()
        {
            //Arrange, Act
            var viewModel = new AddStudentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                _updateOperationServiceMock.Object,
                _deleteOperationServiceMock.Object);

            //Assert
            Assert.NotNull(viewModel);
            Assert.IsType<AddStudentViewModel>(viewModel);
        }

        //SqlInjectionCheck on Properties tests.

        /// <summary>
        /// Verifies that when a SQL injection string is provided for the <see cref="AddStudentViewModel.FirstName"/> 
        /// property, the property value remains unchanged, indicating that the input was rejected. Ensures that the 
        /// application is protected against SQL injection attacks.
        /// </summary>
        [Fact]
        public void FirstName_SetSqlInjectionString_PropertyUnchanges()
        {
            //Arrange
            _viewModel.FirstName = _testString;

            //Act
            _viewModel.FirstName = _sqlInjectionString;

            //Assert
            Assert.Equal(_testString, _viewModel.FirstName);
        }

        /// <summary>
        /// Verifies that when a SQL injection string is provided for the <see cref="AddStudentViewModel.LastName"/> 
        /// property, the property value remains unchanged, indicating that the input was rejected. Ensures that the 
        /// application is protected against SQL injection attacks.
        /// </summary>
        [Fact]
        public void LastName_SetSqlInjectionString_PropertyUnchanges()
        {
            //Arrange
            _viewModel.LastName = _testString;

            //Act
            _viewModel.LastName = _sqlInjectionString;

            //Assert
            Assert.Equal(_testString, _viewModel.LastName);
        }

        /// <summary>
        /// Verifies that when a SQL injection string is provided for the <see cref="AddStudentViewModel.ClassName"/> 
        /// property, the property value remains unchanged, indicating that the input was rejected. Ensures that the 
        /// application is protected against SQL injection attacks.
        /// </summary>
        [Fact]
        public void ClassName_SetSqlInjectionString_PropertyUnchanges()
        {
            //Arrange
            _viewModel.ClassName = _testString;

            //Act
            _viewModel.ClassName = _sqlInjectionString;

            //Assert
            Assert.Equal(_testString, _viewModel.ClassName);
        }

        //DisplayStudentDetails tests.

        /// <summary>
        /// Verifies that when <see cref="AddStudentViewModel.DisplayStudentDetails"/> is called with a valid <see 
        /// cref="SearchResult"/> set to <see cref="AddStudentViewModel.SelectedStudent"/>, the UI properties are updated 
        /// to reflect the details of the selected student. Ensures that the displayed student information is correct.
        /// </summary>
        [Fact]
        public async Task DisplayStudentDetails_ValidSelectedStudentId_UIPropertiesUpdatedCorrectly()
        {
            //Arrange
            _viewModel.SelectedStudent = _testSearchResult;

            //Act
            await _viewModel.DisplayStudentDetails();

            //Assert
            Assert.Equal(_testString, _viewModel.FirstName);
            Assert.Equal(_testString, _viewModel.LastName);
            Assert.Equal(_testDateOfBirth, _viewModel.DateOfBirth);
            Assert.Equal(_testString, _viewModel.ClassName);
        }

        /// <summary>
        /// Verifies that when <see cref="AddStudentViewModel.DisplayStudentDetails"/> is called with an invalid <see 
        /// cref="SearchResult"/> (null or Id <= 0) set for the <see cref="AddStudentViewModel.SelectedStudent"/> property, 
        /// the UI properties remain unchanged. Ensures that no erroneous data is displayed when an invalid
        /// </summary>
        /// <remarks>Uses a new viewmodel instance to ensure the key proeprties are set to their default values to ensure 
        /// testing consistency.</remarks>
        [Fact]
        public async Task DisplayStudentDetails_InvalidSelectedStudent_UIPropertiesRemainUnchanged()
        {
            //Arrange
            var invalidSearchResult = new SearchResult()
            {
                Id = -1,
                FirstName = "Invalid",
                LastName = "Student",
                Category = "Student"
            };
            var viewModel = new AddStudentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                _updateOperationServiceMock.Object,
                _deleteOperationServiceMock.Object);
            viewModel.SelectedStudent = invalidSearchResult;

            //Act
            await viewModel.DisplayStudentDetails();

            //Assert
            Assert.Equal(string.Empty, viewModel.FirstName);
            Assert.Equal(string.Empty, viewModel.LastName);
            Assert.Equal(DateTime.MinValue, viewModel.DateOfBirth);
            Assert.Equal(string.Empty, viewModel.ClassName);
        }

        //AddUpdateStudent tests.

        /// <summary>
        /// Verifies that when a whitespace string is provided for the <see cref="AddStudentViewModel.FirstName"/> property, 
        /// the data is not considered valid and neither the <see cref="ICreateOperationService.AddStudent"/> nor the 
        /// <see cref="IUpdateOperationService.UpdateStudent"/> methods are called. Ensures that invalid data is not added 
        /// to or updated in the database.
        /// </summary>
        [Fact]
        public async Task AddUpdateStudent_WhiteSpaceFirstName_AddStudentUpdateStudentNotCalled()
        {
            //Arrange
            _viewModel.FirstName = _whiteSpaceString;
            _viewModel.LastName = _testString;
            _viewModel.DateOfBirth = _testDateOfBirth;
            _viewModel.ClassName = _testString;

            //Act
            await _viewModel.AddUpdateStudent();

            //Assert
            _createOperationServiceMock.Verify(x => x.AddStudent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _updateOperationServiceMock.Verify(x => x.UpdateStudent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when a whitespace string is provided for the <see cref="AddStudentViewModel.LastName"/> property, 
        /// the data is not considered valid and neither the <see cref="ICreateOperationService.AddStudent"/> nor the 
        /// <see cref="IUpdateOperationService.UpdateStudent"/> methods are called. Ensures that invalid data is not added 
        /// to or updated in the database.
        /// </summary>
        [Fact]
        public async Task AddUpdateStudent_WhiteSpaceLastName_AddStudentUpdateStudentNotCalled()
        {
            //Arrange
            _viewModel.FirstName = _testString;
            _viewModel.LastName = _whiteSpaceString;
            _viewModel.DateOfBirth = _testDateOfBirth;
            _viewModel.ClassName = _testString;

            //Act
            await _viewModel.AddUpdateStudent();

            //Assert
            _createOperationServiceMock.Verify(x => x.AddStudent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _updateOperationServiceMock.Verify(x => x.UpdateStudent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when a whitespace string is provided for the <see cref="AddStudentViewModel.ClassName"/> property, 
        /// the data is not considered valid and neither the <see cref="ICreateOperationService.AddStudent"/> nor the 
        /// <see cref="IUpdateOperationService.UpdateStudent"/> methods are called. Ensures that invalid data is not added 
        /// to or updated in the database.
        /// </summary>
        [Fact]
        public async Task AddUpdateStudent_WhiteSpaceClassName_AddStudentUpdateStudentNotCalled()
        {
            //Arrange
            _viewModel.FirstName = _testString;
            _viewModel.LastName = _testString;
            _viewModel.DateOfBirth = _testDateOfBirth;
            _viewModel.ClassName = _whiteSpaceString;

            //Act
            await _viewModel.AddUpdateStudent();

            //Assert
            _createOperationServiceMock.Verify(x => x.AddStudent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _updateOperationServiceMock.Verify(x => x.UpdateStudent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when a min value <see cref="DateTime"/> is provided for the <see cref="AddStudentViewModel
        /// .DateOfBirth"/> property, the data is not considered valid and neither the <see cref="ICreateOperationService
        /// .AddStudent"/> nor the <see cref="IUpdateOperationService.UpdateStudent"/> methods are called. Ensures that 
        /// invalid data is not added to or updated in the database.
        /// </summary>
        [Fact]
        public async Task AddUpdateStudent_MinValueDateOfBirth_AddStudentInvalidStudentNotCalled()
        {
            //Arrange
            _viewModel.FirstName = _testString;
            _viewModel.LastName = _testString;
            _viewModel.DateOfBirth = DateTime.MinValue;
            _viewModel.ClassName = _testString;

            //Act
            await _viewModel.AddUpdateStudent();

            //Assert
            _createOperationServiceMock.Verify(x => x.AddStudent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _updateOperationServiceMock.Verify(x => x.UpdateStudent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when a <see langword="null"/> <see cref="AddStudentViewModel.SelectedStudent"/> is provided when 
        /// the <see cref="AddStudentViewModel.IsNewStudent"/> property is set to <see langword="false"/>, neither the 
        /// <see cref="ICreateOperationService.AddStudent"/> nor the <see cref="IUpdateOperationService.UpdateStudent"/> 
        /// methods are called. Ensures that no update operation is attempted without a valid selected student.
        /// </summary>
        [Fact]
        public async Task AddUpdateStudent_IsNotNewStudentNullSelectedStudent_AddStudentUpdateStudentNotCalled()
        {
            //Arrange
            _viewModel.IsNewStudent = false;
            _viewModel.SelectedStudent = null;
            _viewModel.FirstName = _testString;
            _viewModel.LastName = _testString;
            _viewModel.DateOfBirth = _testDateOfBirth;
            _viewModel.ClassName = _testString;

            //Act
            await _viewModel.AddUpdateStudent();

            //Assert
            _createOperationServiceMock.Verify(x => x.AddStudent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _updateOperationServiceMock.Verify(x => x.UpdateStudent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when a <see cref="AddStudentViewModel.SelectedStudent"/> with an invalid Id is provided and 
        /// the <see cref="AddStudentViewModel.IsNewStudent"/> property is set to <see langword="false"/>, neither the 
        /// <see cref="ICreateOperationService.AddStudent"/> nor the <see cref="IUpdateOperationService.UpdateStudent"/> 
        /// methods are called. Ensures that no update operation is attempted without a valid selected student.
        /// </summary>
        [Fact]
        public async Task AddUpdateStudent_IsNotNewStudentInvalidSelectedStudentId_AddStudentUpdateStudentNotCalled()
        {
            //Arrange
            _viewModel.IsNewStudent = false;
            _viewModel.SelectedStudent = new SearchResult()
            {
                Id = -1,
                FirstName = "Invalid",
                LastName = "Student",
                Category = "Student"
            };
            _viewModel.FirstName = _testString;
            _viewModel.LastName = _testString;
            _viewModel.DateOfBirth = _testDateOfBirth;
            _viewModel.ClassName = _testString;

            //Act
            await _viewModel.AddUpdateStudent();

            //Assert
            _createOperationServiceMock.Verify(x => x.AddStudent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _updateOperationServiceMock.Verify(x => x.UpdateStudent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when valid data is provided for an existing student (with <see cref="AddStudentViewModel.
        /// IsNewStudent"/> property set to <see langword="false"/>) the <see cref="IUpdateOperationService.UpdateStudent"/> 
        /// method is called exactly once with the correct parameters, and so the correct data is updated in the database. 
        /// Ensures that student updates are handled correctly.
        /// </summary>
        [Fact]
        public async Task AddUpdateStudent_IsValidExistingStudent_CallsUpdateStudentOnce()
        {
            //Arrange
            _viewModel.IsNewStudent = false;
            _viewModel.SelectedStudent = _testSearchResult;
            _viewModel.FirstName = _testString;
            _viewModel.LastName = _testString;
            _viewModel.DateOfBirth = _testDateOfBirth;
            _viewModel.ClassName = _testString;

            //Act
            await _viewModel.AddUpdateStudent();

            //Assert
            _updateOperationServiceMock.Verify(x => x.UpdateStudent(_testSearchResult.Id, _testString, _testString, _testDateOfBirthInt, _testString), Times.Once);
            _createOperationServiceMock.Verify(x => x.AddStudent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Verifies that when valid data is provided for a new student (with <see cref="AddStudentViewModel.IsNewStudent"/> 
        /// property set to <see langword="true"/>) the <see cref="ICreateOperationService.AddStudent"/> method is called 
        /// exactly once with the correct parameters, and so the correct data is added to the database. Ensures that new 
        /// student creation is handled correctly.
        /// </summary>
        [Fact]
        public async Task AddUpdateStudent_IsValidNewStudent_CallsAddStudentOnce()
        {
            //Arrange
            _viewModel.IsNewStudent = true;
            _viewModel.FirstName = _testString;
            _viewModel.LastName = _testString;
            _viewModel.DateOfBirth = _testDateOfBirth;
            _viewModel.ClassName = _testString;

            //Act
            await _viewModel.AddUpdateStudent();

            //Assert
            _createOperationServiceMock.Verify(x => x.AddStudent(_testString, _testString, _testDateOfBirthInt, _testString), Times.Once);
            _updateOperationServiceMock.Verify(x => x.UpdateStudent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
    }
}

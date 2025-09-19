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
        private readonly Parent _testParent;
        private readonly SearchResult _testSearchResult;
        private readonly string _testString;
        private readonly int _testDateOfBirthInt;
        private readonly DateTime _testDateOfBirth;
        public AddStudentViewModelTests()
        {
            _readOperationServiceMock = new Mock<IReadOperationService>();
            _createOperationServiceMock = new Mock<ICreateOperationService>();
            _updateOperationServiceMock = new Mock<IUpdateOperationService>();
            _deleteOperationServiceMock = new Mock<IDeleteOperationService>();

            _testString = "test string";
            _testDateOfBirthInt = 20250911;
            _testDateOfBirth = new DateTime(2025, 9, 11);
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
    }
}

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
        private readonly Student _testStudent;
        private readonly Parent _testParent;
        private readonly SearchResult _testSearchResult;
        private readonly string _testString;
        private readonly int _testDateOfBirthInt;
        private readonly string _testRelationship;

        public AddStudentViewModelTests()
        {
            _readOperationServiceMock = new Mock<IReadOperationService>();
            _createOperationServiceMock = new Mock<ICreateOperationService>();
            _updateOperationServiceMock = new Mock<IUpdateOperationService>();
            _deleteOperationServiceMock = new Mock<IDeleteOperationService>();

            _readOperationServiceMock
                .Setup(x => x.GetStudentList())
                .Returns(Task.FromResult<List<SearchResult>>([]));
            _readOperationServiceMock
                .Setup(x => x.GetParentList())
                .Returns(Task.FromResult<List<SearchResult>>([]));

            _viewModel = new AddStudentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                _updateOperationServiceMock.Object,
                _deleteOperationServiceMock.Object
                );

            _testString = "test string";
            _testDateOfBirthInt = 20250911;
            _testStudent = new Student(1, _testString, _testString, _testDateOfBirthInt, _testString, [], new(), new());
            _testParent = new Parent(1, _testString, _testString, []);
            _testSearchResult = new SearchResult();
            _testRelationship = "Relationship";
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

        //Property tests.

        /// <summary>
        /// Verifies that the <see cref="AddStudentViewModel.SelectedNewParent"/> property is set to <see langword="null"/> 
        /// when the <see cref="AddStudentViewModel.SelectedCurrentParent"/> property is updated. Ensures that only one 
        /// parent can be selected.
        /// </summary>
        [Fact]
        public void SelectedCurrentParent_ValueUpdated_SelectedNewParentSetToNull()
        {
            //Arrange & Act
            _viewModel.SelectedCurrentParent = (_testParent, _testRelationship);

            //Assert
            Assert.Null(_viewModel.SelectedNewParent);
        }

        /// <summary>
        /// Verifies that the UI parent detail properties are updated as expected when the <see cref="AddStudentViewModel.
        /// SelectedCurrentParent"/> property is updated. Ensures that the UI displays the correct values to avoid confusion.
        /// </summary>
        [Fact]
        public void SelectedCurrentParent_ValueUpdated_ParentDetailsUpdatedToExpectedValues()
        {
            //Arrange
            string expectedParentName = _testStudent.FirstName + " " + _testStudent.LastName;

            //Act
            _viewModel.SelectedCurrentParent = (_testParent, _testRelationship);

            //Assert
            Assert.NotNull(_viewModel.SelectedCurrentParent);
            Assert.Equal(_testRelationship, _viewModel.Relationship);
            Assert.Equal(expectedParentName, _viewModel.ParentName);
        }


        //Member Data
    }
}

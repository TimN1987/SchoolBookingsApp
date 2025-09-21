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
    public class AddParentViewModelTests
    {
        private const string WhiteSpace = " ";
        private const string EmptyString = "";
        private const string TestName = "Name";
        private const string TestRelationship = "Relationship";
        private const string EmptyFieldsMessage = "Complete all information before adding or updating a parent.";
        private const string RecordAddedMessage = "Parent added successfully.";
        private const string RecordUpdatedMessage = "Parent updated successfully.";
        private const string RecordDeletedMessage = "Parent deleted successfully.";
        private const string InvalidParentUpdateMessage = "Cannot update - the selected parent is invalid.";

        private readonly Mock<IReadOperationService> _readOperationServiceMock;
        private readonly Mock<ICreateOperationService> _createOperationServiceMock;
        private readonly Mock<IUpdateOperationService> _updateOperationServiceMock;
        private readonly Mock<IDeleteOperationService> _deleteOperationServiceMock;

        private readonly AddParentViewModel _viewModel;
        private readonly SearchResult _testParent;

        public AddParentViewModelTests()
        {
            _readOperationServiceMock = new Mock<IReadOperationService>();
            _createOperationServiceMock = new Mock<ICreateOperationService>();
            _updateOperationServiceMock = new Mock<IUpdateOperationService>();
            _deleteOperationServiceMock = new Mock<IDeleteOperationService>();

            _viewModel = new AddParentViewModel(
                _readOperationServiceMock.Object, 
                _createOperationServiceMock.Object, 
                _updateOperationServiceMock.Object, 
                _deleteOperationServiceMock.Object);

            _testParent = new SearchResult()
            {
                Id = 1,
                FirstName = TestName,
                LastName = TestName,
                Category = "Parent"
            };

            _readOperationServiceMock
                .Setup(x => x.GetParentList())
                .Returns(Task.FromResult<List<SearchResult>>([]));
            _readOperationServiceMock
                .Setup(x => x.GetStudentList())
                .Returns(Task.FromResult<List<SearchResult>>([]));
            _createOperationServiceMock
                .Setup(x => x.AddParent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()))
                .Returns(Task.FromResult(true));
            _updateOperationServiceMock
                .Setup(x => x.UpdateParent(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()))
                .Returns(Task.FromResult(true));
        }

        //Constructor tests.

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when a <see langword="null"/> <see 
        /// cref="ReadOperationService"/> is passed as a parameter to the constructor. Ensures that database operations are 
        /// not attempted using a <see langword="null"/> instance of the class.
        /// </summary>
        [Fact]
        public void Constructor_NullReadOperationService_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddParentViewModel(
                null!, 
                _createOperationServiceMock.Object, 
                _updateOperationServiceMock.Object, 
                _deleteOperationServiceMock.Object));
        }

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when a <see langword="null"/> <see 
        /// cref="CreateOperationService"/> is passed as a parameter to the constructor. Ensures that database operations 
        /// are not attempted using a <see langword="null"/> instance of the class.
        /// </summary>
        [Fact]
        public void Constructor_NullCreateOperationService_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddParentViewModel(
                _readOperationServiceMock.Object,
                null!,
                _updateOperationServiceMock.Object,
                _deleteOperationServiceMock.Object));
        }

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when a <see langword="null"/> <see 
        /// cref="UpdateOperationService"/> is passed as a parameter to the constructor. Ensures that database operations 
        /// are not attempted using a <see langword="null"/> instance of the class.
        /// </summary>
        [Fact]
        public void Constructor_NullUpdateOperationService_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddParentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                null!,
                _deleteOperationServiceMock.Object));
        }

        /// <summary>
        /// Verifies that an <see cref="ArgumentNullException"/> is thrown when a <see langword="null"/> <see 
        /// cref="DeleteOperationService"/> is passed as a parameter to the constructor. Ensures that database operations 
        /// are not attempted using a <see langword="null"/> instance of the class.
        /// </summary>
        [Fact]
        public void Constructor_NullDeleteOperationService_ThrowsArgumentNullException()
        {
            //Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AddParentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                _updateOperationServiceMock.Object,
                null!));
        }

        /// <summary>
        /// Verifies that a non-<see langword="null"/> instance of the <see cref="AddParentViewModel"/> in created when 
        /// valid parameters are passed. Ensures that the constructor works as expected.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_CreatesInstanceSuccessfully()
        {
            //Arrange & Act
            var viewModel = new AddParentViewModel(
                _readOperationServiceMock.Object,
                _createOperationServiceMock.Object,
                _updateOperationServiceMock.Object,
                _deleteOperationServiceMock.Object);

            //Assert
            Assert.NotNull(viewModel);
            Assert.IsType<AddParentViewModel>(viewModel);
        }

        //Property tests.

        //AddUpdateParent tests.

        /// <summary>
        /// Verifies that the <see cref="AddParentViewModel.StatusMessage"/> is updated with an error message when an 
        /// invalid field is set and the <see cref="AddParentViewModel.AddUpdateParent"/> method is called. Allows checks 
        /// that no database create or update methods are called. Ensures that no database operations are attempted when 
        /// one or more of the fields are invalid.
        /// </summary>
        /// <param name="firstName">The parent first name.</param>
        /// <param name="lastName">The parent last name.</param>
        [Theory]
        [InlineData(EmptyString, TestName)]
        [InlineData(WhiteSpace, TestName)]
        [InlineData(TestName, EmptyString)]
        [InlineData(TestName, WhiteSpace)]
        public async Task AddUpdateParent_InvalidFields_SetsErrorToStatusUpdate(
            string firstName, string lastName)
        {
            //Arrange
            _viewModel.FirstName = firstName;
            _viewModel.LastName = lastName;

            //Act
            await _viewModel.AddUpdateParent();

            //Assert
            Assert.Equal(EmptyFieldsMessage, _viewModel.StatusMessage);
            _createOperationServiceMock.Verify(x =>
                x.AddParent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()), Times.Never);
            _updateOperationServiceMock.Verify(x =>
                x.UpdateParent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()), Times.Never);
        }

        /// <summary>
        /// Verifies that the <see cref="CreateOperationService.AddParent"/> method is called exactly once when the 
        /// <see cref="AddParentViewModel.IsNewParent"/> value is set to <see langword="true"/> and the <see 
        /// cref="AddParentViewModel.AddUpdateParent"/> method is called. Ensures that the correct database operation is 
        /// run to create a new record for the new parent.
        /// </summary>
        [Fact]
        public async Task AddUpdateParent_IsNewParent_CreateOperationServiceAddParentCalledOnce()
        {
            //Arrange
            _viewModel.IsNewParent = true;
            _viewModel.FirstName = TestName;
            _viewModel.LastName = TestName;

            //Act
            await _viewModel.AddUpdateParent();

            //Assert
            Assert.Equal(RecordAddedMessage, _viewModel.StatusMessage);
            _createOperationServiceMock.Verify(x =>
                x.AddParent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()), Times.Once);
            _updateOperationServiceMock.Verify(x =>
                x.UpdateParent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()), Times.Never);
        }

        /// <summary>
        /// Verifies that the <see cref="AddParentViewModel.StatusMessage"/> is update to an error message when the <see 
        /// cref="AddParentViewModel.AddUpdateParent"/> method is called with no parent selected and the <see 
        /// cref="AddParentViewModel.IsNewParent"/> value set to <see langword="false"/>. Ensures that the correct error 
        /// message is shown when the user attempts to update an invalid parent and that no database operations are 
        /// attempted.
        [Fact]
        public async Task AddUpdateParent_NotIsNewParentNoSelectedParent_StatusMessageSetToInvalidParentMessage()
        {
            //Arrange
            _viewModel.IsNewParent = false;
            _viewModel.FirstName = TestName;
            _viewModel.LastName = TestName;
            _viewModel.SelectedParent = null;

            //Act
            await _viewModel.AddUpdateParent();

            //Assert
            Assert.Equal(InvalidParentUpdateMessage, _viewModel.StatusMessage);
            _createOperationServiceMock.Verify(x =>
                x.AddParent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()), Times.Never);
            _updateOperationServiceMock.Verify(x =>
                x.UpdateParent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()), Times.Never);
        }

        /// <summary>
        /// Verifies that the <see cref="UpdateOperationService.UpdateParent"/> method is called exactly once when the 
        /// <see cref="AddParentViewModel.IsNewParent"/> value is set to <see langword="false"/> and the <see 
        /// cref="AddParentViewModel.AddUpdateParent"/> method is called. Ensures that the correct database operation is 
        /// run to update an existing record for the current parent.
        /// </summary>
        [Fact]
        public async Task AddUpdateParent_NotIsNewParent_UpdateOperationServiceUpdateParentCalledOnce()
        {
            //Arrange
            _viewModel.IsNewParent = false;
            _viewModel.FirstName = TestName;
            _viewModel.LastName = TestName;
            _viewModel.SelectedParent = _testParent;

            //Act
            await _viewModel.AddUpdateParent();

            //Assert
            Assert.Equal(RecordUpdatedMessage, _viewModel.StatusMessage);
            _createOperationServiceMock.Verify(x =>
                x.AddParent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()), Times.Never);
            _updateOperationServiceMock.Verify(x =>
                x.UpdateParent(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<(int, string)>>()), Times.Once);
        }

        //DeleteCurrentParent tests.
    }
}

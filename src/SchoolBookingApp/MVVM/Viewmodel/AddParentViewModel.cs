using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class AddParentViewModel : ViewModelBase
    {
        //Constants


        //UI Label Properties
        public string AddParentTitle => IsNewParent ? "Add Parent" : "Update Parent";

        //Fields
        private readonly IReadOperationService _readOperationService;
        private readonly ICreateOperationService _createOperationService;
        private readonly IUpdateOperationService _updateOperationService;
        private readonly IDeleteOperationService _deleteOperationService;

        private bool _isNewParent;
        private bool _isExistingParentSelected;
        private string _statusMessage;

        //Fields for parent details.
        private List<SearchResult> _allParents;
        private List<SearchResult> _allStudents;

        private string _firstName;
        private string _lastName;
        private string _relationship;

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
        }


    }
}

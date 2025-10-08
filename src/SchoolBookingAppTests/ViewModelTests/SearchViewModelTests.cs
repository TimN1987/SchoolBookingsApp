using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SchoolBookingApp.MVVM.Database;

namespace SchoolBookingAppTests.ViewModelTests
{
    public class SearchViewModelTests
    {
        private readonly Mock<IEventAggregator> _eventAggregatorMock;
        private readonly Mock<IReadOperationService> _readOperationServiceMock;

        public SearchViewModelTests()
        {
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _readOperationServiceMock = new Mock<IReadOperationService>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolBookingApp.MVVM.Viewmodel.Base;

namespace SchoolBookingApp.MVVM.Viewmodel
{
    public class ViewBookingViewModel(IEventAggregator eventAggregator) : ViewModelBase
    {
        private readonly IEventAggregator _eventAggregator = eventAggregator 
            ?? throw new ArgumentNullException(nameof(eventAggregator));
    }
}

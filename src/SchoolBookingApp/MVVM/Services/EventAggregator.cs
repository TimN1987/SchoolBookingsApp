using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Viewmodel;
using SchoolBookingApp.MVVM.Model;

namespace SchoolBookingApp.MVVM.Services
{
    /// <summary>
    /// Used to update the <see cref="MainViewModel"/> when the view needs to be updated.
    /// </summary>
    public class NavigateToViewEvent : PubSubEvent<Type> { }

    /// <summary>
    /// Used to pass a <see cref="Booking"/> to the <see cref="AddBookingViewModel"/> to display the details of the 
    /// booking.
    /// </summary>
    public class DisplayBookingEvent : PubSubEvent<Booking> { }

    /// <summary>
    /// Used to pass an <see langword="int"/> representing a <see cref="Student"/> or <see cref="Parent"/> id to the 
    /// relevant viewmodel to display the associated information. Allows the user to view a search result in the appropriate 
    /// view.
    /// </summary>
    public class LoadFromIdEvent : PubSubEvent<int> { }
}

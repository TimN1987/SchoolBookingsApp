using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SchoolBookingApp.MVVM.Viewmodel;

namespace SchoolBookingApp.MVVM.Services
{
    /// <summary>
    /// Used to update the <see cref="MainViewModel"/> when the view needs to be updated.
    /// </summary>
    public class NavigateToViewEvent : PubSubEvent<Type> { }
}

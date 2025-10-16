using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SchoolBookingApp.MVVM.Viewmodel;
using SchoolBookingApp.MVVM.View;
using System.Windows.Data;

namespace SchoolBookingApp.Converters;

/// <summary>
/// Provides a conversion from the <see cref="MainViewModel.CurrentView"/> to set the IsEnabled property for buttons in 
/// the <see cref="MainWindow"/>. Implements the <see cref="IValueConverter"/> interface.
/// </summary>
public class ViewToEnabledConverter : IValueConverter
{
    /// <summary>
    /// Used to enable or disable a button based on whether the <see cref="MainViewModel.CurrentView"/> is the same as 
    /// the view that the given button navigates to. Ensures that the button that navigates to the displayed view is 
    /// disabled.
    /// </summary>
    /// <param name="value">The view currently displayed in the <see cref="MainWindow"/>.</param>
    /// <param name="parameter">The type of view that the button navigates to.</param>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is UserControl currentView && parameter is Type targetViewType)
            return currentView.GetType() != targetViewType;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

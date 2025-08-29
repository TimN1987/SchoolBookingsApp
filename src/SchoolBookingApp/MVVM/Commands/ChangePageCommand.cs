using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using SchoolBookingApp.MVVM.Services;

namespace SchoolBookingApp.MVVM.Commands
{
    public class ChangePageCommand<TView>(IEventAggregator eventAggregator) : ICommand where TView : UserControl
    {
        private readonly IEventAggregator _eventAggregator = eventAggregator
            ?? throw new ArgumentNullException(nameof(eventAggregator));

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) =>
            _eventAggregator.GetEvent<NavigateToViewEvent>().Publish(typeof(TView));

        public event EventHandler? CanExecuteChanged;
    }
}

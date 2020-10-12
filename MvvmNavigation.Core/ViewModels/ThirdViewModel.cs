using BuildIt.Navigation;
using BuildIt.Navigation.Messages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmNavigation.ViewModels
{
    [Register]
    public class ThirdViewModel: ObservableObject
    {
        [EventMessage(typeof(CloseMessage))]
        public event EventHandler ViewModelDone;
        public string Title { get; } = "Third Page - VM";

        public void Done()
        {
            ViewModelDone?.Invoke(this, EventArgs.Empty);
        }
    }
}

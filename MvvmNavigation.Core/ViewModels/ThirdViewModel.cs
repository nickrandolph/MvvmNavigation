using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MvvmNavigation.ViewModels
{
    public class ThirdViewModel: ObservableObject
    {
        public event EventHandler ViewModelDone;
        public string Title { get; } = "Third Page - VM";

        public void Done()
        {
            ViewModelDone?.Invoke(this, EventArgs.Empty);
        }
    }
}

using BuildIt.Navigation;
using BuildIt.Navigation.Messages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace MvvmNavigation.ViewModels
{
    [Register]
    public class SecondViewModel : ObservableObject
    {
        private int count;

        [EventMessage(typeof(CloseMessage))]
        public event EventHandler ViewModelDone;

        public string Title { get; } = "Second Page - VM";

        public int Count { get => count; set => SetProperty(ref count,value); }
        public void Done()
        {
            ViewModelDone?.Invoke(this, EventArgs.Empty);
        }

        public void Increment()
        {
            Count++;
        }
    }


}

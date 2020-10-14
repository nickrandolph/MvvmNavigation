using BuildIt.Navigation;
using BuildIt.Navigation.Messages;
using System;

namespace MvvmNavigation.ViewModels
{
    [Register]
    public class FifthViewModel
    {
        [EventMessage(typeof(CloseMessage))]
        public event EventHandler FifthDone;

        public string Title => "Page 5";

        public void RaiseFifthDone()
        {
            FifthDone?.Invoke(this, EventArgs.Empty);
        }
    }
}

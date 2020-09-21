using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using BuildIt.Navigation.Messages;

namespace BuildIt.Navigation
{
    public interface INavigationEventService
    {
        ISubject<INavigationMessage> Messages { get; }
        void RaiseNavigationMessage(INavigationMessage navigationMessage);

        void Wire(Action<INavigationMessage> raiseMessage, object page);
        void Unwire(object page);
    }
}

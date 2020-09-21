using BuildIt.Navigation.Messages;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace BuildIt.Navigation
{
    public class NavigationEventService : INavigationEventService
    {
        public ISubject<INavigationMessage> Messages { get; } = new Subject<INavigationMessage>();
        private NavigationEvents Events { get; }

        public NavigationEventService(
        NavigationEvents events)
        {
            Events = events;
        }

        public void RaiseNavigationMessage(INavigationMessage navigationMessage)
        {
            Messages.OnNext(navigationMessage);
        }

        public void Wire(Action<INavigationMessage> raiseMessage, object page)
        {
            Events.Wire(raiseMessage, page);
        }

        public void Unwire(object page)
        {
            Events.Unwire(page);
        }

    }
}

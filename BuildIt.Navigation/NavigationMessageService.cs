using BuildIt.Navigation.Messages;
using System;
using System.Linq;

namespace BuildIt.Navigation
{
    public class NavigationMessageService
    {
        private INavigationService Navigation { get; }

        private INavigationEventService EventService { get; }

        private NavigationMessageRoutes Routes { get; }

        public NavigationMessageService(
                INavigationService navigation,
                INavigationEventService eventService,
                NavigationMessageRoutes routes)
        {
            Navigation = navigation;
            EventService = eventService;
            Routes = routes;
            EventService.Messages.Subscribe(HandleNavigationEvent);
        }


        private void HandleNavigationEvent(INavigationMessage obj)
        {
            var messageType = obj.GetType();
            var senderType = obj.Sender?.GetType();

            var route = (from r in Routes.Behaviors
                         where r.Item1 == senderType &&
                         (r.Item2 == messageType || r.Item2.IsAssignableFrom(messageType))
                         select r.Item3).FirstOrDefault();
            route?.Invoke(obj.Sender, obj, Navigation);

            if (route == null)
            {
                route = (from r in Routes.Behaviors
                         where r.Item1 == typeof(object) &&
                         r.Item2 == messageType
                         select r.Item3).FirstOrDefault();
                route?.Invoke(obj.Sender, obj, Navigation);
            }
        }


    }
}

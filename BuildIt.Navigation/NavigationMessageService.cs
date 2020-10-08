using BuildIt.Navigation.Messages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace BuildIt.Navigation
{
    public static class ServiceCollectionHelpers
    {
        public static void RegisterNavigationEventService(this IServiceCollection services)
        {
            services.AddSingleton<INavigationEventService>(sp =>
            {
                var events = sp.GetService<INavigationEvents>();
                return new NavigationEventService(events);
            });
        }

        public static void RegisterNavigationMessageService(this IServiceCollection services)
        {
            services.AddSingleton<INavigationMessageService>(sp =>
            {
                var eventService = sp.GetService<INavigationEventService>();
                var navService = sp.GetService<INavigationService>();

                var routes = sp.GetService<INavigationMessageRoutes>();

                return new NavigationMessageService(navService, eventService, routes);
            });
        }
    }

    public class NavigationMessageService: INavigationMessageService
    {
        private INavigationService Navigation { get; }

        private INavigationEventService EventService { get; }

        private INavigationMessageRoutes Routes { get; }

        public NavigationMessageService(
                INavigationService navigation,
                INavigationEventService eventService,
            INavigationMessageRoutes routes)
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

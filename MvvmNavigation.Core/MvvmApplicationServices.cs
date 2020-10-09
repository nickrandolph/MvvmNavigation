using BuildIt.Navigation;
using BuildIt.Navigation.Messages;
using Microsoft.Extensions.DependencyInjection;
using MvvmNavigation.Messages;
using MvvmNavigation.ViewModels;
using System;

namespace MvvmNavigation
{



        [ApplicationService(nameof(RegisterEvents))]
    public partial class MvvmApplicationServices:ApplicationService
    {
        partial void RegisterEvents(NavigationEvents events);


        protected override void InitializeApplicationServices(IServiceCollection serviceRegistrations)
        {
            base.InitializeApplicationServices(serviceRegistrations);

           // HelloWorldGenerated.HelloWorld.SayHello(); 

            serviceRegistrations.AddSingleton<INavigationEvents>(sp =>
            {
                var events = new NavigationEvents()

                // Explicit creation of navigation message
                .Register<MainViewModel, EventHandler>
                    ((v, a) => v.ViewModelDone += a, (v, a) => v.ViewModelDone -= a, (nav) => (s, ev) => nav(s.Complete(CompletionStates.One)))

                // Implicit creation of completed navigation message
                .RegisterMessageWithParameter<MainViewModel, CompletedWithStatusMessage<CompletionStates>, CompletionStates>
                    ((v, a) => v.ViewModelAlsoDone += a, (v, a) => v.ViewModelAlsoDone -= a, CompletionStates.Two)

                //// Implicit creation of close navigation message
                //.RegisterMessage<SecondViewModel, CloseMessage>
                //    ((v, a) => v.ViewModelDone += a, (v, a) => v.ViewModelDone -= a)

                // Explicit creation of close navigation message
                .Register<ThirdViewModel, EventHandler>
                    ((v, a) => v.ViewModelDone += a, (v, a) => v.ViewModelDone -= a, (nav) => (s, ev) => nav(s.Close()));

                RegisterEvents(events);

                return events;
            });

            serviceRegistrations.AddSingleton<INavigationMessageRoutes>(sp =>
            {
                var routes = new NavigationMessageRoutes()
                    .RegisterNavigate<MainViewModel, CompletedMessage, SecondViewModel>()
                    .Register<MainViewModel, CompletedWithStatusMessage<CompletionStates>>((vm, msg, nav) =>
                    {
                        if (msg.Parameter == CompletionStates.One)
                        {
                            nav.Navigate<SecondViewModel>();
                        }
                        else
                        {
                            nav.Navigate<ThirdViewModel>();
                        }
                    })
                    .RegisterGoBack<SecondViewModel, CloseMessage>()
                    .RegisterGoBack<CloseMessage>();

                return routes;
            });
        }
    }


}

using BuildIt.Navigation;
using BuildIt.Navigation.Messages;
using Microsoft.Extensions.DependencyInjection;
using MvvmNavigation.Messages;
using MvvmNavigation.ViewModels;
using System;

namespace MvvmNavigation
{



        [ApplicationService(nameof(RegisterEvents), nameof(RegisterServices))]
    public partial class MvvmApplicationServices:ApplicationService
    {
        partial void RegisterServices(IServiceCollection serviceRegistrations);
        partial void RegisterEvents(NavigationEvents events);


        protected override void InitializeApplicationServices(IServiceCollection serviceRegistrations)
        {
            base.InitializeApplicationServices(serviceRegistrations);

            serviceRegistrations.AddSingleton<INavigationEvents>(sp =>
            {
                var events = new NavigationEvents();
                RegisterEvents(events);
                return events;
            });

            serviceRegistrations.AddSingleton<INavigationMessageRoutes>(sp =>
            {
                var routes = new NavigationMessageRoutes()
                    .RegisterNavigate<MainViewModel, PleadTheFifthMessage, FifthViewModel>()
                    .RegisterNavigate<MainViewModel, CompletedMessage, SecondViewModel>()
                    .Register<MainViewModel, CompletedWithStatusMessage<CompletionStates>>((vm, msg, nav) =>
                    {
                        switch(msg.Parameter)
                        {
                            case CompletionStates.One:
                                nav.Navigate<MainViewModel>();
                                break;
                            case CompletionStates.Two:
                                nav.Navigate<SecondViewModel>();
                                break;
                            case CompletionStates.Three:
                                nav.Navigate<ThirdViewModel>();
                                break;
                            case CompletionStates.Four:
                                nav.Navigate<FourthViewModel>();
                                break;
                        }
                    })
                    .RegisterGoBack<SecondViewModel, CloseMessage>()
                    .RegisterGoBack<CloseMessage>();

                return routes;
            });


            RegisterServices(serviceRegistrations);
        }
    }


}

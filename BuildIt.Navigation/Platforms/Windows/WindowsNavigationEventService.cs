using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using BuildIt.Navigation.Messages;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BuildIt.Navigation
{
    public interface IViewModelToViewMapping
    {
        Type ViewFromViewModel<TViewModel>();

        Type ViewFromViewModel(Type viewModelType);
    }

    public static class WindowsServiceCollectionHelpers
    {
        public static void RegisterNavigationService(this IServiceCollection services, Frame rootFrame)
        {
            services.AddSingleton<INavigationService>(sp =>
            {
                var eventService = sp.GetService<INavigationEventService>();
                var mapping = sp.GetService<IViewModelToViewMapping>();
                var nav = new WindowsNavigationEventService(rootFrame, eventService, mapping);
                return nav;
            });
        }

        public static void RegisterAllServices(this IServiceCollection services, Frame rootFrame, Action<WindowsViewModelToViewMapping> registerMappings)
        {
            services.AddSingleton<IViewModelToViewMapping>(sp =>
            {
                var nav = new WindowsViewModelToViewMapping();
                registerMappings?.Invoke(nav);
                return nav;
            });

            services.RegisterNavigationEventService();
            services.RegisterNavigationService(rootFrame);
            services.RegisterNavigationMessageService();
        }
    }

    public class WindowsViewModelToViewMapping : IViewModelToViewMapping
    {
        private IDictionary<Type, Type> ViewModelToPageMap { get; } = new Dictionary<Type, Type>();

        public Type ViewFromViewModel<TViewModel>()
        {
            return ViewFromViewModel(typeof(TViewModel));
        }

        public Type ViewFromViewModel(Type viewModelType)
        {
            return ViewModelToPageMap[viewModelType];
        }

        public WindowsViewModelToViewMapping RegisterForNavigation<TPage, TViewModel>() where TPage : Page
        {
            ViewModelToPageMap[typeof(TViewModel)] = typeof(TPage);
            return this;
        }
    }



    public class WindowsNavigationEventService : INavigationService
    {
        private Frame NavigationFrame { get; }

        private INavigationEventService EventService { get; }

        private IViewModelToViewMapping Mapping { get; }

        public WindowsNavigationEventService(
            Frame navigationFrame,
            INavigationEventService eventService,
            IViewModelToViewMapping mapping)
        {
            NavigationFrame = navigationFrame;
            NavigationFrame.Navigated += OnNavigated;
            EventService = eventService;
            Mapping = mapping;
        }


        public async Task GoBack()
        {
            NavigationFrame.GoBack();
        }

        public async Task Navigate<TViewModel>()
        {
            NavigationFrame.Navigate(Mapping.ViewFromViewModel<TViewModel>());
        }


        private object PreviousPage { get; set; }
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (PreviousPage != null)
            {
                EventService.Unwire((PreviousPage as Page).DataContext);
                PreviousPage = null;
            }

            PreviousPage = e.Content;
            EventService.Wire(EventService.RaiseNavigationMessage, (PreviousPage as Page).DataContext);
        }
    }
}

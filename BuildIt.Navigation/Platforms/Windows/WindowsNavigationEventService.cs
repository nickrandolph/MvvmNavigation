using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BuildIt.Navigation.Messages;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace BuildIt.Navigation
{
    public class PageViewModelConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sp = (Application.Current as INavigationApplication)?.AppService?.Services;
            var viewModelType=sp.GetService<IViewModelToViewMapping>().ViewModelFromView(value.GetType());
            var vm = sp.GetService(viewModelType);
            return vm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public interface IViewModelToViewMapping
    {
        Type ViewFromViewModel<TViewModel>();

        Type ViewFromViewModel(Type viewModelType);

        Type ViewModelFromView<TView>();
        Type ViewModelFromView(Type viewType);
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

        public Type ViewModelFromView<TView>()
        {
            return ViewModelFromView(typeof(TView));
        }

        public Type ViewModelFromView(Type viewType)
        {
            return ViewModelToPageMap.FirstOrDefault(x => x.Value == viewType).Key;
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


        private Page PreviousPage { get; set; }
        private object PreviousDataContext { get; set; }
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (PreviousDataContext != null)
            {
                PreviousPage.DataContextChanged -= WindowsNavigationEventService_DataContextChanged;
                EventService.Unwire(PreviousDataContext);
                PreviousPage = null;
                PreviousDataContext = null;
            }

            PreviousPage = e.Content as Page;
            PreviousDataContext = PreviousPage.DataContext;

            EventService.Wire(EventService.RaiseNavigationMessage, PreviousDataContext);
            PreviousPage.DataContextChanged += WindowsNavigationEventService_DataContextChanged;
        }

        private void WindowsNavigationEventService_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (PreviousDataContext != null)
            {
                EventService.Unwire(PreviousDataContext);
            }

            PreviousDataContext = PreviousPage.DataContext;
            EventService.Wire(EventService.RaiseNavigationMessage, PreviousDataContext);

        }
    }
}

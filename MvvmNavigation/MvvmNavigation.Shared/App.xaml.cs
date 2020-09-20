using Microsoft.Extensions.Logging;
using MvvmNavigation.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MvvmNavigation
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        //   private NavigationRoutes Routes { get; } = new NavigationRoutes()
        //.Register<MainViewModel, EventHandler>(
        //            (vm, act) => vm.ViewModelDone += act,
        //            (vm, act) => vm.ViewModelDone -= act,
        //            (nav) => (s, e) => nav.Navigate<SecondViewModel>())
        //.Register<SecondViewModel, EventHandler>(
        //            (vm, act) => vm.ViewModelDone += act,
        //            (vm, act) => vm.ViewModelDone -= act,
        //            (nav) => (s, e) => nav.GoBack());

        public INavigationEventService EventService { get; private set; }

        private NavigationMessageRoutes MessageRoutes { get; } = new NavigationMessageRoutes()
.RegisterNavigate<MainViewModel, CompletedMessage, SecondViewModel>()
.RegisterGoBack<SecondViewModel, CloseMessage>()
.Register<MainViewModel, CompletedWithStatusMessage<CompletionStates>>((vm, msg, nav) =>
{
    if (msg.Status == CompletionStates.One)
    {
        nav.Navigate<SecondViewModel>();
    }
    else
    {
        nav.Navigate<ThirdViewModel>();
    }
})
.RegisterGoBack<CloseMessage>();
        //.Register<MainViewModel, CompletedMessage>((vm, msg, nav) => nav.Navigate<SecondViewModel>())
        //.Register<SecondViewModel, CloseMessage>((vm, msg, nav) => nav.GoBack());


        private NavigationEvents Events { get; } = new NavigationEvents()
            .Register<MainViewModel, EventHandler>(
                        (vm, act) => vm.ViewModelDone += act,
                        (vm, act) => vm.ViewModelDone -= act,
                        (nav) => (s, e) => nav.OnNext(s.Complete(CompletionStates.One))
            )

            .Register<MainViewModel, EventHandler>(
                        (vm, act) => vm.ViewModelAlsoDone += act,
                        (vm, act) => vm.ViewModelAlsoDone -= act,
                        (nav) => (s, e) => nav.OnNext(s.Complete(CompletionStates.Two))
            )
            .Register<SecondViewModel, EventHandler>(
                        (vm, act) => vm.ViewModelDone += act,
                        (vm, act) => vm.ViewModelDone -= act,
                        (nav) => (s, e) => nav.OnNext(s.Close()))
            .Register<ThirdViewModel, EventHandler>(
                        (vm, act) => vm.ViewModelDone += act,
                        (vm, act) => vm.ViewModelDone -= act,
                        (nav) => (s, e) => nav.OnNext(s.Close()));

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            ConfigureFilters(global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);

            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Windows.UI.Xaml.Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                //var navService = new WindowsNavigationService(rootFrame, Routes)
                //    .RegisterForNavigation<MainPage, MainViewModel>()
                //    .RegisterForNavigation<SecondPage, SecondViewModel>();

                EventService = new WindowsNavigationEventService(rootFrame, Events, MessageRoutes)
    .RegisterForNavigation<MainPage, MainViewModel>()
    .RegisterForNavigation<SecondPage, SecondViewModel>()
        .RegisterForNavigation<ThirdPage, ThirdViewModel>();




                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Windows.UI.Xaml.Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Windows.UI.Xaml.Window.Current.Activate();
            }
        }




        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        /// <summary>
        /// Configures global logging
        /// </summary>
        /// <param name="factory"></param>
        static void ConfigureFilters(ILoggerFactory factory)
        {
            factory
                .WithFilter(new FilterLoggerSettings
                    {
                        { "Uno", LogLevel.Warning },
                        { "Windows", LogLevel.Warning },

						// Debug JS interop
						// { "Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug },

						// Generic Xaml events
						// { "Windows.UI.Xaml", LogLevel.Debug },
						// { "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
						// { "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.UIElement", LogLevel.Debug },

						// Layouter specific messages
						// { "Windows.UI.Xaml.Controls", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
						// { "Windows.Storage", LogLevel.Debug },

						// Binding related messages
						// { "Windows.UI.Xaml.Data", LogLevel.Debug },

						// DependencyObject memory references tracking
						// { "ReferenceHolder", LogLevel.Debug },

						// ListView-related messages
						// { "Windows.UI.Xaml.Controls.ListViewBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.ListView", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.GridView", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.VirtualizingPanelLayout", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.NativeListViewBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.Controls.ListViewBaseSource", LogLevel.Debug }, //iOS
						// { "Windows.UI.Xaml.Controls.ListViewBaseInternalContainer", LogLevel.Debug }, //iOS
						// { "Windows.UI.Xaml.Controls.NativeListViewBaseAdapter", LogLevel.Debug }, //Android
						// { "Windows.UI.Xaml.Controls.BufferViewCache", LogLevel.Debug }, //Android
						// { "Windows.UI.Xaml.Controls.VirtualizingPanelGenerator", LogLevel.Debug }, //WASM
					}
                )
#if DEBUG
                .AddConsole(LogLevel.Debug);
#else
                .AddConsole(LogLevel.Information);
#endif
        }
    }

    public enum CompletionStates
    {
        One,
        Two
    }

    public class WindowsNavigationService : INavigationService
    {
        private Frame NavigationFrame { get; }
        private NavigationRoutes Routes { get; }
        public IDictionary<Type, Type> ViewModelToPageMap { get; } = new Dictionary<Type, Type>();

        public WindowsNavigationService(Frame navigationFrame, NavigationRoutes routes)
        {
            NavigationFrame = navigationFrame;
            NavigationFrame.Navigated += OnNavigated;
            Routes = routes;
        }

        public async Task GoBack()
        {
            NavigationFrame.GoBack();
        }

        public async Task Navigate<TViewModel>()
        {
            NavigationFrame.Navigate(ViewModelToPageMap[typeof(TViewModel)]);
        }

        public WindowsNavigationService RegisterForNavigation<TPage, TViewModel>() where TPage : Page
        {
            ViewModelToPageMap[typeof(TViewModel)] = typeof(TPage);
            return this;
        }

        private object PreviousPage { get; set; }
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (PreviousPage != null)
            {
                Routes.Unwire((PreviousPage as Page).DataContext);
                PreviousPage = null;
            }

            PreviousPage = e.Content;
            Routes.Wire(this, (PreviousPage as Page).DataContext);
        }
    }

    public interface INavigationEventService
    {
        void RaiseNavigationMessage<TNavigationMessage>(TNavigationMessage navigationMessage) where TNavigationMessage:INavigationMessage;
    }


    public class WindowsNavigationEventService : INavigationService, INavigationEventService
    {
        private Frame NavigationFrame { get; }
        private ISubject<INavigationMessage> Messages { get; } = new Subject<INavigationMessage>();
        private NavigationEvents Events { get; }
        private NavigationMessageRoutes Routes { get; }
        public IDictionary<Type, Type> ViewModelToPageMap { get; } = new Dictionary<Type, Type>();



        public WindowsNavigationEventService(
            Frame navigationFrame,
            NavigationEvents events,
            NavigationMessageRoutes routes)
        {
            NavigationFrame = navigationFrame;
            NavigationFrame.Navigated += OnNavigated;
            Events = events;
            Routes = routes;
            Messages.Subscribe(HandleNavigationEvent);

        }

        public void RaiseNavigationMessage<TNavigationMessage>(TNavigationMessage navigationMessage) 
            where TNavigationMessage : INavigationMessage
            {
            Messages.OnNext(navigationMessage);
            }


    private void HandleNavigationEvent(INavigationMessage obj)
        {
            var messageType = obj.GetType();
            var senderType = obj.Sender?.GetType();

            var route = (from r in Routes.Behaviours
                         where r.Item1 == senderType &&
                         (r.Item2 == messageType|| r.Item2.IsAssignableFrom(messageType))
                         select r.Item3).FirstOrDefault();
            route?.Invoke(obj.Sender, obj, this);

            if (route == null)
            {
                route = (from r in Routes.Behaviours
                             where r.Item1 == typeof(object) &&
                             r.Item2 == messageType
                             select r.Item3).FirstOrDefault();
                route?.Invoke(obj.Sender, obj, this);
            }

            //if(obj is CompletedMessage completed)
            //{
            //    if(completed.Sender is MainViewModel)
            //    {
            //        await Navigate<SecondViewModel>();
            //    }
            //}
            //if(obj is CloseMessage)
            //{
            //    await GoBack();
            //}
        }


        public async Task GoBack()
        {
            NavigationFrame.GoBack();
        }

        public async Task Navigate<TViewModel>()
        {
            NavigationFrame.Navigate(ViewModelToPageMap[typeof(TViewModel)]);
        }

        public WindowsNavigationEventService RegisterForNavigation<TPage, TViewModel>() where TPage : Page
        {
            ViewModelToPageMap[typeof(TViewModel)] = typeof(TPage);
            return this;
        }

        private object PreviousPage { get; set; }
        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (PreviousPage != null)
            {
                Events.Unwire((PreviousPage as Page).DataContext);
                PreviousPage = null;
            }

            PreviousPage = e.Content;
            Events.Wire(this.Messages, (PreviousPage as Page).DataContext);
        }
    }

    public interface INavigationMessage
    {
        object Sender { get; }
    }

    public abstract class BaseMessage : INavigationMessage
    {
        public object Sender { get; set; }

        protected BaseMessage(object sender)
        {
            Sender = sender;
        }
    }

    public static class ObjectHelpers
    {
        public static INavigationMessage Complete(this object sender)
        {
            return new CompletedMessage(sender);
        }
        public static INavigationMessage Complete<TStatus>(this object sender, TStatus status)
        {
            return new CompletedWithStatusMessage<TStatus>(sender, status);
        }
        public static INavigationMessage Close(this object sender)
        {
            return new CloseMessage(sender);
        }
    }


    public class CompletedMessage : BaseMessage
    {
        public CompletedMessage(object sender) : base(sender) { }

    }

    public class CompletedWithStatusMessage<TStatus> : BaseMessage
    {
        public TStatus Status { get; }
        public CompletedWithStatusMessage(object sender, TStatus status) : base(sender)
        {
            Status = status;
        }

    }

    public class StateMessage : CompletedWithStatusMessage<CompletionStates>
    {
        public StateMessage(object sender, CompletionStates status) : base(sender, status)
        {
        }
    }


    public class CloseMessage : BaseMessage
    {
        public CloseMessage(object sender) : base(sender) { }

    }
}

using BuildIt.Navigation.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildIt.Navigation
{
    public class NavigationEvents
    {
        private IList<Tuple<Type, IApplicationBehavior<Action<INavigationMessage>>>> Behaviors { get; } = new List<Tuple<Type, IApplicationBehavior<Action<INavigationMessage>>>>();

        public NavigationEvents RegisterMessageWithParameter<TViewModel, TMessage, TParameter>(
            Action<TViewModel, EventHandler> init,
            Action<TViewModel, EventHandler> deinit,
            TParameter parameter) 
                    where TMessage : INavigationMessageWithParameter<TParameter>, new()
        {
            var actionFactory = new Func<Action<INavigationMessage>, EventHandler>(nav =>
            {
                return new EventHandler((s, e) =>
                {
                    var msg = new TMessage();
                    msg.Sender = s;
                    msg.Parameter = parameter;
                    nav(msg);
                });
            });

            Behaviors.Add(
                new Tuple<Type, IApplicationBehavior<Action<INavigationMessage>>>(
                    typeof(TViewModel),
                    Behavior<Action<INavigationMessage>, TViewModel, EventHandler>.Create(init, deinit, actionFactory)));
            return this;
        }

        public NavigationEvents RegisterMessage<TViewModel, TMessage>(
            Action<TViewModel, EventHandler> init,
            Action<TViewModel, EventHandler> deinit) where TMessage : INavigationMessage, new()
        {
            var actionFactory = new Func<Action<INavigationMessage>, EventHandler>(nav =>
            {
                return new EventHandler((s, e) =>
                {
                    var msg = new TMessage();
                    msg.Sender = s;
                    nav(msg);
                });
            });

            Behaviors.Add(
                new Tuple<Type, IApplicationBehavior<Action<INavigationMessage>>>(
                    typeof(TViewModel),
                    Behavior<Action<INavigationMessage>, TViewModel, EventHandler>.Create(init, deinit, actionFactory)));
            return this;
        }

        public NavigationEvents Register<T, THandler>(Action<T, THandler> init, Action<T, THandler> deinit, Func<Action<INavigationMessage>, THandler> actionFactory)
        {
            Behaviors.Add(
                new Tuple<Type, IApplicationBehavior<Action<INavigationMessage>>>(
                    typeof(T),
                    Behavior<Action<INavigationMessage>, T, THandler>.Create(init, deinit, actionFactory)));
            return this;
        }

        public void Wire(Action<INavigationMessage> raiseMessage, object page)
        {
            var typeOfPage = page.GetType();
            var behs = Behaviors.Where(x => x.Item1 == typeOfPage).Select(x => x.Item2);
            foreach (var Behavior in behs)
            {
                Behavior.Attach(raiseMessage, page);
            }
        }

        public void Unwire(object page)
        {
            var typeOfPage = page.GetType();
            var behs = Behaviors.Where(x => x.Item1 == typeOfPage).Select(x => x.Item2);
            foreach (var Behavior in behs)
            {
                Behavior.Detach();
            }
        }
    }
}

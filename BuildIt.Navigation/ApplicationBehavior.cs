using System;
using System.Collections.Generic;
using System.Text;

namespace BuildIt.Navigation
{

    public class Behavior<TService, T, THandler> : IApplicationBehavior<TService>
    {
        private Action<T, THandler> Init { get; set; }
        private Action<T, THandler> Deinit { get; set; }

        private Func<TService, THandler> ActionFactory { get; set; }

        private T attachedEntity;
        private THandler action;

        public void Attach(TService service, object entity)
        {
            if (attachedEntity != null)
            {
                Detach();
            }

            action = ActionFactory(service);
            attachedEntity = (T)entity;
            Init(attachedEntity, action);
        }

        public void Detach()
        {
            if (attachedEntity == null)
            {
                return;
            }

            Deinit(attachedEntity, action);
        }

        private Behavior() { }

        public static Behavior<TService, T, THandler> Create(
            Action<T, THandler> init,
            Action<T, THandler> deinit,
            Func<TService, THandler> actionFactory)
        {
            return new Behavior<TService, T, THandler>()
            {
                Init = init,
                Deinit = deinit,
                ActionFactory = actionFactory
            };
        }
    }
}

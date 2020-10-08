using BuildIt.Navigation.Messages;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;


namespace BuildIt.Navigation.Behaviors
{
    public sealed class NavigationMessageAction : DependencyObject, IAction
    {
        public Type MessageType { get; set; }

        public object MessageParameter { get; set; }


        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="sender">The <see cref="System.Object"/> that is passed to the action by the behavior. Generally this is <seealso cref="Microsoft.Xaml.Interactivity.IBehavior.AssociatedObject"/> or a target object.</param>
        /// <param name="parameter">The value of this parameter is determined by the caller.</param>
        /// <returns>True if updating the property value succeeds; else false.</returns>
        public object Execute(object sender, object parameter)
        {
            var messageSender = (sender as FrameworkElement).DataContext;

            var args = new List<object>
            {
                messageSender
            };
            if (MessageParameter != null)
            {
                var constructor = MessageType.GetConstructors().FirstOrDefault();
                var paraType = constructor.GetParameters().Skip(1).FirstOrDefault();
                if (paraType != null)
                {
                    if (paraType.ParameterType != MessageParameter.GetType())
                    {
                        if (paraType.ParameterType.IsEnum)
                        {
                            MessageParameter = Enum.Parse(paraType.ParameterType, MessageParameter + "");
                        }
                        else
                        {
                            MessageParameter = Convert.ChangeType(MessageParameter, paraType.ParameterType);
                        }
                    }
                    args.Add(MessageParameter);
                }
            }

            var message = Activator.CreateInstance(MessageType, args.ToArray()) as INavigationMessage;

            (Application.Current as INavigationApplication)?.AppService?.Services.GetService<INavigationEventService>().RaiseNavigationMessage(message);

            return true;
        }


    }
}

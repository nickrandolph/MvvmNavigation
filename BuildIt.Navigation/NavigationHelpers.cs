using BuildIt.Navigation.Messages;
using System;

namespace BuildIt.Navigation
{

    public static class NavigationHelpers
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
}

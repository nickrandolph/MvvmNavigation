using BuildIt.Navigation.Messages;

namespace MvvmNavigation.Messages
{
    public class StateMessage : CompletedWithStatusMessage<CompletionStates>
    {
        public StateMessage(object sender, CompletionStates status) : base(sender, status)
        {
        }
    }
}

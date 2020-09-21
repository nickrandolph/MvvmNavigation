namespace BuildIt.Navigation.Messages
{
    public abstract class BaseMessage : INavigationMessage
    {
        public object Sender { get; set; }

        protected BaseMessage(object sender)
        {
            Sender = sender;
        }
    }
}

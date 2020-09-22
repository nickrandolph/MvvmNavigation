namespace BuildIt.Navigation.Messages
{
    public class CompletedWithStatusMessage<TStatus> : BaseMessage, INavigationMessageWithParameter<TStatus>
    {
        public TStatus Parameter { get; set; }

        public CompletedWithStatusMessage() : base() { }

        public CompletedWithStatusMessage(object sender, TStatus parameter) : base(sender)
        {
            Parameter =parameter;
        }

    }
}

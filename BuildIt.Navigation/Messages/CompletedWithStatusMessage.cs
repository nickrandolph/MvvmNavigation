namespace BuildIt.Navigation.Messages
{
    public class CompletedWithStatusMessage<TStatus> : BaseMessage
    {
        public TStatus Status { get; }
        public CompletedWithStatusMessage(object sender, TStatus status) : base(sender)
        {
            Status = status;
        }

    }
}

namespace BuildIt.Navigation.Messages
{
    public interface INavigationMessage
    {
        object Sender { get; set; }
    }

    public interface INavigationMessageWithParameter<TParameter>:INavigationMessage
    {
        TParameter Parameter { get; set; }
    }
}

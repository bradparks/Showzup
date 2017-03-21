namespace Silphid.Showzup.Navigation
{
    public interface INavigatable
    {
        bool CanHandle(NavigationCommand command);
        void Handle(NavigationCommand command);
    }
}
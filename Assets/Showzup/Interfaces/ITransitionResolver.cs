namespace Silphid.Showzup
{
    public interface ITransitionResolver
    {
        Transition Resolve(IView source, IView target);
    }
}
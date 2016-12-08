using UniRx;

namespace Silphid.Showzup
{
    public interface IShowable
    {
        IObservable<Unit> Show();
        IObservable<Unit> Hide();
    }
}
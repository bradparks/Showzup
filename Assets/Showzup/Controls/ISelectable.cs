using UniRx;

namespace Silphid.Showzup
{
    public interface ISelectable
    {
        BoolReactiveProperty IsSelected { get; }
    }
}
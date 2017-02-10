using UniRx;

namespace Silphid.Showzup
{
    public interface ITransitionPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<bool> IsReady { get; }
        ReadOnlyReactiveProperty<bool> IsLoading { get; }

        IObservable<Presentation> Presenting { get; }
        IObservable<Phase> Phases { get; }
        IObservable<CompletedPhase> CompletedPhases { get; }
        IObservable<Presentation> Presented { get; }
    }
}
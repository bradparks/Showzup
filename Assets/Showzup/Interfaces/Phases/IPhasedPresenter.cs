using UniRx;

namespace Silphid.Showzup
{
    public interface IPhasedPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<bool> IsPresenting { get; }
        ReadOnlyReactiveProperty<bool> IsLoading { get; }
        IObservable<Present> PresentStarting { get; }
        IObservable<Phase> PhaseStarting { get; }
        IObservable<CompletedPhase> PhaseCompleted { get; }
        IObservable<Present> PresentCompleted { get; }
    }
}
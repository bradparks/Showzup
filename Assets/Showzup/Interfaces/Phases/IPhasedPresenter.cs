using UniRx;

namespace Silphid.Showzup
{
    public interface IPhasedPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<bool> IsPresenting { get; }
        ReadOnlyReactiveProperty<bool> IsLoading { get; }
        IObservable<Presentation> PresentStarting { get; }
        IObservable<Phase> PhaseStarting { get; }
        IObservable<CompletedPhase> PhaseCompleted { get; }
        IObservable<Presentation> PresentCompleted { get; }
    }
}
using UniRx;

namespace Silphid.Showzup
{
    public interface IPhasedPresenter : IPresenter
    {
        IObservable<Presentation> PresentationStarting { get; }
        IObservable<Phase> PhaseStarting { get; }
        IObservable<CompletedPhase> PhaseCompleted { get; }
        IObservable<Presentation> PresentationCompleted { get; }
    }
}
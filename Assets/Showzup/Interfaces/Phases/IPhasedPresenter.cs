using UniRx;

namespace Silphid.Showzup
{
    public interface IPhasedPresenter : IPresenter
    {
        IObservable<IPresentation> PresentationStarting { get; }
        IObservable<IPhase> PhaseStarting { get; }
        IObservable<CompletedPhase> PhaseCompleted { get; }
        IObservable<IPresentation> PresentationCompleted { get; }
    }
}
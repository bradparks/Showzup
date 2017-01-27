using UniRx;

namespace Silphid.Showzup
{
    public interface IPhasedPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<bool> IsPresenting { get; }
        ReadOnlyReactiveProperty<bool> IsLoading { get; }

        IObservable<Phase> PrePresentationPhase { get; }
        IObservable<Phase> DeconstructionPhase { get; }
        IObservable<Phase> LoadPhase { get; }
        IObservable<Phase> PreTransitionPhase { get; }
        IObservable<Phase> TransitionPhase { get; }
        IObservable<Phase> PostTransitionPhase { get; }
        IObservable<Phase> UnloadPhase { get; }
        IObservable<Phase> ConstructionPhase { get; }
        IObservable<Phase> PostPresentationPhase { get; }
    }
}
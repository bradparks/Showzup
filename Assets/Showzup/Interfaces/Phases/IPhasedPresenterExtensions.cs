using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public static class IPhasedPresenterExtensions
    {
        public static IObservable<TPhase> OfPhase<TPhase>(this IObservable<IPhase> This) where TPhase : IPhase =>
            This.OfType<IPhase, TPhase>();

        public static IObservable<CompletedPhase> OfPhase<TPhase>(this IObservable<CompletedPhase> This) where TPhase : IPhase =>
            This.Where(x => x.PhaseType.IsAssignableTo<TPhase>());

        public static IObservable<bool> IsPresenting(this IPhasedPresenter This) =>
            This.PresentationStarting
                .Select(_ => true)
                .Merge(This.PresentationCompleted
                    .Select(_ => false))
                .StartWith(false);

        public static IObservable<bool> IsLoading(this IPhasedPresenter This) =>
            This.PhaseStarting
                .OfPhase<LoadPhase>()
                .Select(_ => true)
                .Merge(This.PhaseCompleted
                    .OfPhase<LoadPhase>()
                    .Select(_ => false))
                .StartWith(false);
    }
}
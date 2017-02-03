using UniRx;

namespace Silphid.Showzup
{
    public static class IPhasedPresenterExtensions
    {
        public static IObservable<bool> IsPresenting(this IPhasedPresenter This) =>
            This.PresentationStarting
                .Select(_ => true)
                .Merge(This.PresentationCompleted
                    .Select(_ => false))
                .StartWith(false);

        public static IObservable<bool> IsLoading(this IPhasedPresenter This) =>
            This.PhaseStarting
                .Where(x => x.Id == PhaseId.Load)
                .Select(_ => true)
                .Merge(This.PhaseCompleted
                    .Where(x => x.Id == PhaseId.Load)
                    .Select(_ => false))
                .StartWith(false);
    }
}
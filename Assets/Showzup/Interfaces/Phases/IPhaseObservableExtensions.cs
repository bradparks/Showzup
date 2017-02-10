using Silphid.Extensions;
using UniRx;

namespace Silphid.Showzup
{
    public static class IPhaseObservableExtensions
    {
        #region IObservable<Phase>

        public static IObservable<TPhase> OfPhase<TPhase>(this IObservable<Phase> This) where TPhase : Phase =>
            This.OfType<Phase, TPhase>();

        public static IObservable<Phase> From<T>(this IObservable<Phase> This) =>
            This.Where(x => x.SourceViewModel is T || x.SourceView is T);

        public static IObservable<Phase> To<T>(this IObservable<Phase> This) =>
            This.Where(x => x.TargetViewModel is T || x.TargetViewType.IsAssignableTo<T>());

        public static IObservable<Phase> Between<TSource, TTarget>(this IObservable<Phase> This) =>
            This.From<TSource>().To<TTarget>();

        public static IObservable<TViewModel> OfSourceViewModel<TViewModel>(this IObservable<Phase> This) =>
            This.Select(x => x.SourceViewModel).OfType<object, TViewModel>();

        public static IObservable<TViewModel> OfTargetViewModel<TViewModel>(this IObservable<Phase> This) =>
            This.Select(x => x.TargetViewModel).OfType<object, TViewModel>();

        public static IObservable<TView> OfSourceView<TView>(this IObservable<Phase> This) where TView : IView =>
            This.Select(x => x.SourceView).OfType<IView, TView>();

        public static IObservable<TView> OfTargetView<TView>(this IObservable<Phase> This) where TView : IView =>
            This.Select(x => x.TargetView).OfType<IView, TView>();

        #endregion

        #region IObservable<Presentation>

        public static IObservable<Presentation> From<T>(this IObservable<Presentation> This) =>
            This.Where(x => x.SourceViewModel is T || x.SourceView is T);

        public static IObservable<Presentation> To<T>(this IObservable<Presentation> This) =>
            This.Where(x => x.TargetViewModel is T || x.TargetViewType.IsAssignableTo<T>());

        public static IObservable<Presentation> Between<TSource, TTarget>(this IObservable<Presentation> This) =>
            This.From<TSource>().To<TTarget>();

        public static IObservable<Presentation> WhereSourceViewIs<TView>(this IObservable<Presentation> This) where TView : IView =>
            This.Where(x => x.SourceViewType.IsAssignableTo<TView>());

        public static IObservable<Presentation> WhereTargetViewIs<TView>(this IObservable<Presentation> This) where TView : IView =>
            This.Where(x => x.TargetViewType.IsAssignableTo<TView>());

        public static IObservable<TViewModel> OfSourceViewModel<TViewModel>(this IObservable<Presentation> This) =>
            This.Select(x => x.SourceViewModel).OfType<object, TViewModel>();

        public static IObservable<TViewModel> OfTargetViewModel<TViewModel>(this IObservable<Presentation> This) =>
            This.Select(x => x.TargetViewModel).OfType<object, TViewModel>();

        public static IObservable<TView> OfSourceView<TView>(this IObservable<Presentation> This) where TView : IView =>
            This.Select(x => x.SourceView).OfType<IView, TView>();

        public static IObservable<TView> OfTargetView<TView>(this IObservable<Presentation> This) where TView : IView =>
            This.Select(x => x.TargetView).OfType<IView, TView>();

        #endregion

        #region IObservable<CompletedPhase>

        public static IObservable<CompletedPhase> OfPhase<TPhase>(this IObservable<CompletedPhase> This) where TPhase : Phase =>
            This.Where(x => x.PhaseType.IsAssignableTo<TPhase>());

        public static IObservable<CompletedPhase> From<T>(this IObservable<CompletedPhase> This) =>
            This.Where(x => x.SourceViewModel is T || x.SourceView is T);

        public static IObservable<CompletedPhase> To<T>(this IObservable<CompletedPhase> This) =>
            This.Where(x => x.TargetViewModel is T || x.TargetViewType.IsAssignableTo<T>());

        public static IObservable<CompletedPhase> Between<TSource, TTarget>(this IObservable<CompletedPhase> This) =>
            This.From<TSource>().To<TTarget>();

        public static IObservable<CompletedPhase> WhereSourceViewIs<TView>(this IObservable<CompletedPhase> This) where TView : IView =>
            This.Where(x => x.SourceViewType.IsAssignableTo<TView>());

        public static IObservable<CompletedPhase> WhereTargetViewIs<TView>(this IObservable<CompletedPhase> This) where TView : IView =>
            This.Where(x => x.TargetViewType.IsAssignableTo<TView>());

        public static IObservable<TViewModel> OfSourceViewModel<TViewModel>(this IObservable<CompletedPhase> This) =>
            This.Select(x => x.SourceViewModel).OfType<object, TViewModel>();

        public static IObservable<TViewModel> OfTargetViewModel<TViewModel>(this IObservable<CompletedPhase> This) =>
            This.Select(x => x.TargetViewModel).OfType<object, TViewModel>();

        public static IObservable<TView> OfSourceView<TView>(this IObservable<CompletedPhase> This) where TView : IView =>
            This.Select(x => x.SourceView).OfType<IView, TView>();

        public static IObservable<TView> OfTargetView<TView>(this IObservable<CompletedPhase> This) where TView : IView =>
            This.Select(x => x.TargetView).OfType<IView, TView>();

        #endregion
    }
}
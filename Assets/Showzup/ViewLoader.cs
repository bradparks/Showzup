using System;
using System.Linq;
using Silphid.Extensions;
using Silphid.Loadzup;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Silphid.Showzup
{
    public class ViewLoader : IViewLoader
    {
        [Inject] internal ILoader _loader;

        private readonly IViewResolver _viewResolver;
        private readonly Action<GameObject> _injectGameObject;

        public ViewLoader(IViewResolver viewResolver, Action<GameObject> injectGameObject)
        {
            _viewResolver = viewResolver;
            _injectGameObject = injectGameObject;
        }

        public IObservable<IView> Load(object input, Options options = null)
        {
            if (input == null)
            {
                Debug.Log("#Views# Returning null view for null content");
                return Observable.Return<IView>(null);
            }

            if (input is IView)
            {
                Debug.Log("#Views# Returning content itself as view");
                return Observable.Return((IView) input);
            }

            if (input is Type)
            {
                var type = (Type) input;
                if (!type.IsAssignableTo<IView>())
                    return Observable.Throw<IView>(
                            new NotSupportedException($"#Views# Input type does not implement IView: {type}"));

                return LoadByViewType(type, options);
            }

            return LoadByViewModel(input, options);
        }

        private IObservable<IView> LoadByViewType(Type viewType, Options options = null)
        {
            //Debug.Log($"#Views# Loading view of type {viewType}");

            // Resolve view mapping
            var mapping = _viewResolver.ResolveFromViewType(viewType, options);
            if (mapping == null)
                throw new InvalidOperationException($"View type {viewType.Name} not mapped");

            return LoadInternal(mapping, null);
        }

        private IObservable<IView> LoadByViewModel(object viewModel, Options options = null)
        {
            //Debug.Log($"#Views# Loading view for view model of type {viewModel}");
            // Resolve view mapping
            var mapping = _viewResolver.ResolveFromViewModelType(viewModel.GetType(), options);
            if (mapping == null)
                throw new InvalidOperationException($"ViewModel type {viewModel.GetType().Name} not mapped to any valid view");

            return LoadInternal(mapping, viewModel);
        }

        private IObservable<IView> LoadInternal(ViewMapping mapping, object viewModel)
        {
//            Debug.Log($"#Views# Loading view {mapping.ViewType} for view model {viewModel} using mapping {mapping}");
            return LoadPrefabView(mapping.ViewType, mapping.Uri)
                .Do(view => InjectView(view, viewModel))
                .ContinueWith(view => LoadLoadable(view).ThenReturn(view));
        }

        private void InjectView(IView view, object viewModel)
        {
            view.ViewModel = viewModel;
//            Debug.Log($"#Views# Initializing view {view} with view model {viewModel}");
            _injectGameObject(view.GameObject);
        }

        private IObservable<Unit> LoadLoadable(IView view) =>
            (view as ILoadable)?.Load() ?? Observable.ReturnUnit();

        #region Prefab view loading

        private IObservable<IView> LoadPrefabView(Type viewType, Uri uri)
        {
            //Debug.Log($"#Views# LoadPrefabView({viewType}, {uri})");

            return _loader.Load<GameObject>(uri)
                .Last()
                .Do(DisableAllViews)
                .Select(Object.Instantiate)
                .DoOnError(ex => Debug.LogError(
                    $"Failed to load view {viewType} from {uri} with error:{Environment.NewLine}{ex}"))
                .Select(x => GetViewFromPrefab(x, viewType));
        }

        private void DisableAllViews(GameObject obj)
        {
            obj.GetComponents<IView>().ForEach(x => x.IsActive = false);
        }

        private static IView GetViewFromPrefab(GameObject gameObject, Type viewType)
        {
            var view = (IView) gameObject.GetComponent(viewType);

            if (view == null)
                throw new InvalidOperationException(
                    $"Loaded prefab {gameObject.name} has no view component of type {viewType.Name}");

            return view;
        }

        #endregion
    }
}
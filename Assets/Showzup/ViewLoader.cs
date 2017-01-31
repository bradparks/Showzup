using System;
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

        private readonly Action<GameObject> _injectGameObject;

        public ViewLoader(Action<GameObject> injectGameObject)
        {
            _injectGameObject = injectGameObject;
        }

        public IObservable<IView> Load(ViewInfo viewInfo)
        {
            if (viewInfo.View != null)
                return Load(viewInfo.ViewModel, viewInfo.View);

            if (viewInfo.ViewType != null && viewInfo.Uri != null)
                return Load(viewInfo.ViewModel, viewInfo.ViewType, viewInfo.Uri);

            throw new InvalidOperationException("Must specify either view instance to load or view type and URI");
        }

        private IObservable<IView> Load(object viewModel, IView view)
        {
            return Observable.Return(view)
                .Do(x => InjectView(x, viewModel))
                .ContinueWith(x => LoadLoadable(x).ThenReturn(view));
        }

        private IObservable<IView> Load(object viewModel, Type viewType, Uri uri)
        {
//            Debug.Log($"#Views# Loading view {viewInfo.ViewType} for view model {viewModel} using viewInfo {viewInfo}");
            return LoadPrefabView(viewType, uri)
                .Do(view => InjectView(view, viewModel))
                .ContinueWith(view => LoadLoadable(view).ThenReturn(view));
        }

        private void InjectView(IView view, object viewModel)
        {
//            Debug.Log($"#Views# Initializing view {view} with view model {viewModel}");

            if (viewModel != null)
                view.ViewModel = viewModel;

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
                .Select(InstantiatePrefabInstanceIfEditor)
                .Do(DisableAllViews)
                .Select(Instantiate)
                .DoOnError(ex => Debug.LogError(
                    $"Failed to load view {viewType} from {uri} with error:{Environment.NewLine}{ex}"))
                .Select(x => GetViewFromPrefab(x, viewType));
        }

        private GameObject InstantiatePrefabInstanceIfEditor(GameObject obj)
        {
#if UNITY_EDITOR
            obj = Object.Instantiate(obj);
#endif
            return obj;
        }

        private GameObject Instantiate(GameObject prefab)
        {
            var instance = Object.Instantiate(prefab);
#if UNITY_EDITOR
            Object.Destroy(prefab);
#endif
            return instance;
        }

        private void DisableAllViews(GameObject obj)
        {
            obj.GetComponents<IView>().ForEach(x => x.IsActive = false);
        }

        private IView GetViewFromPrefab(GameObject gameObject, Type viewType)
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
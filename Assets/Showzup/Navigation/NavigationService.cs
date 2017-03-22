using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Silphid.Showzup.Navigation
{
    public class NavigationService : INavigationService
    {
        public NavigationService()
        {
            Observable
                .EveryUpdate()
                .Select(_ => EventSystem.current.currentSelectedGameObject)
                .DistinctUntilChanged()
                .Subscribe(x => Debug.Log($"#Select# Selection changed to: {x?.name}"));
        }

        private IEnumerable<INavigatable> Navigatables
        {
            get
            {
                var current = EventSystem.current.currentSelectedGameObject;
                while (current != null)
                {
                    var navigatable = current.GetComponent<INavigatable>();
                    if (navigatable != null)
                        yield return navigatable;

                    current = current.transform.parent?.gameObject;
                }
            }
        }

        public bool CanHandle(NavigationCommand command) =>
            Navigatables.FirstOrDefault(x => x.CanHandle(command)) != null;

        public void Handle(NavigationCommand command)
        {
            Navigatables
                .FirstOrDefault(x => x.CanHandle(command))
                ?.Handle(command);
        }
    }
}
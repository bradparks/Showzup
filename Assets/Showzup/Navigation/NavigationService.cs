using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Silphid.Showzup.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly List<INavigatable> _navigatables = new List<INavigatable>();

        public IDisposable Push(INavigatable navigatable)
        {
            _navigatables.Add(navigatable);
            return Disposable.Create(() => Pop(navigatable));
        }

        private void Pop(INavigatable navigatable)
        {
            var index = _navigatables.IndexOf(navigatable);
            if (index == -1)
                return;

            // Remove item and all following items
            for (int i = _navigatables.Count - 1; i >= index; i--)
                _navigatables.RemoveAt(i);
        }

        public bool CanHandle(NavigationCommand command) =>
            _navigatables.LastOrDefault(x => x.CanHandle(command)) != null;

        public void Handle(NavigationCommand command)
        {
            _navigatables
                .LastOrDefault(x => x.CanHandle(command))
                ?.Handle(command);
        }
    }
}
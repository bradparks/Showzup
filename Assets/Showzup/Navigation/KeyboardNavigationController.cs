using UniRx;
using UnityEngine;

namespace Silphid.Showzup.Navigation
{
    public class KeyboardNavigationController
    {
        public KeyboardNavigationController(INavigationService navigationService)
        {
            Observable
                .EveryUpdate()
                .Subscribe(_ =>
                {
                    var command = NavigationCommand.None;

                    if (Input.GetKeyDown(KeyCode.DownArrow))        command = NavigationCommand.Down;
                    else if (Input.GetKeyDown(KeyCode.UpArrow))     command = NavigationCommand.Up;
                    else if (Input.GetKeyDown(KeyCode.LeftArrow))   command = NavigationCommand.Left;
                    else if (Input.GetKeyDown(KeyCode.RightArrow))  command = NavigationCommand.Right;
                    else if (Input.GetKeyDown(KeyCode.Return))      command = NavigationCommand.Select;
                    else if (Input.GetKeyDown(KeyCode.Escape))      command = NavigationCommand.Back;

                    if (command != NavigationCommand.None && navigationService.CanHandle(command))
                        navigationService.Handle(command);
                });
        }
    }
}
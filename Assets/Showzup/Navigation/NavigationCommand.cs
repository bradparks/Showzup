using Silphid.Extensions.DataTypes;

namespace Silphid.Showzup.Navigation
{
    public class NavigationCommand : ObjectEnum<NavigationCommand>
    {
        public static readonly NavigationCommand None = new NavigationCommand();
        public static readonly NavigationCommand Select = new NavigationCommand();
        public static readonly NavigationCommand Back = new NavigationCommand();
        public static readonly NavigationCommand Left = new NavigationCommand {Orientation = NavigationOrientation.Horizontal, Offset = -1};
        public static readonly NavigationCommand Right = new NavigationCommand {Orientation = NavigationOrientation.Horizontal, Offset = +1};
        public static readonly NavigationCommand Up = new NavigationCommand {Orientation = NavigationOrientation.Vertical, Offset = -1};
        public static readonly NavigationCommand Down = new NavigationCommand {Orientation = NavigationOrientation.Vertical, Offset = +1};

        public NavigationOrientation Orientation { get; private set; } = NavigationOrientation.None;
        public int Offset { get; private set; }
    }
}
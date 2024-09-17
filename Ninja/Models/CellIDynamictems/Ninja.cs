using System.Linq;

namespace Models.CellIDynamictems
{
    public class Ninja : CellDynamicItem
    {
        public int Shurikens { get; set; } = 3;
        public Direction Direction { get; set; } = Direction.South;
        public bool BreakerMode { get; set; } = false;
        public bool IsMirrored { get; set; } = false;

        //Save default order
        public Dictionary<Direction, Location> DefaultMoveOrder
        {
            get {
                return new Dictionary<Direction, Location>(4)
                {
                    { Direction.South, new Location(X, Y + 1) },
                    { Direction.East, new Location(X + 1, Y) },
                    { Direction.North, new Location(X, Y - 1) },
                    { Direction.West, new Location(X - 1, Y) }
                };
            }
        }

        public Dictionary<Direction, Location> CurrentMoveOrder
        {
            get {
                Dictionary<Direction, Location> moveOrder = new Dictionary<Direction, Location>(4);
                moveOrder.Add(Direction, DefaultMoveOrder[Direction]);

                int itemCount = DefaultMoveOrder.Count();
                for(int index = 0; index < itemCount; index++)
                {
                    int selectedIndex = index;
                    if(IsMirrored)
                        selectedIndex = itemCount - 1 - index;

                    Direction direction = DefaultMoveOrder.Keys.ElementAt(selectedIndex);
                    moveOrder.Add(direction, DefaultMoveOrder[direction]);
                }

                return moveOrder;
            }
        }
    }
}

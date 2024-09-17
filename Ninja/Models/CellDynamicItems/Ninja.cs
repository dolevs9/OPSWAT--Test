using System.Linq;

namespace Models.CellDynamicItems
{
    public class Ninja : CellDynamicItem
    {
        public string Name { get; set; } = Guid.NewGuid().ToString();
        public int Shurikens { get; set; } = 3;
        public Direction Direction { get; set; } = Direction.South;
        public bool BreakerMode { get; set; } = false;
        public bool IsMirrored { get; set; } = false;
        public bool IsAlive { get; set; } = true;

        //Save default order
        public List<(Direction dircection, int X,int Y)> DefaultMoveOrder
        {
            get {
                return new List<(Direction dircection, int X, int Y)>(4)
                {
                    ( Direction.South, X, Y + 1 ),
                    ( Direction.East, X + 1, Y ),
                    ( Direction.North, X, Y - 1 ),
                    ( Direction.West, X - 1, Y ) 
                };
            }
        }

        public List<(Direction dircection, int X, int Y)> CurrentMoveOrder
        {
            get {
                List<(Direction dircection, int X, int Y)> moveOrder = new List<(Direction dircection, int X, int Y)>(4);
                moveOrder.AddRange(DefaultMoveOrder);

                if(IsMirrored)
                    moveOrder.Reverse();

                (Direction dircection, int X, int Y) currentDirection = DefaultMoveOrder.First(moveOption => moveOption.dircection == Direction);
                moveOrder.Remove(currentDirection);
                moveOrder.Insert(0,(Direction, currentDirection.X, currentDirection.Y));

                return moveOrder;
            }
        }
    }
}

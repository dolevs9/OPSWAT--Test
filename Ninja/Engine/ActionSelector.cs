using Models;
using Models.CellIDynamictems;

namespace Engine
{
    public class ActionSelector
    {
        Dictionary<string, Func<Ninja, Board, Ninja>> CellActionList = new Dictionary<string, Func<Ninja, Board, Ninja>>()
        {
            { "@" , (Ninja n, Board brd) =>
                {
                    return DefaultMoveAction(n,brd);
                }
            },

            { " " , (Ninja n, Board brd) =>
                {
                    return DefaultMoveAction(n,brd);
                }
            }
        };


        private static Ninja DefaultMoveAction(Ninja ninja, Board brd)
        {
            List<char> blockers = new List<char>(2) { '#' };

            if(ninja.Shurikens == 0)
                blockers.Add('X');

            //Save default order
            Dictionary<Direction, Location> firstLocationOrderChecker = new Dictionary<Direction, Location>(4)
            {
                { Direction.South, new Location(ninja.X,ninja.Y+1) },
                { Direction.East, new Location(ninja.X+1,ninja.Y) },
                { Direction.North, new Location(ninja.X,ninja.Y-1) },
                { Direction.West, new Location(ninja.X-1,ninja.Y) }
            };

            //Set the current ninja direction to check first
            Dictionary<Direction, Location> locationOrderChecker = new Dictionary<Direction, Location>(4);
            locationOrderChecker.Add(ninja.Direction, locationOrderChecker[ninja.Direction]);

            //Add the default order of the rest afterwards
            foreach(var item in firstLocationOrderChecker)
            {
                if(item.Key != ninja.Direction)
                    locationOrderChecker.Add(item.Key, item.Value);
            }

            //Check what nearest cell is open to move to it
            foreach(var orderChecker in locationOrderChecker)
            {
                bool canGo = true;
                char nextCellValue = brd[orderChecker.Value.X, orderChecker.Value.Y];

                foreach(var blocker in blockers)
                {
                    if(nextCellValue == blocker)
                        canGo = false;
                }

                if(canGo)
                {
                    ninja.Direction = orderChecker.Key;
                    break;
                }
            }

            return ninja;
        }


    }
}

using Models;
using Models.CellIDynamictems;

namespace Engine
{
    public class ActionSelector
    {
        Ninja ninja;
        Board brd;
        IEnumerable<Ninja> allNinjas;
        IEnumerable<Bomb> bombs;
        bool shouldContinue;

        Dictionary<Func<bool>, Action> CellActionList;

        private Dictionary<Func<bool>, Action> LoadActionList()
        {


            return new Dictionary<Func<bool>, Action>()
            {
                {       //Activate Bombs
                        () =>   char.IsDigit(brd[ninja.X, ninja.Y-1]) ||
                                char.IsDigit(brd[ninja.X+1, ninja.Y]) ||
                                char.IsDigit(brd[ninja.X, ninja.Y+1]) ||
                                char.IsDigit(brd[ninja.X-1, ninja.Y]),
                        () => {
                            bombs.Where(bomb =>  bomb.X == ninja.X-1 && bomb.Y == ninja.Y ||
                                                 bomb.X == ninja.X-1 && bomb.Y == ninja.Y ||
                                                 bomb.X == ninja.X && bomb.Y == ninja.Y-1 ||
                                                 bomb.X == ninja.X && bomb.Y == ninja.Y+1)
                                 .Select(bomb => bomb.IsActive = true);
                            shouldContinue = true;
                        }

                },

                {       () => brd[ninja.X, ninja.Y] == '@',
                        () => DefaultMoveAction(ninja,brd)
                },
                {       //Fight
                        //Check if there are 2 ninjas in same position that matches our current ninja position, since our current ninja is also in the list
                        () => allNinjas.Where(otherNinja => otherNinja.X == ninja.X && otherNinja.Y == ninja.Y).Count() > 1,
                        () => Fight()
                },
                {       //Throw Shurikans
                        () => CanThrowShurikan(ninja, brd),
                        () => ThrowShurikan(ninja,brd)
                },
                {       () => brd[ninja.X, ninja.Y] == ' ',
                        () => DefaultMoveAction(ninja,brd)
                },
                {       () => brd[ninja.X, ninja.Y] == 'S',
                        () => ninja.Direction = Direction.South
                },
                {       () => brd[ninja.X, ninja.Y] == 'E',
                        () => ninja.Direction = Direction.East
                },
                {       () => brd[ninja.X, ninja.Y] == 'N',
                        () => ninja.Direction = Direction.North
                },
                {       () => brd[ninja.X, ninja.Y] == 'W',
                        () => ninja.Direction = Direction.West
                },
                {       () => brd[ninja.X, ninja.Y] == '*',
                        () =>
                        {
                            ninja.Shurikens++;
                            DefaultMoveAction(ninja,brd);
                        }
                },
                {       () => brd[ninja.X, ninja.Y] == 'B',
                        () =>
                        {
                            ninja.BreakerMode = true;
                            DefaultMoveAction(ninja,brd);
                        }
                },
    
                //Portal Items
                {       () =>
                    {
                        char[] portalLetters = {'F', 'G', 'H', 'I', 'J', 'K', 'L' };
                        return portalLetters.Where(portalLetter => portalLetter == brd[ninja.X,ninja.Y]).Count() > 0;
                    } ,
                        () =>
                    {
                        //In each cell on the board
                        brd.ScanItems((x, y, cell) =>
                        {
                            //Check if its a different position for the same letter that the ninja is on
                            if(x != ninja.X && y != ninja.Y && cell == brd[ninja.X,ninja.Y])
                            {
                                ninja.X = x;
                                ninja.Y = y;
                            }
                        });
    
                        //And after placing the the ninja move as usual
                        DefaultMoveAction(ninja,brd);
                    }
                }
    
    
            };
        }



        public ActionSelector(Ninja actingNinja, Board board, IEnumerable<Bomb> bmb, IEnumerable<Ninja> ninjas)
        {
            ninja = actingNinja;
            brd = board;
            bombs = bmb;
            allNinjas = ninjas;
            CellActionList = LoadActionList();
        }


        public void SelectAction()
        {
            shouldContinue = false;
            foreach(var cellActionOption in CellActionList)
            {
                if(cellActionOption.Key())
                    cellActionOption.Value();

                if(!shouldContinue)
                    break;
            }
        }


        private void DefaultMoveAction(Ninja ninja, Board brd)
        {
            List<char> blockers = new List<char>(2) { '#' };

            if(!ninja.BreakerMode)
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
        }


        private bool CanThrowShurikan(Ninja ninja, Board brd)
        {
            return false;
        }

        private void ThrowShurikan(Ninja ninja, Board brd)
        {

        }

        private void Fight()
        {

        }
    }
}

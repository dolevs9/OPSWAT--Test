using Models;
using Models.CellDynamicItems;

namespace Engine
{
    public class ActionSelector
    {
        public bool WasWinningStep { get; private set; } = false;

        Ninja ninja;
        Board brd;
        IEnumerable<Ninja> allNinjas;
        IEnumerable<Bomb> bombs;
        bool shouldContinue;
        bool ignoreLogDirection = false;

        List<(string name, Func<bool> condition, Action action)> CellActionList;

        private List<(string name, Func<bool> condition, Action action)> LoadActionList()
        {


            return new List<(string name, Func<bool> condition, Action action)>()
            {(
                      "ActivateBombs",
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
                            ignoreLogDirection = true;
                        }
                ),

                (      "Start",
                        () => brd[ninja.X, ninja.Y] == '@',
                        () => DefaultMoveAction(ninja,brd)
                ),
                (       "Fight",
                        //Check if there are 2 ninjas in same position that matches our current ninja position, since our current ninja is also in the list
                        () => allNinjas.Where(otherNinja => otherNinja.X == ninja.X && otherNinja.Y == ninja.Y).Count() > 1,
                        () => Fight()
                ),
                (       "ThrowShurikans",
                        () => CanThrowShurikan(ninja, brd),
                        () => ThrowShurikan(ninja,brd)
                ),
                (       "Empty",
                        () => brd[ninja.X, ninja.Y] == ' ',
                        () => DefaultMoveAction(ninja,brd)
                ),
                (       "South",
                        () => brd[ninja.X, ninja.Y] == 'S',
                        () => ninja.Direction = Direction.South
                ),
                (       "East",
                        () => brd[ninja.X, ninja.Y] == 'E',
                        () => ninja.Direction = Direction.East
                ),
                (       "North",
                        () => brd[ninja.X, ninja.Y] == 'N',
                        () => ninja.Direction = Direction.North
                ),
                (       "West",
                        () => brd[ninja.X, ninja.Y] == 'W',
                        () => ninja.Direction = Direction.West
                ),
                (       "CollectShurikan",
                        () => brd[ninja.X, ninja.Y] == '*',
                        () =>
                        {
                            ninja.Shurikens++;
                            brd[ninja.X, ninja.Y] = ' ';
                            DefaultMoveAction(ninja,brd);
                        }
                ),
                (       "BreakerMode",
                        () => brd[ninja.X, ninja.Y] == 'B',
                        () =>
                        {
                            ninja.BreakerMode = true;
                            DefaultMoveAction(ninja,brd);
                        }
                ),
                (       "BreakableObstacle",
                        () => brd[ninja.X, ninja.Y] == 'X',
                        () =>
                        {
                            if(ninja.BreakerMode)
                                brd[ninja.X, ninja.Y] = ' ';
                            DefaultMoveAction(ninja,brd);
                        }
                ),
    
                //Portal Items
                (       "Portalling",
                        () =>
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
                )


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
            int actionNumber = 0;
            shouldContinue = false;
            foreach(var cellActionOption in CellActionList)
            {
                ignoreLogDirection = false;
                if(cellActionOption.condition())
                {
                    cellActionOption.action();
                    LogLine(cellActionOption);
                }

                if(!shouldContinue)
                    break;

                actionNumber++;
            }
        }

        private void LogLine((string name, Func<bool> condition, Action action) cellActionOption)
        {
            string logLineString = ignoreLogDirection ? $"{cellActionOption.name}" : $"{ninja.Direction}({cellActionOption.name})";
            logLineString = $"{logLineString}{Environment.NewLine}";
            File.AppendAllText("Log.txt", logLineString);
        }


        private void DefaultMoveAction(Ninja ninja, Board brd)
        {
            List<char> blockers = new List<char>(2) { '#' };

            if(!ninja.BreakerMode)
                blockers.Add('X');

            //Check what nearest cell is open to move to it
            foreach(var orderChecker in ninja.CurrentMoveOrder)
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
            List<Func<int, (int X, int Y)>> calculateRangeLocation = Board.RetrieveRangeLocationCalculation(ninja);

            for(int i = 0; i < calculateRangeLocation.Count; i++)
            {
                int range = 1;
                (int X, int Y) curLoc = calculateRangeLocation[i](range);
                while(brd[curLoc.X, curLoc.Y] != '#')
                {
                    if(brd[curLoc.X, curLoc.Y] == 'X' || brd[curLoc.X, curLoc.Y] == '$')
                        return true;

                    range++;
                    curLoc = calculateRangeLocation[i](range);
                }
            }

            return false;
        }





        private void ThrowShurikan(Ninja ninja, Board brd)
        {
            ignoreLogDirection = true;

            List<Func<int, (int X, int Y)>>  calculateRangeLocation = Board.RetrieveRangeLocationCalculation(ninja);

            for(int i = 0; i < calculateRangeLocation.Count; i++)
            {
                int range = 1;
                (int X, int Y) curLoc = calculateRangeLocation[i](range);
                while(brd[curLoc.X, curLoc.Y] != '#')
                {
                    if(brd[curLoc.X, curLoc.Y] == '$')
                    {
                        WasWinningStep = true;
                        return;
                    }

                    range++;
                    curLoc = calculateRangeLocation[i](range);
                }
            }

            for(int i = 0; i < calculateRangeLocation.Count; i++)
            {
                int range = 1;
                (int X, int Y) curLoc = calculateRangeLocation[i](range);
                while(brd[curLoc.X, curLoc.Y] != '#')
                {
                    if(brd[curLoc.X, curLoc.Y] == 'X')
                    {
                        brd[curLoc.X, curLoc.Y] = ' ';
                        return;
                    }

                    range++;
                    curLoc = calculateRangeLocation[i](range);
                }
            }
        }

        private void Fight()
        {

        }
    }
}

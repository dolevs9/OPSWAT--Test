using Models;
using Models.CellDynamicItems;

namespace Engine
{
    internal class ActionSelector
    {
        internal bool WasWinningStep { get; private set; } = false;

        Ninja ninja;
        Board brd;
        IEnumerable<Ninja> allNinjas;
        IEnumerable<Bomb> bombs;
        bool shouldContinue;
        bool ignoreLogDirection = false;
        bool stepAction = true;

        List<(string name, Func<bool> condition, Action action)> CellActionList;

        public ActionSelector(Ninja actingNinja, Board board, IEnumerable<Bomb> bmb, IEnumerable<Ninja> ninjas)
        {
            ninja = actingNinja;
            brd = board;
            bombs = bmb;
            allNinjas = ninjas;
            CellActionList = LoadActionList();

        }

        private List<(string name, Func<bool> condition, Action action)> LoadActionList()
        {


            return new List<(string name, Func<bool> condition, Action action)>()
            {(
                      "ActivateBombs",
                      () =>     char.IsDigit(brd[ninja.X, ninja.Y-1]) ||
                                char.IsDigit(brd[ninja.X+1, ninja.Y]) ||
                                char.IsDigit(brd[ninja.X, ninja.Y+1]) ||
                                char.IsDigit(brd[ninja.X-1, ninja.Y]),
                      () => {
                                var bombsNearNinja = bombs.Where(
                                                     bomb =>  bomb.X == ninja.X+1 && bomb.Y == ninja.Y ||
                                                     bomb.X == ninja.X-1 && bomb.Y == ninja.Y ||
                                                     bomb.X == ninja.X && bomb.Y == ninja.Y-1 ||
                                                     bomb.X == ninja.X && bomb.Y == ninja.Y+1);
                                
                                foreach (Bomb bomb in bombsNearNinja)
                                  bomb.IsActive = true;

                                shouldContinue = true;
                                ignoreLogDirection = true;
                                stepAction = false;
                        }
                ),

                (      "Start",
                        () => brd[ninja.X, ninja.Y] == '@',
                        () => DefaultMoveAction(ninja,brd)
                ),
                (       "ThrowShurikens",
                        () => CanThrowShuriken(ninja, brd),
                        () => ThrowShuriken(ninja,brd)
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
                (       "CollectShuriken",
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
                            ninja.BreakerMode = !ninja.BreakerMode;
                            DefaultMoveAction(ninja,brd);
                        }
                ),
                (       "MirrorMode",
                        () => brd[ninja.X, ninja.Y] == 'M',
                        () =>
                        {
                            ninja.IsMirrored = !ninja.IsMirrored;
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
                (       "HolyGrail",
                        () => brd[ninja.X, ninja.Y] == '$',
                        () =>
                        {
                            if(ninja.BreakerMode)
                            {
                                WasWinningStep = true;
                                ignoreLogDirection = true;
                                stepAction = false;
                            }
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

        public bool SelectAction()
        {
            int actionNumber = 0;
            shouldContinue = false;
            bool actionDone = false;
            foreach(var cellActionOption in CellActionList)
            {
                stepAction = true;
                ignoreLogDirection = false;
                if(cellActionOption.condition())
                {
                    cellActionOption.action();

                    //Move the ninja if required
                    if(stepAction)
                    {
                        var direction = ninja.CurrentMoveOrder[0];
                        ninja.X = direction.X;
                        ninja.Y = direction.Y;
                    }

                    LogLine(cellActionOption);
                    if(!shouldContinue)
                        actionDone = true;
                }

                if(actionDone)
                    return true;

                shouldContinue = false;

                actionNumber++;
            }

            if(!actionDone)
            {
                File.AppendAllText($"Log.{brd.Name}.txt", $"Ninja:{ninja.Name}, got Stuck !{Environment.NewLine}");
                return false;
            }

            return true;
        }

        private void LogLine((string name, Func<bool> condition, Action action) cellActionOption)
        {
            string logLineString = ignoreLogDirection ? $"{cellActionOption.name}" : $"{ninja.Direction}({cellActionOption.name})";
            logLineString = $"Ninja:{ninja.Name}, {logLineString}{Environment.NewLine}";
            File.AppendAllText($"Log.{brd.Name}.txt", logLineString);
        }

        private void DefaultMoveAction(Ninja ninja, Board brd)
        {
            List<char> blockers = new List<char>(2) { '#' };

            if(!ninja.BreakerMode)
            {
                blockers.Add('X');
                blockers.Add('$');
            }

            //Check what nearest cell is open to move to it
            foreach(var orderChecker in ninja.CurrentMoveOrder)
            {
                bool canGo = true;
                char nextCellValue = brd[orderChecker.X, orderChecker.Y];

                foreach(var blocker in blockers)
                {
                    if(nextCellValue == blocker)
                        canGo = false;
                }

                if(canGo)
                {
                    ninja.Direction = orderChecker.dircection;
                    break;
                }
            }
        }

        private bool CanThrowShuriken(Ninja ninja, Board brd)
        {
            if(ninja.Shurikens == 0)
                return false;

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

        private void ThrowShuriken(Ninja ninja, Board brd)
        {
            stepAction = false;
            ignoreLogDirection = true;

            List<Func<int, (int X, int Y)>> calculateRangeLocation = Board.RetrieveRangeLocationCalculation(ninja);

            for(int i = 0; i < calculateRangeLocation.Count; i++)
            {
                int range = 1;
                (int X, int Y) curLoc = calculateRangeLocation[i](range);
                while(brd[curLoc.X, curLoc.Y] != '#' && brd[curLoc.X, curLoc.Y] != 'X')
                {
                    if(ninja.Shurikens > 0 && brd[curLoc.X, curLoc.Y] == '$')
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
                    if(ninja.Shurikens > 0 && brd[curLoc.X, curLoc.Y] == 'X')
                    {
                        ninja.Shurikens--;
                        brd[curLoc.X, curLoc.Y] = '*';
                        return;
                    }

                    range++;
                    curLoc = calculateRangeLocation[i](range);
                }
            }
        }
    }
}

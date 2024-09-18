using Models;
using Models.CellDynamicItems;

namespace Engine
{
    public class MainEngine
    {
        IEnumerable<Ninja> runningNinjas;
        IEnumerable<Bomb> countdownBombs;
        Board brd;
        bool isDuringFight = false;

        private void ExtractDynamicItems()
        {
            ItemCreator creator = new ItemCreator();

            List<CellDynamicItem> cellItems = new List<CellDynamicItem>(10);
            brd.ScanItems((x, y, cell) =>
            {
                CellDynamicItem cellItem = creator.CreateItem(x, y, cell);
                if(cellItem != null)
                    cellItems.Add(cellItem);
            });

            runningNinjas = cellItems.Where(cellItem => cellItem is Ninja).Select(cellItem => cellItem as Ninja);
            countdownBombs = cellItems.Where(cellItem => cellItem is Bomb).Select(cellItem => cellItem as Bomb);
        }

        private void ExplodeBomb(Bomb bomb, Board brd)
        {
            bomb.IsAlive = false;

            File.AppendAllText($"Log.{brd.Name}.txt", $"Bomb at x:{bomb.X} y:{bomb.Y} exploded");

            List<Func<int, (int X, int Y)>> calculateRangeLocation = Board.RetrieveRangeLocationCalculation(bomb);

            for(int i = 0; i < calculateRangeLocation.Count; i++)
            {
                int range = 0;
                (int X, int Y) curLoc = calculateRangeLocation[i](range);
                while(brd[curLoc.X, curLoc.Y] != '#' && range <= 2)
                {
                    if(brd[curLoc.X, curLoc.Y] == '0' || brd[curLoc.X, curLoc.Y] == 'X' || brd[curLoc.X, curLoc.Y] == '*')
                        brd[curLoc.X, curLoc.Y] = ' ';

                    foreach(Ninja ninja in runningNinjas.Where(ninja => ninja.X == curLoc.X && ninja.Y == curLoc.Y))
                    {
                        ninja.IsAlive = false;
                        File.AppendAllText($"Log.{brd.Name}.txt", $"Ninja {ninja.Name} died by bomb !");
                    }
                    
                        

                    range++;
                    curLoc = calculateRangeLocation[i](range);
                }
            }
        }

        private void RunCountdownBombs()
        {
            foreach(Bomb bomb in countdownBombs)
            {
                if(bomb.IsActive && bomb.IsAlive)
                    if(bomb.TurnsToBomb == 0)
                    {
                        ExplodeBomb(bomb, brd);
                    }
                    else
                    {
                        bomb.TurnsToBomb--;
                        brd[bomb.X, bomb.Y] = Convert.ToChar(bomb.TurnsToBomb);
                    }
            }
        }

        private void Fight()
        {
            isDuringFight = false;

            var multipleNinjasPerCell = (from n in runningNinjas
                                         where n.IsAlive == true
                                         group n by new { n.X, n.Y } into grp
                                         select grp).Where(group => group.Count() > 1);

            if(multipleNinjasPerCell == null)
                return;

            isDuringFight = true;

            foreach(var fightingNinjaGroup in multipleNinjasPerCell)
            {
                string fightningNinjasString = string.Empty;
                fightingNinjaGroup.Select(ninja => fightningNinjasString += $"{ninja.Name}, ");
                File.AppendAllText($"Log.{brd.Name}.txt", $"Battle between {fightningNinjasString} at x:{fightingNinjaGroup.First().X} y:{fightingNinjaGroup.First().Y}");

                int ninjaCount = fightingNinjaGroup.Count();
                bool groupHasBreakerMode = fightingNinjaGroup.Where(ninja => ninja.BreakerMode == true).Count() > 0;

                //If breaker mode ninja exist in fight
                if(groupHasBreakerMode)
                {
                    //Kill all non breaker mode ninjas
                    fightingNinjaGroup.Where(ninja => ninja.BreakerMode == false).Select(ninja => ninja.IsAlive = false);
                }

                //Kill the shurikenlessninjas
                fightingNinjaGroup.Where(ninja => ninja.Shurikens == 0)
                    .Select(ninja => ninja.IsAlive = false);

                //Remove 1 shuriken for all ninjas
                fightingNinjaGroup.Where(ninja => ninja.Shurikens > 0)
                    .Select(ninja => ninja.Shurikens--);

                foreach (Ninja ninja in fightingNinjaGroup.Where(ninja => ninja.IsAlive == false))
                {
                    File.AppendAllText($"Log.{brd.Name}.txt", $"Ninja {ninja.Name} died in battle");
                }
            }

            multipleNinjasPerCell = (from n in runningNinjas
                                     where n.IsAlive == true
                                     group n by new { n.X, n.Y } into grp
                                     select grp).Where(group => group.Count() > 1);

            //Count how many groups that has more then 1 ninja per cell are there, and we are not in battle of there are none.
            if(multipleNinjasPerCell.Count() == 0)
                isDuringFight = false;
        }

        public Ninja Run(Board brd)
        {
            this.brd = brd;
            bool finished = false;
            ExtractDynamicItems();

            while(!finished)
            {
                RunCountdownBombs();

                if(!isDuringFight)
                {
                    foreach(Ninja ninja in runningNinjas)
                    {
                        if(ninja.IsAlive)
                        {
                            ActionSelector action = new ActionSelector(ninja, brd, countdownBombs, runningNinjas);
                            bool actionWasDone = action.SelectAction();
                            if(action.WasWinningStep)
                            {
                                File.AppendAllText($"Log.{brd.Name}.txt", $"Ninja {ninja.Name} Won the Game !{Environment.NewLine}");
                                return ninja;
                            }

                            if(!actionWasDone)
                                finished = true;
                        }
                    }
                }

                Fight();
            }

            return null;
        }
    }
}

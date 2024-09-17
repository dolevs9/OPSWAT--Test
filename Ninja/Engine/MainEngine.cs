using Models;
using Models.CellDynamicItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    internal class MainEngine
    {
        IEnumerable<Ninja> runningNinjas;
        IEnumerable<Bomb> countdownBombs;
        Board brd;

        

        private void ExtractDynamicItems()
        {
            ItemCreator creator = new ItemCreator();

            List<CellDynamicItem> cellItems = new List<CellDynamicItem>(10);
            brd.ScanItems((x, y, cell) =>
            {
                CellDynamicItem cellItem = creator.CreateItem(cell);
                if(cellItem != null)
                    cellItems.Add(cellItem);
            });

            runningNinjas = cellItems.Where(cellItem => cellItem is Ninja).Select(cellItem => cellItem as Ninja);
            countdownBombs = cellItems.Where(cellItem => cellItem is Bomb).Select(cellItem => cellItem as Bomb);
        }



        private void ExplodeBomb(Bomb bomb, Board brd)
        {
            bomb.IsAlive = false;

            List<Func<int, (int X, int Y)>> calculateRangeLocation = Board.RetrieveRangeLocationCalculation(bomb);

            for(int i = 0; i < calculateRangeLocation.Count; i++)
            {
                int range = 0;
                (int X, int Y) curLoc = calculateRangeLocation[i](range);
                while(brd[curLoc.X, curLoc.Y] != '#' && range <= 2)
                {
                    if(brd[curLoc.X, curLoc.Y] == '0' || brd[curLoc.X, curLoc.Y] == 'X' || brd[curLoc.X, curLoc.Y] == '*')
                        brd[curLoc.X, curLoc.Y] = ' ';

                    //TODO: Validation
                    runningNinjas.Where(ninja => ninja.X == curLoc.X && ninja.Y == curLoc.Y)
                        .Select(ninja => ninja.IsAlive = false);

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
                        ExplodeBomb(bomb,brd);
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

        }


        public Ninja Run(Board brd)
        {
            this.brd = brd;
            bool finished = false;
            ExtractDynamicItems();

            while(!finished)
            {
                RunCountdownBombs();
                foreach(Ninja ninja in runningNinjas)
                {
                    if(ninja.IsAlive)
                    {
                        ActionSelector action = new ActionSelector(ninja, brd, countdownBombs, runningNinjas);
                        action.SelectAction();
                        if(action.WasWinningStep)
                        {
                            File.AppendAllText("Log.txt", $"Ninja {ninja.Name} Won the Game !{Environment.NewLine}");
                            return ninja;
                        }
                    }
                }
                Fight();
            }

            return null;
        }
    }
}

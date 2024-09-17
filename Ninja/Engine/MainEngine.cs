using Models;
using Models.CellIDynamictems;
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


        private void RunCountdownBombs()
        {
            foreach(Bomb bomb in countdownBombs)
            {
                if(bomb.IsActive)
                    if(bomb.TurnsToBomb <= 1)
                    {
                        //TODO: CLEAR from y-2 to y+2 and from x-2 to x+2 if not # found 

                        brd[bomb.X, bomb.Y] = Convert.ToChar(bomb.TurnsToBomb);

                        //TODO: CLEAR ninjas in that area 
                    }
                    else
                    {
                        bomb.TurnsToBomb--;
                        brd[bomb.X, bomb.Y] = Convert.ToChar(bomb.TurnsToBomb);
                    }
            }
        }

        private void ThrowShurikans(Ninja n)
        {

        }


        public void Run(Board brd)
        {
            this.brd = brd;
            bool finished = false;
            ExtractDynamicItems();

            while(!finished)
            {
                foreach(var ninja in runningNinjas)
                {
                    ThrowShurikans(ninja);
                }
            }
        }
    }
}

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

        private void ScanItems(Action<int, int, char> actionToDoPerCell)
        {
            for(int x = 0; x < brd.GetLength(0); x++)
            {
                for(int y = 0; y < brd.GetLength(1); y++)
                {
                    char cellItem = brd[x, y];
                    actionToDoPerCell(x, y, cellItem);
                }
            }
        }

        private void CreateItems()
        {
            ItemCreator creator = new ItemCreator();

            List<CellDynamicItem> cellItems = new List<CellDynamicItem>(10);
            ScanItems((x, y, cell) =>
            {
                CellDynamicItem cellItem = creator.CreateItem(cell);
                if(cellItem != null)
                    cellItems.Add(cellItem);
            });

            runningNinjas = cellItems.Where(cellItem => cellItem is Ninja).Select(cellItem => cellItem as Ninja);
            countdownBombs = cellItems.Where(cellItem => cellItem is Bomb).Select(cellItem => cellItem as Bomb);
        }





        public void Run(Board brd)
        {
            this.brd = brd;
            bool finished = false;
            while(!finished)
            {
                foreach(var ninja in runningNinjas)
                {
                }
            }
        }
    }
}

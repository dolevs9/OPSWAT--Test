using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.CellDynamicItems
{
    public class Bomb : CellDynamicItem
    {
        public int TurnsToBomb { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlive { get; set; }

        public Bomb(int turnsToBomb)
        {
            TurnsToBomb = turnsToBomb;
            IsActive = false;
            IsAlive = true;
        }
    }
}

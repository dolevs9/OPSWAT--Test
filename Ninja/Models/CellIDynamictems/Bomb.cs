using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.CellIDynamictems
{
    public class Bomb : CellDynamicItem
    {
        public int TurnsToBomb { get; set; }
        public bool IsActive { get; set; }

        public Bomb(int turnsToBomb)
        {
            TurnsToBomb = turnsToBomb;
            IsActive = false;
        }
    }
}

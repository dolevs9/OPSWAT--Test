using Models;
using Models.CellIDynamictems;

namespace Engine
{
    public class ActionSelector
    {
        public delegate Ninja CellAction(Ninja ninja);

        Dictionary<string, CellAction> CellActionList = new Dictionary<string, CellAction>()
        {
            { "@" , (Ninja n) =>
                {
                    n.Direction = Direction.South;
                    return n;
                }
            }
        };




        
    }
}

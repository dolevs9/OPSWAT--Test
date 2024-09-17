using Models.CellDynamicItems;

namespace Engine
{
    public class ItemCreator
    {
        private Dictionary<Func<char, bool>, Func<char, CellDynamicItem>> CellMatcherList = new Dictionary<Func<char, bool>, Func<char, CellDynamicItem>>()
        {
            { (char cell) => cell == '@'        , (char cell) => new Ninja() },
            { (char cell) => char.IsDigit(cell) , (char cell) => new Bomb(Convert.ToInt32(cell)) }
        };

        internal CellDynamicItem CreateItem(char cellItemChar)
        {
            CellDynamicItem item = null;

            foreach(var CellMatcher in CellMatcherList)
            {
                if(CellMatcher.Key.Invoke(cellItemChar))
                    item = CellMatcher.Value.Invoke(cellItemChar);
            }

            return item;
        }
    }
}

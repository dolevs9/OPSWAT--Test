using Models.CellDynamicItems;

namespace Engine
{
    internal class ItemCreator
    {
        private Dictionary<Func<char, bool>, Func<int, int, char, CellDynamicItem>> CellMatcherList = new Dictionary<Func<char, bool>, Func<int, int, char, CellDynamicItem>>()
        {
            { (char cell) => cell == '@'        , (int x, int y, char cell) => new Ninja() {X = x, Y = y } },
            { (char cell) => char.IsDigit(cell) , (int x, int y, char cell) => new Bomb(Convert.ToInt32(cell.ToString())) {X = x, Y = y } }
        };

        internal CellDynamicItem CreateItem(int x, int y, char cellItemChar)
        {
            CellDynamicItem item = null;

            foreach(var CellMatcher in CellMatcherList)
            {
                if(CellMatcher.Key.Invoke(cellItemChar))
                    item = CellMatcher.Value.Invoke(x, y, cellItemChar);
            }

            return item;
        }
    }
}

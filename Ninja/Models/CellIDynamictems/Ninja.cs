namespace Models.CellIDynamictems
{
    public class Ninja : CellDynamicItem
    {
        public int Shurikens { get; set; } = 3;
        public Direction Direction { get; set; } = Direction.South;
    }
}

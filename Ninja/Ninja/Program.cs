using Engine;
using Models;

class TestClass
{
    static void Main(string[] args)
    {
        Board brd = Board.BoardLoader(@"C:\projects\OPSWAT-Test\Ninja\Input\maps\03_in");
        MainEngine engine = new MainEngine();
        engine.Run(brd);
    }
}
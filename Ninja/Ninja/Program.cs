using Engine;
using Models;

class TestClass
{
    static void Main(string[] args)
    {
        string[] logFiles = File.ReadAllLines(@"Input\input-all.list");

        foreach(string logFileName in logFiles)
        {
            Board brd = Board.BoardLoader($@"Input\{logFileName}");
            File.Delete($"Log.{brd.Name}.txt");
            MainEngine engine = new MainEngine();
            engine.Run(brd);
        }
    }
}
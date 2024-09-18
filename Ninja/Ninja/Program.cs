using Engine;
using Models;

class TestClass
{
    static void Main(string[] args)
    {
        Console.WriteLine("Press Y to enable visual? ");
        string answer = Console.ReadLine();
        bool enableVisual = (answer.ToLower() == "y");

        string[] logFiles = File.ReadAllLines(@"Input\input-all.list");

        foreach(string logFileName in logFiles)
        {
            Board brd = Board.BoardLoader($@"Input\{logFileName}");
            File.Delete($"Log.{brd.Name}.txt");
            MainEngine engine = new MainEngine();

            if(enableVisual)
                engine.GameStepWasDone += Engine_GameStepWasDone;

            engine.Run(brd);
        }
    }

    private static void Engine_GameStepWasDone(Board brd)
    {
        Console.Clear();
        Console.WriteLine(brd.Name);
        brd.PrintBoard();
        Console.WriteLine("Press enter for next step");
        Console.ReadLine();
    }
}
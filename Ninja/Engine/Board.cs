using Models.CellDynamicItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Board
    {
        private char[,] board { get; set; }

        public char this[int x, int y]
        {
            get {
                return board[x, y];
            }
            set {
                board[x, y] = value;
            }
        }

        public int GetLength(int dimension)
        {
            return board.GetLength(dimension);
        }

        public Board(char[,] content)
        {
            board = content;
            
        }


        internal void ScanItems(Action<int, int, char> actionToDoPerCell)
        {
            for(int x = 0; x < board.GetLength(0); x++)
            {
                for(int y = 0; y < board.GetLength(1); y++)
                {
                    char cellItem = board[x, y];
                    actionToDoPerCell(x, y, cellItem);
                }
            }
        }

        public static Board BoardLoader(string filePath)
        {
            //String is immuetable, so i prefer using array of chars, and not list of lines.
            string fileContent = File.ReadAllText(filePath);
            string[] lines = fileContent.Split("\n");

            //Remove last line if it contains only new line or space
            if(string.IsNullOrWhiteSpace(lines.Last()))
                lines = lines.Take(lines.Length-1).ToArray();

            //We know the array size, it takes less storange then using string line list.
            char[,] board = new char[lines[0].Length, lines.Length];

            for(int y = 0; y < lines.Length; y++)
                for(int x = 0; x < lines[y].Length; x++)
                    board[x, y] = lines[y][x];

            Board b = new Board(board);
            return b;
        }

        static internal List<Func<int, (int X, int Y)>> RetrieveRangeLocationCalculation(CellDynamicItem cellItem)
        {
            return new List<Func<int, (int X, int Y)>>()
            {
                (int range)=> (cellItem.X, cellItem.Y + range),
                (int range)=> (cellItem.X+range, cellItem.Y),
                (int range)=> (cellItem.X, cellItem.Y - range),
                (int range)=> (cellItem.X-range, cellItem.Y)
            };
        }
    }
}

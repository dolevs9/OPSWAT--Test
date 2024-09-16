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
            private set {
                board[x, y] = value;
            }
        }

        public Board(char[,] content)
        {
            board = content;
        }


        public static Board BoardLoader(string filePath)
        {
            //String is immuetable, so i prefer using array of chars, and not list of lines.
            string fileContent = File.ReadAllText(filePath);
            string[] lines = fileContent.Split("\r\n");

            //We know the array size, it takes less storange then using string line list.
            char[,] board = new char[lines[0].Length, lines.Length];

            for(int y = 0; y < lines.Length; y++)
                for(int x = 0; x < lines[y].Length; x++)
                    board[x, y] = lines[y][x];

            Board b = new Board(board);
            return b;
        }
    }
}

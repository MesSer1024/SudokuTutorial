using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SudokuTutorial.Techniques;

namespace SudokuTutorial
{
    public class SudokuLib
    {

        public static SudokuBoard generateBoard(string input)
        {
            if (input.Length != 81)
                throw new Exception("Invalid board size!");
            var board = new SudokuBoard(input);
            var nodes = board.getNodes();
            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];
                if (c != '.')
                {
                    var node = nodes[i];
                    var dbl = char.GetNumericValue(c);
                    node.Value = (int)dbl;
                }
            }
            return board;
        }
    }
}

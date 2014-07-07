using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SudokuTutorial;
using SudokuTutorial.Techniques;

namespace SudokuPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            //http://www.thonky.com/sudoku/
            //var board = SudokuLib.generateBoard("..5.398...82.1...7.4.75.62..3.49.................23.8..91.82.6.5...6.93...894.1..");
            //var board = SudokuLib.generateBoard("1...8...3.7.5...81....1365...73..........8..9.4.6.25....9.....8....6...4..3...9..");
            var board = SudokuLib.generateBoard("31....2..9..7....3.....9.6....8........9.28...7.6.5.2.....2.5...5....1.4.64......");
            //var board = SudokuLib.generateBoard(".....1.68..4.26.3.286.7....84..1...66..7..........4...51......7....5.9..769...8.2");
            //tutorial.getNodes()[4].BoardValue = 4;
            //tutorial.getNodes()[11].BoardValue = 4;
            //tutorial.getNodes()[28].BoardValue = 4;
            printValues(board);

            bool iterate = true;
            var unknowns = new List<SudokuNode>();
            while (iterate)
            {
                unknowns.Clear();
                Techniques.fillUnknowns(board, unknowns);
                Techniques.removeBasicCandidates(board, unknowns);
                var items = SudokuUtils.getItemsWithOneCandidate(unknowns);
                if (items.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Possible to write {0} values", items.Count);
                    items.ForEach((a) =>
                    {
                        Console.WriteLine(a.ToString());
                        a.Value = a.getCandidates()[0];
                    });
                }
                else
                {
                    iterate = false;
                }
            }

            printValues(board);
            Console.ReadLine();
            int foo = 0;
        }

        static void printBoardLayout(SudokuBoard board)
        {
            var nodes = board.getNodes();

            //rows
            Console.WriteLine();
            Console.WriteLine("----Rows----");
            var sb = new StringBuilder();
            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    var node = nodes[i * 9 + j];
                    sb.Append(node.Row);
                }
                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());

            //cols
            Console.WriteLine();
            Console.WriteLine("----Columns----");
            sb.Clear();
            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    var node = nodes[i * 9 + j];
                    sb.Append(node.Col);
                }
                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());

            //blocks
            Console.WriteLine();
            Console.WriteLine("----Blocks----");
            sb.Clear();
            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    var node = nodes[i * 9 + j];
                    sb.Append(node.Block);
                }
                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());            
        }

        static void printValues(SudokuBoard board)
        {
            var sb = new StringBuilder();
            sb.AppendLine("----Values----");
            foreach (var node in board.getNodes())
            {
                if (node.Col == 0)
                {
                    if (node.Row == 3 || node.Row == 6)
                        sb.AppendLine();

                    sb.AppendLine();
                }
                
                if (node.Col % 3 == 0)
                {
                    sb.Append(" ");
                }

                sb.Append(node.Value == 0 ? "-" : node.Value.ToString());
            }
            Console.WriteLine(sb.ToString());
        }
    }
}

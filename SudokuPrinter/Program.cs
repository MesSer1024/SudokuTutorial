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
            //basic technique
            //var board = SudokuLib.generateBoard("..5.398...82.1...7.4.75.62..3.49.................23.8..91.82.6.5...6.93...894.1..");
            //var board = SudokuLib.generateBoard("1...8...3.7.5...81....1365...73..........8..9.4.6.25....9.....8....6...4..3...9..");

            //advanced (not naked)
            //var board = SudokuLib.generateBoard("31....2..9..7....3.....9.6....8........9.28...7.6.5.2.....2.5...5....1.4.64......");

            //advanced
            //var board = SudokuLib.generateBoard(".....1.68..4.26.3.286.7....84..1...66..7..........4...51......7....5.9..769...8.2");
            //var board = SudokuLib.generateBoard("..219.4....5....3.......6.5..19...87.2...6...5.8........6......41.6.7..97..8...2.");
            //var board = SudokuLib.generateBoard("..93.7.....142.87..7.......3...6......791..2......2..5..2....5......16.4..8.....9");
            //var board = SudokuLib.generateBoard("3.8.2..7.....31....4......8.....8.57..3..61..5.....2.675...9............4..75..9.");
            //var board = SudokuLib.generateBoard("....65...1...9.5...7.28...1..7.14..64.9...32..5..........63.4.....8.....2....1.7.");
            var board = SudokuLib.generateBoard("1.........5....1.2..25.74....5.38.1......467.7....5..9.9.....8.......9...734.....");
            
            printValues(board);

            bool iterate = true;
            var unknowns = new List<SudokuNode>();
            while (iterate)
            {
                int level = 0;
                //reset state
                unknowns.Clear();
                Techniques.fillUnknowns(board, unknowns);

                //remove candidates
                if (++level > 0 && Techniques.removeBasicCandidates(board, unknowns))
                    if (++level > 0 && Techniques.removeNakedCandidates(board, unknowns) && ++level > 0)
                        if (++level > 0 && Techniques.removeHiddenCandidates(board, unknowns) && ++level > 0)
                    {}

                //handle iteration
                var items = SudokuUtils.getItemsWithOneCandidate(unknowns);
                if (items.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Possible to write {0} values using technique {1}", items.Count, level);
                    items.ForEach((a) =>
                    {
                        a.Value = a.getCandidates()[0];
                        Console.WriteLine(a.ToString());
                    });
                    printValues(board);
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

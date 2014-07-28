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
            var techniques = new List<Action>();
            var boards = new List<string>();

            //http://www.thonky.com/sudoku/
            //basic technique
            boards.Add("..5.398...82.1...7.4.75.62..3.49.................23.8..91.82.6.5...6.93...894.1..");
            boards.Add("..5.398...82.1...7.4.75.62..3.49.................23.8..91.82.6.5...6.93...894.1..");
            boards.Add("1...8...3.7.5...81....1365...73..........8..9.4.6.25....9.....8....6...4..3...9..");

            //advanced (not naked)
            boards.Add("31....2..9..7....3.....9.6....8........9.28...7.6.5.2.....2.5...5....1.4.64......");
            boards.Add("1.........5....1.2..25.74....5.38.1......467.7....5..9.9.....8.......9...734.....");

            //advanced
            boards.Add(".....1.68..4.26.3.286.7....84..1...66..7..........4...51......7....5.9..769...8.2");
            boards.Add("3.8.2..7.....31....4......8.....8.57..3..61..5.....2.675...9............4..75..9.");
            boards.Add("7.....9......8..67..37...141..6.2....9.....7....9.8..637...41..95..3......6.....2");
            boards.Add("....65...1...9.5...7.28...1..7.14..64.9...32..5..........63.4.....8.....2....1.7.");
            boards.Add(".....5..1...9...36.9..2......4....9.......583.....7..2.7...1.2..5.4..1...1..5.37.");
            boards.Add(".1.........91..36...68..79.89.5..2....7.......4...9.5....3.....9.2.6..8..3...4...");
            boards.Add("13......95.64.1......29.....8....5.3.7...4.....3.7..2.....4..9......3....276.8.3.");
            boards.Add("4...2..8...1..495....5.1....53....9.........1..6..7...........7.....2....7.34..26");
            boards.Add("75..4..8.........2.93....5..1...8...5...63.7.......4.....72.3....19.62..8.....1..");
            boards.Add("..219.4....5....3.......6.5..19...87.2...6...5.8........6......41.6.7..97..8...2.");
            boards.Add(".3...8.....7.13.8..6.5..314......95.6.....273.72......291..5.3..4.39.5.....7...6.");

            //pointing pairs required
            boards.Add("5..683....8..7......6.2..7....2.5....9.7..35.81.........9......43....1........82.");

            //hard (simple colouring required)
            //boards.Add("..93.7.....142.87..7.......3...6......791..2......2..5..2....5......16.4..8.....9");

            foreach (var b in boards)
            {
                var board = SudokuLib.generateBoard(b);
                printValues(board);

                bool iterate = true;
                var unknowns = new List<SudokuNode>();
                bool forceIteration = false;
                techniques.Clear();

                techniques.Add(() => { if (!Techniques.removeBasicCandidates(board, unknowns)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeHiddenBasic(board, unknowns)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeNakedCandidates(board, unknowns)) forceIteration = true; });
                //techniques.Add(() => { if (!Techniques.removeNakedManyCandidates(board, unknowns)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeHiddenCandidates(board, unknowns)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removePointingPairs(board, unknowns)) forceIteration = true; });
                //techniques.Add(() => { if (!Techniques.removeCubeCandidates(board, unknowns)) forceIteration = true; });
                //techniques.Add(() => { if (!Techniques.removeSingleChainCandidates(board, unknowns)) forceIteration = true; });

                while (iterate)
                {
                    int level = 0;
                    if (forceIteration)
                    {
                        forceIteration = false;
                    }
                    else
                    {
                        //reset state
                        unknowns.Clear();
                        Techniques.fillUnknowns(board, unknowns);
                    }
                    //remove candidates
                    for (int i = 0; i < techniques.Count; ++i)
                    {
                        level++;
                        techniques[i].Invoke();
                        if (forceIteration)
                            break;
                    }

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
                        if (forceIteration)
                        {
                            iterate = true;
                            Console.WriteLine("Forcing another iteration due to candidates being removed");
                        }
                    }
                }

                printValues(board);
                validateSolvedValues(board, true);
            }
            Console.ReadLine();
        }

        static void validateSolvedValues(SudokuBoard board, bool throwNotSolved=false)
        {
            //validate board
            var allKnown = board.getNodes().FindAll(a => a.isKnown());
            if (throwNotSolved && allKnown.Count != 81)
                throw new Exception("Solution is unsolved");
            for (int value = 0; value < 9; ++value)
            {
                var nodes = allKnown.FindAll(a => a.Value == value);
                for (int i = 0; i < nodes.Count; ++i)
                {
                    for (int j = i+1; j < nodes.Count; ++j)
                    {
                        if (nodes[i].Row == nodes[j].Row || nodes[i].Col == nodes[j].Col || nodes[i].Block == nodes[j].Block)
                        {
                            throw new Exception("Invalid board!");
                        }
                    }
                }
            }
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

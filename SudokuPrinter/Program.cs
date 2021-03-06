﻿using System;
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

            ////http://www.thonky.com/sudoku/
            ////basic technique
            //boards.Add("..5.398...82.1...7.4.75.62..3.49.................23.8..91.82.6.5...6.93...894.1..");
            //boards.Add("..5.398...82.1...7.4.75.62..3.49.................23.8..91.82.6.5...6.93...894.1..");
            //boards.Add("1...8...3.7.5...81....1365...73..........8..9.4.6.25....9.....8....6...4..3...9..");

            ////advanced (not naked)
            //boards.Add("31....2..9..7....3.....9.6....8........9.28...7.6.5.2.....2.5...5....1.4.64......");
            //boards.Add("1.........5....1.2..25.74....5.38.1......467.7....5..9.9.....8.......9...734.....");

            ////advanced
            //boards.Add(".....1.68..4.26.3.286.7....84..1...66..7..........4...51......7....5.9..769...8.2");
            //boards.Add("3.8.2..7.....31....4......8.....8.57..3..61..5.....2.675...9............4..75..9.");
            //boards.Add("7.....9......8..67..37...141..6.2....9.....7....9.8..637...41..95..3......6.....2");
            //boards.Add("....65...1...9.5...7.28...1..7.14..64.9...32..5..........63.4.....8.....2....1.7.");
            //boards.Add(".....5..1...9...36.9..2......4....9.......583.....7..2.7...1.2..5.4..1...1..5.37.");
            //boards.Add(".1.........91..36...68..79.89.5..2....7.......4...9.5....3.....9.2.6..8..3...4...");
            //boards.Add("13......95.64.1......29.....8....5.3.7...4.....3.7..2.....4..9......3....276.8.3.");
            //boards.Add("4...2..8...1..495....5.1....53....9.........1..6..7...........7.....2....7.34..26");
            //boards.Add("75..4..8.........2.93....5..1...8...5...63.7.......4.....72.3....19.62..8.....1..");
            //boards.Add("..219.4....5....3.......6.5..19...87.2...6...5.8........6......41.6.7..97..8...2.");
            //boards.Add(".3...8.....7.13.8..6.5..314......95.6.....273.72......291..5.3..4.39.5.....7...6.");

            ////pointing pairs required
            //boards.Add("5..683....8..7......6.2..7....2.5....9.7..35.81.........9......43....1........82.");

            ////Example Boards
            //boards.Add(".....1.3.231.9.....65..31..6789243..1.3.5...6...1367....936.57...6.198433........"); //Hidden triples
            //boards.Add("....3..86....2..4..9..7852.3718562949..1423754..3976182..7.3859.392.54677..9.4132"); //Naked Triple/Quads
            //boards.Add("42.9..386.6.2..7948.9.6.2517....3.259..1.26.32..5....8..4.2.5676827..439......812"); //Unique Rectangle | Also good because solver at sudokuwiki.org uses unneccessary techniques such as simple colouring & X-cycle which removes candidates but is unneccessary for solution
            //boards.Add("1.957.3...7.39..1...3.1.597.8.743...492.5.78373.289.4.317.2.4..26..3..7.95..67231"); //(Hidden Unique Rectangle - type 1)
            //boards.Add("5..291836.3.475.1...9386457.5.143...4..7.9..1...8.2.4.3...2.17..8.937.2.7.2.1...3"); //(Hidden Unique Rectangle - type 2)
            //boards.Add(".2.58..3.35.....84.867...2..48.9.1565..6.8.4.963.5.278.9..6581.6..8...9283.....6."); //(Hidden Unique Rectangle - type 2b)
            //boards.Add("5184726393.6859..44.9316...94562.3..861.34..5732.85..665..9.8.3293.48.6118..63..."); //(Hidden Unique Rectangle - type 2b - awesome example)
            //boards.Add("123...587..5817239987...164.51..847339.75.6187.81..925.76...89153..8174681..7.352"); //Simple coloring (rule 2)
            //boards.Add(".3621.84.8...45631.14863..9287.3.456693584...1456723984.8396...35..28.64.6.45..83"); //Simple colouring (rule 4) & Y-Wing
            //boards.Add("..463.5..6.54.1..337..5964.938.6.154457198362216345987.435.6.19.6.9.34.55.9.14.36"); //Simple colouring (rule 5)
            //boards.Add(".16..78.3.9.8.....87...126..48...3..65...9.82.39...65..6.9...2..8...29369246..51."); //Box-Line Reduction & Y-Wing
            //boards.Add(".2.9437159.4...6..75.....4.5..48....2.....4534..352....42....81..5..426..9.2.85.4"); // Box-Line reduction(s) & Y-Wing

            //hard
            boards.Add("..93.7.....142.87..7.......3...6......791..2......2..5..2....5......16.4..8.....9"); //Simple Colouring
            boards.Add("..7.836...397.68..82641975364.19.387.8.367....73.48.6.39.87..267649..1382.863.97."); //Simple Coloring |same as above but partially solved
            //boards.Add("..5...987.4..5...1..7......2...48....9.1.....6..2.....3..6..2.......9.7.......5.."); //X Cycles
            //boards.Add("48.3............71.2.......7.5....6....2..8.............1.76...3.....4......5...."); //Y-Wing | XY-chain
            boards.Add("....14....3....2...7..........9...3.6.1.............8.2.....1.4....5.6.....7.8..."); //Simple Colouring & Pointing Pairs improvement (Or Hidden Unique Rectangles)
            boards.Add(".623148.7.3....2...7.2..4.3...9...3.6.1....42.......8.2..6..174....5.6.8.167.83.."); //Simple colouring
            boards.Add(".524.........7.1..............8.2...3.....6...9.5.....1.6.3...........897........"); // 3D-medusa, Hidden Unique Rectangle, Alternating Inference Chains
            //boards.Add(".923.........8.1...........1.7.4...........658.........6.5.2...4.....7.....9....."); //Line-Box Reduction, 3D Medusa, Hidden Unique Rectangle, Alternating Inference Chains
            //boards.Add("6..3.2....5.....1..........7.26............543.........8.15........4.2........7.."); //X Cycle, Unique Rectangle, Grouped X-Cycle, Cell forcing chain, Almost Locked Set, Quad Forcing Chain, Unit Forcing Chain, Line Box Reduction
            //boards.Add("..5...987.4..5...1..7......2...48....9.1.....6..2.....3..6..2.......9.7.......5.."); //X Cycles


            //boards.Add("6.2.5.........3.4..........43...8....1....2........7..5..27...........81...6....."); //X-cycles | Quad Forcing Chains | Unit forcing chains | Altern Inferencing chains | Bowman's Bingo -- Force Solve required
            //boards.Add("6.2.5.........4.3..........43...8....1....2........7..5..27...........81...6....."); //force required

            //---special boards---
            //appendTop95Boards(boards);



            foreach (var b in boards)
            {
                var board = SudokuLib.generateBoard(b);
                printValues(board);

                bool iterate = true;
                var unknowns = new List<SudokuNode>();
                bool forceIteration = false;
                techniques.Clear();

                //if using "forceIteration" you CANT set values, you have to reduce candidates to 1 on a node (you can but, as a guideline you can't)
                techniques.Add(() => { if (!Techniques.removeBasicCandidates(board, unknowns)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeHiddenBasic(board, unknowns)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeNakedCandidates(board, unknowns, 2)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeNakedCandidates(board, unknowns, 3)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeHiddenCandidates(board, unknowns, 2)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeHiddenCandidates(board, unknowns, 3)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeNakedCandidates(board, unknowns, 4)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeHiddenCandidates(board, unknowns, 4)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removePointingPairs(board, unknowns)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeBoxLineReductions(board, unknowns)) forceIteration = true; });
                //techniques.Add(() => { if (!Techniques.removeCubeCandidates(board, unknowns)) forceIteration = true; });
                techniques.Add(() => { if (!Techniques.removeSimpleColoringCandidates(board, unknowns)) forceIteration = true; });
                
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
                            Console.WriteLine("Forcing another iteration due to candidates being removed by using technique {0}", level);
                        }
                    }
                }

                printValues(board);
                validateSolvedValues(board, true);
            }
            Console.ReadLine();
        }

        private static void appendTop95Boards(List<string> boards)
        {
            var top95 = @"4.....8.5.3..........7......2.....6.....8.4......1.......6.3.7.5..2.....1.4......
                52...6.........7.13...........4..8..6......5...........418.........3..2...87.....
                6.....8.3.4.7.................5.4.7.3..2.....1.6.......2.....5.....8.6......1....
                ......52..8.4......3...9...5.1...6..2..7........3.....6...1..........7.4.......3.
                3.6.7...........518.........1.4.5...7.....6.....2......2.....4.....8.3.....5.....
                1.....3.8.7.4..............2.3.1...........958.........5.6...7.....8.2...4.......
                ....3..9....2....1.5.9..............1.2.8.4.6.8.5...2..75......4.1..6..3.....4.6.
                45.....3....8.1....9...........5..9.2..7.....8.........1..4..........7.2...6..8..
                .237....68...6.59.9.....7......4.97.3.7.96..2.........5..47.........2....8.......
                ..84...3....3.....9....157479...8........7..514.....2...9.6...2.5....4......9..56
                .98.1....2......6.............3.2.5..84.........6.........4.8.93..5...........1..
                ..247..58..............1.4.....2...9528.9.4....9...1.........3.3....75..685..2...
                4.....8.5.3..........7......2.....6.....5.4......1.......6.3.7.5..2.....1.9......
                .2.3......63.....58.......15....9.3....7........1....8.879..26......6.7...6..7..4
                1.....7.9.4...72..8.........7..1..6.3.......5.6..4..2.........8..53...7.7.2....46
                4.....3.....8.2......7........1...8734.......6........5...6........1.4...82......
                .......71.2.8........4.3...7...6..5....2..3..9........6...7.....8....4......5....
                6..3.2....4.....8..........7.26............543.........8.15........8.2........7..
                .47.8...1............6..7..6....357......5....1..6....28..4.....9.1...4.....2.69.
                ......8.17..2........5.6......7...5..1....3...8.......5......2..4..8....6...3....
                38.6.......9.......2..3.51......5....3..1..6....4......17.5..8.......9.......7.32
                ...5...........5.697.....2...48.2...25.1...3..8..3.........4.7..13.5..9..2...31..
                .2.......3.5.62..9.68...3...5..........64.8.2..47..9....3.....1.....6...17.43....
                .8..4....3......1........2...5...4.69..1..8..2...........3.9....6....5.....2.....
                ..8.9.1...6.5...2......6....3.1.7.5.........9..4...3...5....2...7...3.8.2..7....4
                4.....5.8.3..........7......2.....6.....5.8......1.......6.3.7.5..2.....1.8......
                1.....3.8.6.4..............2.3.1...........958.........5.6...7.....8.2...4.......
                1....6.8..64..........4...7....9.6...7.4..5..5...7.1...5....32.3....8...4........
                249.6...3.3....2..8.......5.....6......2......1..4.82..9.5..7....4.....1.7...3...
                ...8....9.873...4.6..7.......85..97...........43..75.......3....3...145.4....2..1
                ...5.1....9....8...6.......4.1..........7..9........3.8.....1.5...2..4.....36....
                ......8.16..2........7.5......6...2..1....3...8.......2......7..3..8....5...4....
                .476...5.8.3.....2.....9......8.5..6...1.....6.24......78...51...6....4..9...4..7
                .....7.95.....1...86..2.....2..73..85......6...3..49..3.5...41724................
                .4.5.....8...9..3..76.2.....146..........9..7.....36....1..4.5..6......3..71..2..
                .834.........7..5...........4.1.8..........27...3.....2.6.5....5.....8........1..
                ..9.....3.....9...7.....5.6..65..4.....3......28......3..75.6..6...........12.3.8
                .26.39......6....19.....7.......4..9.5....2....85.....3..2..9..4....762.........4
                2.3.8....8..7...........1...6.5.7...4......3....1............82.5....6...1.......
                6..3.2....1.....5..........7.26............843.........8.15........8.2........7..
                1.....9...64..1.7..7..4.......3.....3.89..5....7....2.....6.7.9.....4.1....129.3.
                .........9......84.623...5....6...453...1...6...9...7....1.....4.5..2....3.8....9
                .2....5938..5..46.94..6...8..2.3.....6..8.73.7..2.........4.38..7....6..........5
                9.4..5...25.6..1..31......8.7...9...4..26......147....7.......2...3..8.6.4.....9.
                ...52.....9...3..4......7...1.....4..8..453..6...1...87.2........8....32.4..8..1.
                53..2.9...24.3..5...9..........1.827...7.........981.............64....91.2.5.43.
                1....786...7..8.1.8..2....9........24...1......9..5...6.8..........5.9.......93.4
                ....5...11......7..6.....8......4.....9.1.3.....596.2..8..62..7..7......3.5.7.2..
                .47.2....8....1....3....9.2.....5...6..81..5.....4.....7....3.4...9...1.4..27.8..
                ......94.....9...53....5.7..8.4..1..463...........7.8.8..7.....7......28.5.26....
                .2......6....41.....78....1......7....37.....6..412....1..74..5..8.5..7......39..
                1.....3.8.6.4..............2.3.1...........758.........7.5...6.....8.2...4.......
                2....1.9..1..3.7..9..8...2.......85..6.4.........7...3.2.3...6....5.....1.9...2.5
                ..7..8.....6.2.3...3......9.1..5..6.....1.....7.9....2........4.83..4...26....51.
                ...36....85.......9.4..8........68.........17..9..45...1.5...6.4....9..2.....3...
                34.6.......7.......2..8.57......5....7..1..2....4......36.2..1.......9.......7.82
                ......4.18..2........6.7......8...6..4....3...1.......6......2..5..1....7...3....
                .4..5..67...1...4....2.....1..8..3........2...6...........4..5.3.....8..2........
                .......4...2..4..1.7..5..9...3..7....4..6....6..1..8...2....1..85.9...6.....8...3
                8..7....4.5....6............3.97...8....43..5....2.9....6......2...6...7.71..83.2
                .8...4.5....7..3............1..85...6.....2......4....3.26............417........
                ....7..8...6...5...2...3.61.1...7..2..8..534.2..9.......2......58...6.3.4...1....
                ......8.16..2........7.5......6...2..1....3...8.......2......7..4..8....5...3....
                .2..........6....3.74.8.........3..2.8..4..1.6..5.........1.78.5....9..........4.
                .52..68.......7.2.......6....48..9..2..41......1.....8..61..38.....9...63..6..1.9
                ....1.78.5....9..........4..2..........6....3.74.8.........3..2.8..4..1.6..5.....
                1.......3.6.3..7...7...5..121.7...9...7........8.1..2....8.64....9.2..6....4.....
                4...7.1....19.46.5.....1......7....2..2.3....847..6....14...8.6.2....3..6...9....
                ......8.17..2........5.6......7...5..1....3...8.......5......2..3..8....6...4....
                963......1....8......2.5....4.8......1....7......3..257......3...9.2.4.7......9..
                15.3......7..4.2....4.72.....8.........9..1.8.1..8.79......38...........6....7423
                ..........5724...98....947...9..3...5..9..12...3.1.9...6....25....56.....7......6
                ....75....1..2.....4...3...5.....3.2...8...1.......6.....1..48.2........7........
                6.....7.3.4.8.................5.4.8.7..2.....1.3.......2.....5.....7.9......1....
                ....6...4..6.3....1..4..5.77.....8.5...8.....6.8....9...2.9....4....32....97..1..
                .32.....58..3.....9.428...1...4...39...6...5.....1.....2...67.8.....4....95....6.
                ...5.3.......6.7..5.8....1636..2.......4.1.......3...567....2.8..4.7.......2..5..
                .5.3.7.4.1.........3.......5.8.3.61....8..5.9.6..1........4...6...6927....2...9..
                ..5..8..18......9.......78....4.....64....9......53..2.6.........138..5....9.714.
                ..........72.6.1....51...82.8...13..4.........37.9..1.....238..5.4..9.........79.
                ...658.....4......12............96.7...3..5....2.8...3..19..8..3.6.....4....473..
                .2.3.......6..8.9.83.5........2...8.7.9..5........6..4.......1...1...4.22..7..8.9
                .5..9....1.....6.....3.8.....8.4...9514.......3....2..........4.8...6..77..15..6.
                .....2.......7...17..3...9.8..7......2.89.6...13..6....9..5.824.....891..........
                3...8.......7....51..............36...2..4....7...........6.13..452...........8..".Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in top95)
            {
                boards.Add(s.Trim());
            }
        }

        static void validateSolvedValues(SudokuBoard board, bool throwNotSolved=false)
        {
            //validate board
            var allKnown = board.getNodes().FindAll(a => a.isKnown());
            if (allKnown.Count != 81)
            {
                printCandidates(board);
                if (throwNotSolved)
                    throw new Exception("Solution is unsolved");
                else
                    Console.WriteLine("Board {0} was not solved!", board.ToString());
            }
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

        static void printCandidates(SudokuBoard board)
        {
            var sb = new StringBuilder();
            foreach (var node in board.getNodes())
            {
                if (!node.isKnown())
                {
                    sb.AppendLine(node.ToString());
                }
            }
            Console.Write(sb.ToString());
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------- Thonky -------------
//http://www.thonky.com/sudoku/
//Singles (rows, columns, blocks)
//Naked (pairs, tripels, quads)
//Hidden (pairs, triples, quads)
//Pointing (pairs, triples, intersection-removal)
//Box/Line reduction (intersection-removal)

//X-wing
//simple colouring/single chains
//Y-wing
//Swordfish

//x-cycles
//xy-chain
//3d-medusa
//unique rectangle
//hidden unique rectangle
//xyz-wing

//---------------- Sudoku Solver -------------
//Check for solved cells
//Show Possibles	No

//1: Hidden Singles	
//2: Naked Pairs/Triples	
//3: Hidden Pairs/Triples	 
//4: Naked Quads	 
//5: Pointing Pairs	 
//6: Box/Line Reduction
	 
//Tough Strategies	
//    7: X-Wing	 
//    8: Simple Colouring	 
//    9: Y-Wing	 
//    10: Sword-Fish	 
//    11: XYZ Wing	 
//Diabolical Strategies	
//    12: X-Cycles	 
//    13: XY-Chain	 
//    14: 3D Medusa	 
//    15: Jelly-Fish	 
//    16: Unique Rectangles	 
//    17: Extended Unique Rect.	 
//    18: Hidden Unique Rect's	 
//    19: WXYZ Wing	 
//    20: Aligned Pair Exclusion	 
//Extreme Strategies	
//    21: Grouped X-Cycles	 
//    22: Empty Rectangles	 
//    23: Finned X-Wing	 
//    24: Finned Sword-Fish	 
//    25: Altern. Inference Chains	 
//    26: Sue-de-Coq	 
//    27: Digit Forcing Chains	 
//    28: Nishio Forcing Chains	 
//    29: Cell Forcing Chains	 
//    30: Unit Forcing Chains	 
//    31: Almost Locked Sets	 
//    32: Death Blossom	 
//    33: Pattern Overlay Method	 
//    34: Quad Forcing Chains	 
//"Trial and Error"	
//    35: Bowman's Bingo

namespace SudokuTutorial.Techniques
{
    public class SudokuUtils
    {
        public const int BOARD_SIZE = 81;

        public static List<SudokuNode> getItemsWithOneCandidate(List<SudokuNode> unknowns)
        {
            var items = new List<SudokuNode>();
            foreach (var node in unknowns)
            {
                if (node.getCandidates().Length == 1)
                {
                    items.Add(node);
                }
            }
            return items;
        }

        public static object populateCandidateMap(List<SudokuNode> nodes)
        {
            var map = new Dictionary<int, List<SudokuNode>>();
            for(int i=0; i < 9; ++i) {
                map.Add(i+1, new List<SudokuNode>());
            }
            foreach (var n in nodes)
            {
                n.getCandidates().ToList().ForEach(candidate => map[candidate].Add(n));
            }
            return map;
        }
    }

    public class SudokuNode
    {
        private int _boardValue;
        private List<int> _candidates = new List<int>(9);
        private int _id;
        public int ID { get { return _id; } }

        public int Value
        {
            get { return _boardValue; }
            set
            {
                if (value < 0 || value > 9)
                    throw new Exception("Invalid value for node, expected 0 >= x <= 9 | was=" + _boardValue);
                _boardValue = value;
                if(value != 0)
                    _candidates.Clear();
            }
        }

        public int[] getCandidates()
        {
            return _boardValue == 0 ? _candidates.ToArray() : new int[0];
        }

        public void setCandidates(IEnumerable<int> candidates)
        {
            _candidates = candidates.Distinct().ToList();
        }

        public SudokuNode(int id)
        {
            _id = id;
        }

        public static bool hasValue(SudokuNode node)
        {
            return node._boardValue > 0;
        }

        public bool isKnown()
        {
            return Value != 0 || ( _candidates.Count == 1);
        }

        public int Row { get { return _id / 9; } }
        public int Col { get { return _id % 9; } }
        public int Block { get { return (Row / 3) * 3 + Col / 3; } }
        //public int Unit
        //{
        //    get
        //    {
        //        int r = Row << 24;
        //        int c = Col << 16;
        //        int b = Block << 8;
        //        int val = r + c + b;
        //        return val;
        //    }
        //}

        public void removeCandidate(int value)
        {
            _candidates.Remove(value);
        }

        public override string ToString()
        {
            if (isKnown())
            {
                return string.Format("ID=[{3}], Row: {0}, Col: {1}, Block: {3}, Value={2}", Row + 1, Col + 1, Value, ID, Block + 1);
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var c in _candidates)
                {
                    sb.Append(c);
                }
                return string.Format("ID=[{3}], Row: {0}, Col: {1}, Block: {4}, Candidate(s)={2}", Row + 1, Col + 1, sb.ToString(), ID, Block + 1);
            }
        }
    }

    public class SudokuBoard
    {
        private List<SudokuNode> _nodes;
        public List<SudokuNode> getNodes() { return _nodes; }
        string OriginalInput { get; set; }
        //#TODO: Add a lookup table traversing between values:nodes --> so it is easier to find out where a number could potentially be placed (used in remove naked etc)

        public SudokuBoard(string orgInput)
        {
            OriginalInput = orgInput;
            _nodes = new List<SudokuNode>(SudokuUtils.BOARD_SIZE);
            for (int i = 0; i < SudokuUtils.BOARD_SIZE; ++i)
            {
                _nodes.Add(new SudokuNode(i));
            }
        }

        /// <summary>
        /// A row is 9 nodes aligned horizontally inside board [0,1,2,3,4,5,6,7,8]
        /// </summary>
        /// <param name="row">0..8</param>
        /// <returns></returns>
        public List<SudokuNode> getNodesByRow(int row)
        {
            if (row < 0 || row >= 9)
                throw new Exception("Invalid row-index");

            return _nodes.GetRange(row * 9, 9).ToList();
        }

        /// <summary>
        /// A column is 9 nodes aligned vertically inside board [0,9,18,27,36,45,54,63,72]
        /// </summary>
        /// <param name="col">0..8</param>
        /// <returns></returns>
        public List<SudokuNode> getNodesByColumn(int col)
        {
            if (col < 0 || col >= 9)
                throw new Exception("Invalid col-index");

            var value = new List<SudokuNode>();
            for (int i = 0; i < 9; ++i) {
                value.Add(_nodes[i*9 + col]);
            }
            return value;
        }

        /// <summary>
        /// Block is a 3*3 Matrix of nodes [3 horizontal, 3 vertical]
        /// [ 0, 1, 2]      [ 3, 4, 5]      [ 6, 7, 8]
        /// [ 9,10,11]      [12,13,14]      [15,16,17]
        /// [18,19,20]      [21,22,23]      [24,25,26]
        /// ...
        /// </summary>
        /// <param name="block">0..8</param>
        /// <returns></returns>
        public List<SudokuNode> getNodesByBlock(int block)
        {
            if (block < 0 || block >= 9)
                throw new Exception("Invalid block-index");

            var startIndex = (block / 3) * 27 + (block % 3) * 3;
            var value = new List<SudokuNode>();
            value.AddRange(_nodes.GetRange(startIndex + 9*0, 3));
            value.AddRange(_nodes.GetRange(startIndex + 9*1, 3));
            value.AddRange(_nodes.GetRange(startIndex + 9*2, 3));
            return value;
        }

        public List<SudokuNode> getNeighbours(int id)
        {
            var node = _nodes[id];

            var blockNeighbours = getNodesByBlock(node.Block);
            var colNeighbours = getNodesByColumn(node.Col);
            var rowNeighbours = getNodesByRow(node.Row);

            var neighbours = blockNeighbours.Concat(colNeighbours).Concat(rowNeighbours).Distinct().ToList();
            neighbours.RemoveAll(a => a.ID == id);
            return neighbours;
        }

        public override string ToString()
        {
            return this.OriginalInput;
        }
    }

    public class SudokuResponse 
    {

    }

    public static class Techniques
    {
        public static void fillUnknowns(SudokuBoard board, List<SudokuNode> unknowns)
        {
            var data = board.getNodes();
            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 9; ++j)
                {
                    var node = data[i*9 + j];
                    if (!node.isKnown())
                    {
                        unknowns.Add(node);
                        if(node.getCandidates().Length == 0)
                            node.setCandidates(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                    }
                }
            }
        }

        /// <summary>
        /// Remove candidates by checking the values of each neighbour, every "known" value found on a neighbour is removed as a candidate from the SudokuNode
        /// </summary>
        /// <param name="board"></param>
        /// <param name="unknowns"></param>
        public static bool removeBasicCandidates(SudokuBoard board, List<SudokuNode> unknowns)
        {
            var handledThisTurn = new Dictionary<int, int>();
            bool needAnotherTechnique = true;
            foreach (var node in unknowns)
            {
                if (!handledThisTurn.ContainsKey(node.ID) && !node.isKnown())
                {
                    handledThisTurn.Add(node.ID, 0);
                    var neighbours = board.getNeighbours(node.ID);
                    foreach (var neighbour in neighbours)
                    {
                        if (neighbour.isKnown() && node.getCandidates().Contains(neighbour.Value))
                            node.removeCandidate(neighbour.Value);
                    }
                    if (node.getCandidates().Length == 1)
                        needAnotherTechnique = false;
                }
            }
            return needAnotherTechnique;
        }

        public static bool removeNakedCandidates(SudokuBoard board, List<SudokuNode> unknowns)
        {
            bool needAnotherTechnique = true;

            //making sure we only check each unit once...
            BitArray rowState = new BitArray(9, false);
            BitArray colState = new BitArray(9, false);
            BitArray blockState = new BitArray(9, false);
            Func<int, bool> colUnhandled = (col) =>
            {
                bool org = colState.Get(col);
                colState.Set(col, true);
                return org == false;
            };
            Func<int, bool> rowUnhandled = (row) =>
            {
                bool org = rowState.Get(row);
                rowState.Set(row, true);
                return org == false;
            };
            Func<int, bool> blockUnhandled = (block) =>
            {
                bool org = blockState.Get(block);
                blockState.Set(block, true);
                return org == false;
            };
            //---

            foreach (var node in unknowns)
            {
                //check rows
                if (rowUnhandled(node.Row))
                {
                    var rowNeighbours = board.getNodesByRow(node.Row);
                    if (removedNakedPair(rowNeighbours))
                        needAnotherTechnique = false;
                }

                //check cols
                if (colUnhandled(node.Col))
                {
                    var colNeighbours = board.getNodesByColumn(node.Col);
                    if (removedNakedPair(colNeighbours))
                        needAnotherTechnique = false;
                }

                //check blocks
                if (blockUnhandled(node.Block))
                {
                    var blockNeighbours = board.getNodesByBlock(node.Block);
                    if (removedNakedPair(blockNeighbours))
                        needAnotherTechnique = false;
                }
            }

            return needAnotherTechnique;
        }

        private static bool removedNakedPair(List<SudokuNode> unitNeighbours)
        {
            //helper function to know what combinations we have allready checked
            var checkedCombinations = new List<int>();
            Func<int, int, bool> candidatesUntested = (a, b) =>
            {
                int id = a << 16 + b;
                if (checkedCombinations.Contains(id))
                    return false;

                checkedCombinations.Add(id);
                return true;
            };

            bool progress = false;
            //find nodes that have 2 candidates
            var potentialPairs = unitNeighbours.FindAll(a => { return a.getCandidates().Length == 2; });
            if (potentialPairs.Count < 2)
                return false;

            //check candidates to see if they are a true match (exactly two items using same two values)
            for (int i = 0; i < potentialPairs.Count; ++i)
            {
                var c = potentialPairs[i].getCandidates();
                if (c.Length != 2)
                    continue; //item has been changed due to previous iterations of algorithm
                if (candidatesUntested(c[0], c[1]))
                {
                    SudokuNode exactMatch = null;
                    //check candidates against all other potential matches
                    for (int j = i + 1; j < potentialPairs.Count; ++j)
                    {
                        var d = potentialPairs[j].getCandidates();
                        if (d.Length == 2 && c[0] == d[0] && c[1] == d[1])
                        {
                            if (exactMatch != null)
                                throw new Exception("Logic Error!+!?!");
                            exactMatch = potentialPairs[j];
                            //break; //TODO#: uncomment and remove exception
                        }
                    }

                    if (exactMatch != null)
                    {
                        //we know that these candidates can only exist on these two nodes, and can hence remove these candidates from all other nodes in this unit
                        var diff = unitNeighbours.Except(new SudokuNode[2] { potentialPairs[i], exactMatch });
                        removeCandidatesFromNodes(diff, c[0], c[1]);
                    }
                }
            }
            return progress;
        }

        private static bool removeCandidatesFromNodes(IEnumerable<SudokuNode> nodes, params int[] candidates)
        {
            bool candidatesRemoved = false;
            foreach (var item in nodes)
            {
                if (!item.isKnown())
                {
                    var orgCount = item.getCandidates().Length;
                    foreach (var c in candidates)
                        item.removeCandidate(c);
                    if (orgCount != item.getCandidates().Length)
                        candidatesRemoved = true;
                }
            }
            return candidatesRemoved;
        }

        public static bool removeHiddenBasic(SudokuBoard board, List<SudokuNode> unknowns)
        {
            bool needAnotherTechnique = true;
            foreach (var node in unknowns)
            {
                bool progress = false;
                //check rows
                {
                    var rowNeighbours = board.getNodesByRow(node.Row);
                    if (findHiddenBasic(rowNeighbours))
                        progress = true;
                }

                //check cols
                {
                    var colNeighbours = board.getNodesByColumn(node.Col);
                    if (findHiddenBasic(colNeighbours))
                        progress = true;
                }

                //check blocks
                {
                    var blockNeighbours = board.getNodesByBlock(node.Block);
                    if (findHiddenBasic(blockNeighbours))
                        progress = true;
                }

                //end of iteration
                if (progress || node.getCandidates().Length == 1)
                    needAnotherTechnique = false;
            }
            return needAnotherTechnique;
        }

        public static bool findHiddenBasic(List<SudokuNode> neighbours)
        {
            bool success = false;
            var potentialValues = new Dictionary<int, List<SudokuNode>>();
            for (int i = 0; i < 9; ++i)
                potentialValues.Add(i + 1, new List<SudokuNode>());
            foreach (var node in neighbours)
            {
                foreach (var c in node.getCandidates())
                    potentialValues[c].Add(node);
            }

            //if a number is only referenced by 1 node, that node has to be that number
            var foo = potentialValues.Where(a => a.Value.Count == 1);
            int n = foo.Count();
            if (n > 0)
            {
                for (int i = 0; i < n; ++i)
                {
                    var ele = foo.ElementAt(i);
                    if (ele.Value.Count > 1)
                        throw new Exception("Foobar!");
                    var node = ele.Value[0];
                    var cand = node.getCandidates();
                    var orgCount = node.getCandidates().Length;
                    foreach (var c in cand)
                    {
                        if (c != ele.Key)
                        {
                            node.removeCandidate(c);
                        }
                    }
                    if (orgCount != node.getCandidates().Length)
                        success = true;
                }
            }
            return success;
        }

        public static bool removeHiddenCandidates(SudokuBoard board, List<SudokuNode> unknowns)
        {
            bool needAnotherTechnique = true;
            foreach (var node in unknowns)
            {
                bool progress = false;
                //check rows
                {
                    //if row contains exactly 2 nodes that have candidates that can only be found in these 2 nodes...
                    var rowNeighbours = board.getNodesByRow(node.Row);
                    if (findHidden(rowNeighbours))
                        progress = true;
                }

                //check cols
                {
                    var colNeighbours = board.getNodesByColumn(node.Col);
                    if (findHidden(colNeighbours))
                        progress = true;
                }

                //check blocks
                {
                    var blockNeighbours = board.getNodesByBlock(node.Block);
                    if (findHidden(blockNeighbours))
                        progress = true;
                }

                //end of iteration
                if (progress || node.getCandidates().Length == 1)
                    needAnotherTechnique = false;
            }
            return needAnotherTechnique;
        }

        private static bool findHidden(List<SudokuNode> neighbours)
        {
            bool success = false;
            var potentialValues = new Dictionary<int, List<SudokuNode>>();
            for (int i = 0; i < 9; ++i)
                potentialValues.Add(i + 1, new List<SudokuNode>());
            foreach (var node in neighbours)
            {
                foreach (var c in node.getCandidates())
                    potentialValues[c].Add(node);
            }

            //if a number only has 2 nodes ... and another number only has 2 nodes, and those numbers share the same nodes...
            var foo = potentialValues.Where(a => a.Value.Count == 2);
            int n= foo.Count();
            if (n > 1)
            {
                for (int i = 0; i < n; ++i)
                {
                    bool iterationSuccess = false;
                    var ele = foo.ElementAt(i);
                    SudokuNode aNode = ele.Value[0];
                    SudokuNode bNode = ele.Value[1];
                    int pairedNum = 0;
                    for (int j = i + 1; j < n; ++j)
                    {
                        var pair = foo.ElementAt(j);
                        if (pair.Value.Contains(aNode) && pair.Value.Contains(bNode))
                        {
                            if(pairedNum == 0)
                                pairedNum = pair.Key;
                            else {
                                //more than 2 items...
                                pairedNum = 0;
                                break;
                            }
                        }
                    }

                    if (pairedNum > 0)
                    {
                        int aVal = pairedNum;
                        int bVal = ele.Key;
                        int aCandsStart = aNode.getCandidates().Length;
                        int bCandsStart = bNode.getCandidates().Length;
                        var cands = aNode.getCandidates();
                        foreach (var c in cands)
                        {
                            if (c != aVal && c != bVal)
                            {
                                aNode.removeCandidate(c);
                                iterationSuccess = true;
                            }
                        }
                        cands = bNode.getCandidates();
                        foreach (var c in cands)
                        {
                            if (c != aVal && c != bVal)
                            {
                                bNode.removeCandidate(c);
                                iterationSuccess = true;
                            }
                        }

                        if (iterationSuccess)
                        {
                            success = true;
                            Console.WriteLine("Using removeHidden, we successfully reduced candidates for {0} from originally {1} candidates", aNode, aCandsStart);
                            Console.WriteLine("Using removeHidden, we successfully reduced candidates for {0} from originally {1} candidates", bNode, bCandsStart);
                        }
                    }
                }
            }
            return success;
        }

        public static bool removeSingleChainCandidates(SudokuBoard board, List<SudokuNode> unknowns)
        {
            Func<SudokuNode, SudokuNode, ulong> createIdentifier = (a, b) =>
            {
                var x = a.ID;
                var y = b.ID;

                ulong id = x > y ? (uint)y | ((ulong)x << 32) :
                   (uint)x | ((ulong)y << 32);
                return id;
            };
            Func<SudokuNode, SudokuNode, KeyValuePair<ulong, KeyValuePair<SudokuNode, SudokuNode>>> foobar = (alpha, beta) =>
            {
                var x = alpha.ID;
                var y = beta.ID;

                ulong id = x > y ? (uint)y | ((ulong)x << 32) : (uint)x | ((ulong)y << 32);
                return new KeyValuePair<ulong, KeyValuePair<SudokuNode, SudokuNode>>(id, new KeyValuePair<SudokuNode, SudokuNode>(alpha, beta));
            };

            bool needAnotherTechnique = true;

            for (int candidate = 0; candidate < 9; ++candidate)
            {
                var allNodesWithCandidate = unknowns.FindAll( a => a.getCandidates().Contains(candidate));
                var pairs = new Dictionary<ulong, KeyValuePair<SudokuNode, SudokuNode>>();

                //fill units
                for(int i=0; i<9; ++i) {
                    var nodesOnRow = allNodesWithCandidate.FindAll(a => a.Row == i);
                    var nodesOnCol = allNodesWithCandidate.FindAll(a => a.Col == i);
                    var nodesOnBlock = allNodesWithCandidate.FindAll(a => a.Block == i);

                    if (nodesOnRow.Count == 2)
                    {
                        var p = foobar(nodesOnRow[0], nodesOnRow[1]);
                        if (!pairs.ContainsKey(p.Key))
                            pairs.Add(p.Key, p.Value);
                    }
                    if (nodesOnCol.Count == 2)
                    {
                        var p = foobar(nodesOnCol[0], nodesOnCol[1]);
                        if (!pairs.ContainsKey(p.Key))
                            pairs.Add(p.Key, p.Value);
                    }
                    if (nodesOnBlock.Count == 2)
                    {
                        var p = foobar(nodesOnBlock[0], nodesOnBlock[1]);
                        if(!pairs.ContainsKey(p.Key))
                            pairs.Add(p.Key, p.Value);
                    }
                }

                foreach (var pair in pairs.Values)
                {
                    //pair.Key.Value = 5;
                }

            }

            return needAnotherTechnique;
        }

        public static bool removeSimpleColoringCandidates(SudokuBoard board, List<SudokuNode> unknowns)
        {
            for (int candidate = 0; candidate < 9; ++candidate)
            {
                foreach (var startNode in unknowns)
                {
                    //Single Chains
                    //desc: remove candidates due to building a hierarchy tree between nodes that share the same candidate
                    // for every depth traversal, you change between toggling node as "on" or "off"
                    // finding start point:
                    //  need to find a node that have a candidate that is shared between EXACTLY two nodes inside a [row/col/block] 
                    // that means it can be shared both due to block, or due to row, but not more than 2 items for each type

                    if (startNode.getCandidates().Contains(candidate))
                    {
                        var openNodes = new List<SudokuNode>();
                        var neighbours = board.getNeighbours(startNode.ID);
                        var sharedCandidates = neighbours.FindAll(a => a != startNode && a.getCandidates().Contains(candidate));
                        var rowMatches = sharedCandidates.FindAll(a => a.Row == startNode.Row);
                        var colMatches = sharedCandidates.FindAll(a => a.Col == startNode.Col);
                        var blockMatches = sharedCandidates.FindAll(a => a.Block == startNode.Block);

                        //if there only are two items in a row/col/block we can start counting from this item...
                        if (rowMatches.Count == 1 || colMatches.Count == 1 || blockMatches.Count == 1)
                        {
                            var statusDecorator = new Dictionary<SudokuNode, bool>();

                            if (rowMatches.Count == 1)
                                openNodes.AddRange(rowMatches);
                            if (colMatches.Count == 1)
                                openNodes.AddRange(colMatches);
                            if (blockMatches.Count == 1)
                                openNodes.AddRange(blockMatches);

                            statusDecorator.Add(startNode, true);
                            foreach(var n in sharedCandidates) {
                                if(!statusDecorator.ContainsKey(n)) {
                                    statusDecorator.Add(n, !statusDecorator[startNode]);
                                }
                            }

                            while (openNodes.Count > 0)
                            {
                                var currNode = openNodes[0];
                                openNodes.RemoveAt(0);
                                if (statusDecorator.ContainsKey(currNode))
                                    continue;

                                neighbours = board.getNeighbours(currNode.ID);
                                sharedCandidates = neighbours.FindAll(a => a != currNode && a.getCandidates().Contains(candidate));
                                rowMatches = sharedCandidates.FindAll(a => a.Row == currNode.Row);
                                colMatches = sharedCandidates.FindAll(a => a.Col == currNode.Col);
                                blockMatches = sharedCandidates.FindAll(a => a.Block == currNode.Block);

                                if (rowMatches.Count == 1)
                                    openNodes.Add(rowMatches[0]);
                                if (colMatches.Count == 1)
                                    openNodes.AddRange(colMatches);
                                if (blockMatches.Count == 1)
                                    openNodes.AddRange(blockMatches);

                                foreach (var n in sharedCandidates)
                                {
                                    if (!statusDecorator.ContainsKey(n))
                                        statusDecorator.Add(n, !statusDecorator[currNode]);
                                }
                            }

                            //all items have been searched for this value...
                            //verify the items that we traversed and see if we have two items in same "block/col/row" where they are "on" or "off"
                            foreach (var pair in statusDecorator)
                            {
                                bool itemStatus = statusDecorator[pair.Key];
                                var checks = board.getNeighbours(pair.Key.ID);
                                var foobars = new List<SudokuNode>();
                                foobars.Add(pair.Key);
                                checks.ForEach((a) =>
                                {
                                    if (statusDecorator.ContainsKey(a) && statusDecorator[a] == itemStatus)
                                    {
                                        foobars.Add(a);
                                    }
                                });

                                if (foobars.Count > 1)
                                {
                                    //foreach (var p in statusDecorator)
                                    //{
                                    //    if (p.Value == itemStatus)
                                    //    {
                                    //        p.Key.removeCandidate(candidate);
                                    //    }
                                    //}
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static bool removeCubeCandidates(SudokuBoard board, List<SudokuNode> unknowns)
        {
            //throw new NotImplementedException();
            return true;
        }

        public static bool removeNakedManyCandidates(SudokuBoard board, List<SudokuNode> unknowns, int depth)
        {
            //basically we base this solution against subsets, can the subset of candidates from x different nodes fit in 4 nodes?
            bool needAnotherTechnique = true;

            foreach (var baseNode in unknowns)
            {
                //block
                {
                    //find nodes where number of candidates are in range
                    var nodes = board.getNodesByBlock(baseNode.Block).FindAll( a => a.getCandidates().Length >= 2 && a.getCandidates().Length <= depth);
                    for(int i=0; i < nodes.Count; ++i)
                    {
                        for (int j = i + 1; j < nodes.Count; ++j)
                        {
                            for (int k = j + 1; k < nodes.Count; ++k)
                            {
                                for (int l = k + 1; l < nodes.Count; ++l)
                                {
                                    var candidates = Combine(nodes[i].getCandidates(), nodes[j].getCandidates(), nodes[k].getCandidates(), nodes[l].getCandidates()).Distinct();

                                    if (candidates.Count() <= depth)
                                    {
                                        //the entire subset of candidates from all nodes can fit inside x nodes
                                        var closedSet = new List<SudokuNode>() { nodes[i], nodes[j], nodes[k], nodes[l] };
                                        var otherBlockNodes = board.getNodesByBlock(baseNode.Block).Except(closedSet);

                                        foreach (var node in otherBlockNodes)
                                        {
                                            var oldSet = node.getCandidates();
                                            var newSet = oldSet.Except(candidates).ToArray();
                                            if (oldSet.Length > newSet.Length)
                                            {
                                                node.setCandidates(newSet);
                                                needAnotherTechnique = false;
                                            }
                                        }
                                        //break if something was changed
                                        if (needAnotherTechnique == false)
                                            return needAnotherTechnique;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return needAnotherTechnique;
        }

        public static IEnumerable<T> Combine<T>(params ICollection<T>[] toCombine)
        {
            return toCombine.SelectMany(x => x);
        }

        public static bool removePointingPairs(SudokuBoard board, List<SudokuNode> unknowns)
        {
            bool needAnotherTechnique = true;

            //  row | col | block
            //if a number exists in numerous places inside a row/col and they are the only valid places inside a block, 
            //then we know that they must be in that row/col

            //check each block for potentials that only exist in one row or col (and only in that row/col)
            //if true, we can remove that potential number from other cells on that row/col

            for (var i = 0; i < 9; ++i)
            {
                var blockCells = board.getNodesByBlock(i).FindAll(a => a.isKnown() == false);
                for(var candidate=1; candidate <= 9; ++candidate) {
                    var foo = blockCells.FindAll(a => a.getCandidates().Contains(candidate));
                    if (foo.Count >= 2)
                    {
                        var nodeRef = foo[0];
                        bool pointingPairRow = foo.TrueForAll(a => a.Row == nodeRef.Row);
                        bool pointingPairCol = foo.TrueForAll(a => a.Col == nodeRef.Col);
                        if (pointingPairRow || pointingPairCol)
                        {
                            if (pointingPairRow == pointingPairCol) throw new Exception("Foobar!");
                            if (pointingPairRow)
                            {
                                var rowNodes = board.getNodesByRow(nodeRef.Row);
                                var matches = rowNodes.FindAll( a => a.Block != nodeRef.Block && a.getCandidates().Contains(candidate));
                                if (matches.Count > 0)
                                {
                                    needAnotherTechnique = false;
                                    matches.ForEach((a) =>
                                    {
                                        a.removeCandidate(candidate);
                                    });
                                    break;
                                }
                            }
                            else
                            {
                                var colNodes = board.getNodesByColumn(nodeRef.Col);
                                var matches = colNodes.FindAll(a => a.Block != nodeRef.Block && a.getCandidates().Contains(candidate));
                                if (matches.Count > 0)
                                {
                                    needAnotherTechnique = false;
                                    matches.ForEach((a) =>
                                    {
                                        a.removeCandidate(candidate);
                                    });
                                    break;
                                }
                            }
                        }
                    }
                }
                
            }

            return needAnotherTechnique;
        }
    }
}

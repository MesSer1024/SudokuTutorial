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
                return string.Format("ID=[{3}], Cell: {0}{1}, Block: {4}, Value={2}", Convert.ToChar(65 + Row), Col + 1, Value, ID, Block + 1);
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var c in _candidates)
                {
                    sb.Append(c);
                }
                return string.Format("ID=[{3}], Cell: {0}{1}, Block: {4}, Candidate(s)={2}", Convert.ToChar(65 + Row), Col + 1, sb.ToString(), ID, Block + 1);
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
                var c = node.getCandidates();
                if (!handledThisTurn.ContainsKey(node.ID) && !node.isKnown())
                {
                    handledThisTurn.Add(node.ID, 0);
                    var knownNeighbours = board.getNeighbours(node.ID).FindAll(a => a.isKnown() && c.Contains(a.Value));
                    foreach (var neighbour in knownNeighbours)
                    {
                        node.removeCandidate(neighbour.Value);
                    }
                    if (node.getCandidates().Length == 1)
                        needAnotherTechnique = false;
                }
            }
            return needAnotherTechnique;
        }

        public static bool removeHiddenBasic(SudokuBoard board, List<SudokuNode> unknowns)
        {
            bool needAnotherTechnique = true;
            foreach (var node in unknowns)
            {
                //check rows
                {
                    var rowNeighbours = board.getNodesByRow(node.Row);
                    if (findHiddenBasic(rowNeighbours))
                        needAnotherTechnique = false;
                }

                //check cols
                {
                    var colNeighbours = board.getNodesByColumn(node.Col);
                    if (findHiddenBasic(colNeighbours))
                        needAnotherTechnique = false;
                }

                //check blocks
                {
                    var blockNeighbours = board.getNodesByBlock(node.Block);
                    if (findHiddenBasic(blockNeighbours))
                        needAnotherTechnique = false;
                }
            }
            return needAnotherTechnique;
        }

        public static bool findHiddenBasic(List<SudokuNode> neighbours)
        {
            bool success = false;
            for (int i = 0; i < 9; ++i)
            {
                int candidate = i + 1;
                var matches = neighbours.FindAll(a => a.getCandidates().Contains(candidate)).ToList();
                if (matches.Count == 1)
                {
                    //if a number is only referenced by 1 node, that node has to be that number
                    matches[0].setCandidates(new[] { candidate }); //success of a technique is measured with item ending with only 1 candidate...
                    success = true;
                }
            }
            return success;
        }

        private static Dictionary<int, List<SudokuNode>> populateCandidates(List<SudokuNode> nodes)
        {
            var potentialValues = new Dictionary<int, List<SudokuNode>>();
            for (int i = 0; i < 9; ++i)
                potentialValues.Add(i + 1, new List<SudokuNode>());
            foreach (var node in nodes)
            {
                foreach (var c in node.getCandidates())
                    potentialValues[c].Add(node);
            }
            return potentialValues;
        }

        public static bool removeHiddenCandidates(SudokuBoard board, List<SudokuNode> unknowns, int depth)
        {
            bool needAnotherTechnique = true;
            Func<List<SudokuNode>, bool> fn = null;
            switch (depth)
            {
                case 2:
                    fn = findHiddenPair;
                    break;
                case 3:
                    fn = findHiddenTriplets;
                    break;
                case 4:
                    fn = findHiddenQuads;
                    break;
            }

            foreach (var baseNode in unknowns)
            {
                {
                    var nodes = board.getNodesByBlock(baseNode.Block).FindAll(a => a.isKnown() == false);
                    if (fn.Invoke(nodes))
                    {
                        needAnotherTechnique = false;
                    }
                }
                {
                    var nodes = board.getNodesByRow(baseNode.Row).FindAll(a => a.isKnown() == false);
                    if (fn.Invoke(nodes))
                    {
                        needAnotherTechnique = false;
                    }
                }
                {
                    var nodes = board.getNodesByColumn(baseNode.Col).FindAll(a => a.isKnown() == false);
                    if (fn.Invoke(nodes))
                    {
                        needAnotherTechnique = false;
                    }
                }
            }
            return needAnotherTechnique;
        }

        private static bool findHiddenPair(List<SudokuNode> neighbours)
        {
            int depth = 2;
            bool success = false;
            var candidateMap = populateCandidates(neighbours).Where(a => a.Value.Count == depth).ToDictionary( a => a.Key, a => a.Value);
            var keys = candidateMap.Keys;
            if (candidateMap.Keys.Count >= depth)
            {
                for (int i = 0; i < keys.Count; ++i)
                {
                    for (int j = i + 1; j < keys.Count; ++j)
                    {
                        var orgNodes = candidateMap[keys.ElementAt(i)];
                        var compNodes = candidateMap[keys.ElementAt(j)];
                        bool hasSameNodes = Enumerable.Union(orgNodes, compNodes).Count() == depth;
                        if (hasSameNodes)
                        {
                            //the different sets contain exactly the same nodes, meaning that the candidates are shared among these nodes
                            int totalCandidates = 0;
                            orgNodes.ForEach(a => totalCandidates += a.getCandidates().Length);
                            if (totalCandidates > depth * 2) //could also go for comparing depth towards a distinct list of candidates, but depth * 2 seems easier
                            {
                                orgNodes.ForEach(a => a.setCandidates(new[] { keys.ElementAt(i), keys.ElementAt(j) })); //remove all unneccessary candidates
                                success = true;
                                break;
                            }
                        }
                    }
                }
            }

            return success;
        }

        private static bool findHiddenTriplets(List<SudokuNode> neighbours)
        {
            int depth = 3;
            bool success = false;
            var candidateMap = populateCandidates(neighbours).Where(a => a.Value.Count >= 2 && a.Value.Count <= depth).ToDictionary(a => a.Key, a => a.Value);
            var keys = candidateMap.Keys;
            if (candidateMap.Keys.Count >= depth)
            {
                for (int i = 0; i < keys.Count; ++i)
                {
                    for (int j = i + 1; j < keys.Count; ++j)
                    {
                        for (int k = j + 1; k < keys.Count; ++k)
                        {
                            var nodes1 = candidateMap[keys.ElementAt(i)];
                            var nodes2 = candidateMap[keys.ElementAt(j)];
                            var nodes3 = candidateMap[keys.ElementAt(k)];
                            var union = Enumerable.Union(nodes1, nodes2).Union(nodes3);
                            bool hasSameNodes = union.Count() == depth;
                            if (hasSameNodes)
                            {
                                //the different sets contain exactly the same nodes, meaning that the candidates are shared among these nodes
                                var allCandidates = new List<int>();
                                foreach (var n in union)
                                {
                                    allCandidates.AddRange(n.getCandidates());
                                }
                                if (allCandidates.Distinct().Count() > depth)
                                {
                                    var combinedSet = new[] { keys.ElementAt(i), keys.ElementAt(j), keys.ElementAt(k) };
                                    foreach (var n in union)
                                    {
                                        var orgSet = n.getCandidates();
                                        var set = orgSet.Intersect(combinedSet); //remove all unused candidates
                                        n.setCandidates(set); 
                                    }
                                    success = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return success;
        }

        private static bool findHiddenQuads(List<SudokuNode> neighbours)
        {
            int depth = 4;
            bool success = false;
            var candidateMap = populateCandidates(neighbours).Where(a => a.Value.Count >= 2 && a.Value.Count <= depth).ToDictionary(a => a.Key, a => a.Value);
            var keys = candidateMap.Keys;
            if (candidateMap.Keys.Count >= depth)
            {
                for (int i = 0; i < keys.Count; ++i)
                {
                    for (int j = i + 1; j < keys.Count; ++j)
                    {
                        for (int k = j + 1; k < keys.Count; ++k)
                        {
                            for (int l = k + 1; l < keys.Count; ++l)
                            {
                                var nodes1 = candidateMap[keys.ElementAt(i)];
                                var nodes2 = candidateMap[keys.ElementAt(j)];
                                var nodes3 = candidateMap[keys.ElementAt(k)];
                                var nodes4 = candidateMap[keys.ElementAt(l)];
                                var union = Enumerable.Union(nodes1, nodes2).Union(nodes3).Union(nodes4);
                                bool hasSameNodes = union.Count() == depth;
                                if (hasSameNodes)
                                {
                                    //the different sets contain exactly the same nodes, meaning that the candidates are shared among these nodes
                                    var allCandidates = new List<int>();
                                    foreach (var n in union)
                                    {
                                        allCandidates.AddRange(n.getCandidates());
                                    }
                                    if (allCandidates.Distinct().Count() > depth)
                                    {
                                        var combinedSet = new[] { keys.ElementAt(i), keys.ElementAt(j), keys.ElementAt(k), keys.ElementAt(l) };
                                        foreach (var n in union)
                                        {
                                            var orgSet = n.getCandidates();
                                            var set = orgSet.Intersect(combinedSet); //remove all unused candidates
                                            n.setCandidates(set);
                                        }
                                        success = true;
                                        break;
                                    }
                                }
                            }
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

        public static bool removeNakedCandidates(SudokuBoard board, List<SudokuNode> unknowns, int depth)
        {
            //basically we base this solution against subsets, can the subset of candidates from x different nodes be bound to x nodes?
            bool needAnotherTechnique = true;
            Func<List<SudokuNode>, bool> fn = null;
            switch (depth)
            {
                case 2:
                    fn = removeNakedPair;
                    break;
                case 3:
                    fn = removeNakedTriples;
                    break;
                case 4:
                    fn = removeNakedQuads;
                    break;
            }

            foreach (var baseNode in unknowns)
            {
                {
                    var nodes = board.getNodesByBlock(baseNode.Block).FindAll(a => a.isKnown() == false);
                    if (fn.Invoke(nodes))
                    {
                        needAnotherTechnique = false;
                    }
                }
                {
                    var nodes = board.getNodesByRow(baseNode.Row).FindAll(a => a.isKnown() == false);
                    if (fn.Invoke(nodes))
                    {
                        needAnotherTechnique = false;
                    }
                }
                {
                    var nodes = board.getNodesByColumn(baseNode.Col).FindAll(a => a.isKnown() == false);
                    if (fn.Invoke(nodes))
                    {
                        needAnotherTechnique = false;
                    }
                }
            }

            return needAnotherTechnique;
        }

        private static bool removeNakedPair(List<SudokuNode> unitNodes)
        {
            int depth = 2;
            var nodes = unitNodes.FindAll(a => a.getCandidates().Length == depth);
            for (int i = 0; i < nodes.Count; ++i)
            {
                for (int j = i + 1; j < nodes.Count; ++j)
                {
                    var candidates = combineCandidates(nodes[i], nodes[j]).Distinct();

                    if (candidates.Count() <= depth)
                    {
                        //the entire subset of candidates from all nodes can fit inside x nodes
                        var closedSet = new List<SudokuNode>() { nodes[i], nodes[j] };
                        var otherBlockNodes = unitNodes.Except(closedSet);
                        bool couldRemoveCandidate = false;
                        foreach (var node in otherBlockNodes)
                        {
                            var oldSet = node.getCandidates();
                            var newSet = oldSet.Except(candidates).ToArray();
                            if (oldSet.Length > newSet.Length)
                            {
                                node.setCandidates(newSet);
                                couldRemoveCandidate = true;
                            }
                        }
                        //break if something was changed
                        if (couldRemoveCandidate)
                            return true;
                    }
                }
            }
            return false;
        }

        private static bool removeNakedTriples(List<SudokuNode> unitNodes)
        {
            int depth = 3;
            var nodes = unitNodes.FindAll(a => a.getCandidates().Length >= 2 && a.getCandidates().Length <= depth);
            for (int i = 0; i < nodes.Count; ++i)
            {
                for (int j = i + 1; j < nodes.Count; ++j)
                {
                    for (int k = j + 1; k < nodes.Count; ++k)
                    {
                        var candidates = combineCandidates(nodes[i], nodes[j], nodes[k]).Distinct();

                        if (candidates.Count() <= depth)
                        {
                            //the entire subset of candidates from all nodes can fit inside x nodes
                            var closedSet = new List<SudokuNode>() { nodes[i], nodes[j], nodes[k] };
                            var otherBlockNodes = unitNodes.Except(closedSet);
                            bool couldRemoveCandidate = false;
                            foreach (var node in otherBlockNodes)
                            {
                                var oldSet = node.getCandidates();
                                var newSet = oldSet.Except(candidates).ToArray();
                                if (oldSet.Length > newSet.Length)
                                {
                                    node.setCandidates(newSet);
                                    couldRemoveCandidate = true;
                                }
                            }
                            //break if something was changed
                            if (couldRemoveCandidate)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool removeNakedQuads(List<SudokuNode> unitNodes)
        {
            int depth = 4;
            var nodes = unitNodes.FindAll(a => a.getCandidates().Length >= 2 && a.getCandidates().Length <= depth);
            for (int i = 0; i < nodes.Count; ++i)
            {
                for (int j = i + 1; j < nodes.Count; ++j)
                {
                    for (int k = j + 1; k < nodes.Count; ++k)
                    {
                        for (int l = k + 1; l < nodes.Count; ++l)
                        {
                            var candidates = combineCandidates(nodes[i], nodes[j], nodes[k], nodes[l]).Distinct();

                            if (candidates.Count() <= depth)
                            {
                                //the entire subset of candidates from all nodes can fit inside x nodes
                                var closedSet = new List<SudokuNode>() { nodes[i], nodes[j], nodes[k], nodes[l] };
                                var otherBlockNodes = unitNodes.Except(closedSet);
                                bool couldRemoveCandidate = false;
                                foreach (var node in otherBlockNodes)
                                {
                                    var oldSet = node.getCandidates();
                                    var newSet = oldSet.Except(candidates).ToArray();
                                    if (oldSet.Length > newSet.Length)
                                    {
                                        node.setCandidates(newSet);
                                        couldRemoveCandidate = true;
                                    }
                                }
                                //break if something was changed
                                if (couldRemoveCandidate)
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static List<int> combineCandidates(params SudokuNode[] nodes)
        {
            List<int> foo = new List<int>();
            foreach (var node in nodes)
            {
                foo.AddRange(node.getCandidates());
            }
            return foo;
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

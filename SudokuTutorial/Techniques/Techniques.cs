﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//---------------- Thonky -------------
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
//hidden singles
//naked pairs/triples
//hidden pairs/triples
//naked quads
//pointing pairs
//box/line reduction

//x-wing
//Simple Colouring
//Y-wing
//swordfish
//xyz-wing

//X-cycles
//XY-chain
//3d medusa
//jelly-fish
//unique rectangles
//extended unique rectangles
//hidden unique rectangles
//wxyz wing
//aligned pair exclusion

//grouped x-cycles
//empty rectangles
//finned x-wing
//finned sword-fish
//altern. inference chains
//sue-de-coq
//digit forcing chains
//nishio forcing chains
//cell forcing chains
//unit forcing chains
//almost locked sets
//death blossom
//pattern overlay method
//quad forcing chains

//bowman's bingo

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
    }

    public class SudokuNode
    {
        private int _boardValue;
        private List<int> _candidates;
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
            }
        }

        public int[] getCandidates()
        {
            return _candidates.ToArray();
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
            return Value != 0 || (_candidates != null && _candidates.Count == 1);
        }

        public int Row { get { return _id / 9; } }
        public int Col { get { return _id % 9; } }
        public int Block { get { return (Row / 3) * 3 + Col / 3; } }

        public void removeCandidate(int value)
        {
            _candidates.Remove(value);
        }

        public override string ToString()
        {
            if (isKnown())
            {
                return string.Format("ID=[{3}], Row: {0}, Col: {1}, Value={2}", Row + 1, Col + 1, Value, ID);
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var c in _candidates)
                {
                    sb.Append(c);
                }
                return string.Format("ID=[{3}], Row: {0}, Col: {1}, Candidate(s)={2}", Row + 1, Col + 1, sb.ToString(), ID);
            }
        }
    }

    public class SudokuBoard
    {
        private List<SudokuNode> _nodes;
        public List<SudokuNode> getNodes() { return _nodes; }

        public SudokuBoard()
        {
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
        public static void removeBasicCandidates(SudokuBoard board, List<SudokuNode> unknowns)
        {
            var handledThisTurn = new Dictionary<int, bool>();
            foreach (var node in unknowns)
            {
                if (!handledThisTurn.ContainsKey(node.ID) && !node.isKnown())
                {
                    handledThisTurn.Add(node.ID, true);
                    var neighbours = board.getNeighbours(node.ID);
                    foreach (var neighbour in neighbours)
                    {
                        if (neighbour.isKnown())
                            node.removeCandidate(neighbour.Value);
                    }
                    
                }
            }
        }
    }
}

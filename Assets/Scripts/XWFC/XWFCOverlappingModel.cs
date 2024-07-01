using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using PatternWave = XWFC.Grid<bool[]>;

namespace XWFC
{
    public class XWFCOverlappingModel : XWFC
    {
        public readonly PatternMatrix PatternMatrix;
        public AdjacencyMatrix AdjacencyMatrix;
        private PatternWave _patternWave;
        private int _nPatterns;
        private readonly Grid<int> _atomGrid;

        private Queue<Vector3Int> _propagationQueue = new();
        private readonly Vector3Int _patternAtomCoord;

        public XWFCOverlappingModel([NotNull] AdjacencyMatrix adjacencyMatrix, [NotNull] ref Grid<int> seededGrid, Vector3Int kernelSize, bool forceCompleteTiles = true) : base(adjacencyMatrix, ref seededGrid, forceCompleteTiles)
        {
            PatternMatrix = new PatternMatrix(adjacencyMatrix.AtomizedSamples, kernelSize, adjacencyMatrix.AtomMapping);
            AdjacencyMatrix = adjacencyMatrix;
            
            _nPatterns = PatternMatrix.Patterns.Count;
            
            // Initialize wave in superposition.
            _patternWave = new PatternWave(seededGrid.GetExtent(), SuperImposedWave());
            _atomGrid = new Grid<int>(seededGrid.GetExtent(), seededGrid.DefaultFillValue);
            
            CollapseQueue.Insert(new Vector3Int(0,0,0), AdjacencyMatrix.CalcEntropy(_nPatterns));
            Collapse(CollapseQueue.DeleteHead().Coord);

            // This coordinate is used to set the base for the patterns; due to how patterns are constructed, the relative coordinate is always the same for each pattern. 
            _patternAtomCoord = new Vector3Int(0, 0, 0);
        }
        
        private bool[] SuperImposedWave()
        {
            return Enumerable.Repeat(true, _nPatterns).ToArray();
        }
        
        private bool[] EmptyWave()
        {
            return new bool[_nPatterns];
        }

        private void Collapse(Vector3Int coord)
        {
            var choices = _patternWave.Get(coord);

            var chosenPatternId = RandomChoice(choices, Enumerable.Repeat(1.0f, _nPatterns).ToArray());
            var chosenAtom = PatternMatrix.GetPatternAtomAtCoordinate(chosenPatternId, _patternAtomCoord);
            
            /*
             */
            var (tileId, atomCoord, orientation) = AdjacencyMatrix.AtomMapping.Get(chosenAtom);
            var atomCoords = AdjacencyMatrix.TileSet[tileId].OrientedIndices[0];
            
            foreach (var ac in atomCoords)
            {
                var relativePosition = ac - atomCoord;
                if (!_atomGrid.WithinBounds(coord + relativePosition)) continue;
                _atomGrid.Set(relativePosition, AdjacencyMatrix.AtomMapping.Get((tileId, ac, orientation)));
            }
            
            /*
             * TODO: propagation after tile placement.
             * TODO: precompute the pattern masks per tile.
             */
            
            var updatedWave = EmptyWave();
            updatedWave[chosenPatternId] = true;
            _patternWave.Set(coord, updatedWave);
            _atomGrid.Set(coord, chosenAtom);

            _propagationQueue.Enqueue(coord);
            Propagate();
        }

        private new void Propagate()
        {
            while (_propagationQueue.Count > 0)
            {
                var coord = _propagationQueue.Dequeue();
                var choices = _patternWave.Get(coord);
                var patternIds = new HashSet<int>();
                
                for (var i = 0; i < choices.Length; i++)
                {
                    if (choices[i]) patternIds.Add(i);
                }
                
                foreach (var offset in Offsets)
                {
                    var neighbor = coord + offset;
                    
                    // Only consider cells within bounds.
                    if (!_atomGrid.WithinBounds(neighbor)) continue;
                    
                    // Only consider uncollapsed cells.
                    if (_atomGrid.Get(neighbor) != _atomGrid.DefaultFillValue) continue;

                    
                    // Get union of allowed neighbors of the current cell.
                    var allowedNeighbors = new bool[_nPatterns];
                    foreach (var patternId in patternIds)
                    {
                        for (int i = 0; i < _nPatterns; i++)
                        {
                            allowedNeighbors[i] |= PatternMatrix.GetAdjacency(patternId, i, offset);
                        }
                    }
                    
                    var neighborChoices = _patternWave.Get(neighbor);

                    var post = new bool[_nPatterns];
                    var preIsPost = true;
                    var remainingChoiceCount = 0;
                    
                    for (int i = 0; i < neighborChoices.Length; i++)
                    {
                        var isPatternAllowed = allowedNeighbors[i] && neighborChoices[i];
                        post[i] = isPatternAllowed;
                        if (isPatternAllowed != neighborChoices[i])
                        {
                            preIsPost = false;
                        }

                        if (isPatternAllowed)
                        {
                            remainingChoiceCount++;
                        }
                    }

                    if (preIsPost) continue;
                    if (remainingChoiceCount == 1)
                    {
                        /*
                         * Can already collapse the neighbor if there's only one choice remaining.
                         */
                        Collapse(neighbor);
                        return;
                    }

                    if (remainingChoiceCount == 0)
                    {
                        /*
                         * Conflict...
                         */
                        throw new NoMoreChoicesException($"No more choices remain for {neighbor}");
                    }
                    
                    /*
                     * Update neighbor and propagate.
                     */
                    _patternWave.Set(neighbor, post);
                    _propagationQueue.Enqueue(neighbor);
                }
            }
        }

        private HashSet<int> GetValidAtomsInPattern(int patternId, Vector3Int offset, Vector3Int atomCoord)
        {
            /*
             * Given a context of a pattern id in the cell of question and an atom id in a neighbouring cell at the given offset,
             * find the set of atom ids that fit the context.
             */
            
            var neighborCoord = atomCoord + offset;
            
            var pattern = PatternMatrix.Patterns[patternId];
            
            /*
             * TODO: move this to precomputation.
             */
            var patternIndexMapping = new Dictionary<Vector3Int, int>();
            var patternIndexMappingInv = new Dictionary<int, List<Vector3Int>>();
            
            for (var y = 0; y < pattern.GetLength(0); y++)
            {
                for (var x = 0; x < pattern.GetLength(1); x++)
                {
                    for (var z = 0; z < pattern.GetLength(2); z++)
                    {
                        var atomId = pattern[y, x, z];
                        patternIndexMapping[new Vector3Int(x, y, z)] = atomId;
                        
                        if (patternIndexMappingInv[pattern[y, x, z]] == null)
                        {
                            patternIndexMappingInv[atomId] = new List<Vector3Int>() { new Vector3Int(x, y, z) };
                        }
                        else
                        {
                            patternIndexMappingInv[atomId].Add(new Vector3Int(x,y,z));
                        }
                    }
                }
            }

            var neighborAtomId = _atomGrid.Get(neighborCoord); 
                
            var neighborPatterns = _patternWave.Get(neighborCoord);

            var atoms = new HashSet<int>();

            // If the neighbor was not set yet, obtain the of remaining valid atom ids.
            // These mus be part of the pattern given, by construction of the pattern overlap.
            if (neighborAtomId != _atomGrid.DefaultFillValue)
            {
                atoms.Add(neighborAtomId);
            }
            else
            {
                foreach (var neighborPattern in neighborPatterns)
                {
                    // if (neighborPattern.Count > 0)
                    // {
                    //     
                    // }
                }
            }
            var validAtoms = new HashSet<int>();
            var neighborPositions = patternIndexMappingInv[neighborAtomId];
            foreach (var neighborPosition in neighborPositions)
            {
                var position = neighborPosition - offset;
                if (patternIndexMapping.ContainsKey(position))
                {
                    validAtoms.Add(pattern[position.y, position.x, position.z]);
                }
            }

            return validAtoms;


        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class BorderOutline
{
    public struct Edge
    {
        public Vector3 From;
        public Vector3 To;

        public Edge(Vector3 from, Vector3 to)
        {
            From = from;
            To = to;
        }

        public override string ToString()
        {
            return $"<{From},{To}>";
        }

        public Vector3 GetDistance()
        {
            return To - From;
        }
    }
    private Dictionary<int, HashSet<Edge>> _borderOutlines;

    public BorderOutline()
    {
        _borderOutlines = BorderOutlinePermutations();
    }
    private HashSet<Vector3Int> Nodes()
    {
        var nodes = new HashSet<Vector3Int>();
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    nodes.Add(new Vector3Int(x, y, z));
                }
            }
        }

        return nodes;
    }

    private HashSet<Edge> FullEdges()
    {
        /*
         * Returns a set of edges, where from < to, i.e. edges only in positive directions.
         */
        const int nSides = 6;
        // var edges = new Dictionary<Vector3, Vector3>(); // Origin, direction.
        var edges = new HashSet<Edge>(); // from, to.
        
        Convert.ToString(nSides, 2);

        var nodes = Nodes();
        // For each edge of the vector, save coordinate and direction. 
        foreach (Vector3Int node in nodes)
        {
            var x = node.x;
            var y = node.y;
            var z = node.z;
            var origin = new Vector3(x, y, z);
            var directions = new HashSet<Vector3>();
            if ((x & 1) == 0)
            {
                var direction = new Vector3(1, 0, 0);
                directions.Add(direction);
            }

            if ((y & 1) == 0)
            {
                var direction = new Vector3(0, 1, 0);
                directions.Add(direction);
            }

            if ((z & 1) == 0)
            {
                var direction = new Vector3(0, 0, 1);
                directions.Add(direction);
            }

            foreach (var d in directions)
            {
                edges.Add(new Edge(origin, origin + d));
            }
        }
        return edges;
    }
    private Dictionary<int, HashSet<Edge>> BorderOutlinePermutations()
    {
        const int nSides = 6;
        // side order: EWTBNS, xxyyzz, 101010
        var offsets = Convert.ToInt16("101010", 2);
        const int nPerms = 1 << nSides;
        var nodes = Nodes();
        var permutations = new Dictionary<int, HashSet<Edge>>();
        
        /*
         * Get perm:
         * shift all to left. & with 1, obtain value for dimension.
         * For all nodes with that value in dim, remove edges.
         */
        for (int perm = 0; perm < nPerms; perm++)
        {
            var edges = FullEdges();
            var s = new StringBuilder();
            foreach (var edge in edges)
            {
                s.Append(edge.ToString());
            }
            for (int shift = nSides - 1; shift >= 0; shift--)
            {
                // Obtain whether offset should be included by shifting.
                // If offset should be included, obtain the corresponding value.
                // , obtain which offset to exclude for that perm.
                int offset = perm >> shift;
                int includeOffset = offset & 1;
                int value = 0;
                
                // Debug.Log($"perm: {perm}, shift: {shift}, inc off: {includeOffset}, offset: {offset}");
                if (includeOffset == 1)
                {
                    value = (offsets >> shift) & 1;
                }
                else
                {
                    continue;
                }
                
                switch (shift)
                {
                    case >= 4:
                    {
                        // x
                        foreach (var node in nodes)
                        {
                            if (node.x != value) continue;
                            if ((node.z & 1) == 0)
                            {
                                edges.Remove(new Edge(node, node + new Vector3Int(0, 0, 1)));
                            }
                            if ((node.y & 1) == 0)
                            {
                                edges.Remove(new Edge(node, node + new Vector3Int(0, 1, 0)));
                            }
                        }

                        break;
                    }
                    case >= 2:
                    {
                        // y
                        foreach (var node in nodes)
                        {
                            if (node.y != value) continue;
                            if ((node.x & 1) == 0)
                            {
                                edges.Remove(new Edge(node, node + new Vector3Int(1, 0, 0)));
                            }
                            if ((node.z & 1) == 0)
                            {
                                edges.Remove(new Edge(node, node + new Vector3Int(0, 0, 1)));
                            }
                        }

                        break;
                    }
                    default:
                    {
                        // z
                        foreach (var node in nodes)
                        {
                            if (node.z != value) continue;
                            if ((node.x & 1) == 0)
                            {
                                edges.Remove(new Edge(node, node + new Vector3Int(1, 0, 0)));
                            }
                            if ((node.y & 1) == 0)
                            {
                                edges.Remove(new Edge(node, node + new Vector3Int(0, 1, 0)));
                            }
                        }

                        break;
                    }
                }
            }
            permutations[perm] = edges;
        }

        return permutations;
    }

    public Dictionary<Vector3Int, int> ToIntAdjacencyMask(bool[,,] mask)
    {
        /*
         * Represents each atom in the mask's adjacency as an integer. This follows the EWTBNS offset order.
         */
        var adjacencyMask = new Dictionary<Vector3Int, int>();
        adjacencyMask[new Vector3Int(0,0,0)] = 0;
        for (int i = 0; i < mask.GetLength(0); i++)
        {
            for (int j = 0; j < mask.GetLength(1); j++)
            {
                for (int k = 0; k < mask.GetLength(2); k++)
                {
                    int m = mask[i, j, k] ? 1 : 0;
                    var atomIndex = new Vector3Int(j, i, k);
                    if (!adjacencyMask.Keys.Contains(atomIndex) && m != 0) adjacencyMask[atomIndex] = 0;
                    // ignore m == 0?
                    if (i + 1 < mask.GetLength(0))
                    {
                        // Get the value of the neighbour and shift the bit to represent the offset.
                        // Only need an update for a cell if neighbour is 1.
                        int n = mask[i + 1, j, k] ? 1 : 0;
                        // TB
                        var shift = 3;
                        if (m == 1 && n == 1)
                        {
                            adjacencyMask[atomIndex] += 1 << shift;
                            var adj = new Vector3Int(j, i + 1, k);
                            if (!adjacencyMask.Keys.Contains(adj)) adjacencyMask[adj] = 0;
                            adjacencyMask[adj] += 1 << (shift - 1);
                        }
                    }

                    if (j + 1 < mask.GetLength(1))
                    {
                        int n = mask[i, j + 1, k] ? 1 : 0;
                        // EW
                        var shift = 5;
                        if (m == 1 && n == 1)
                        {
                            adjacencyMask[atomIndex] += 1 << shift;
                            var adj = new Vector3Int(j + 1, i, k);
                            if (!adjacencyMask.Keys.Contains(adj)) adjacencyMask[adj] = 0;
                            adjacencyMask[adj] += 1 << (shift - 1);
                        }
                    }

                    if (k + 1 < mask.GetLength(2))
                    {
                        int n = mask[i, j, k + 1] ? 1 : 0;
                        // NS
                        var shift = 1;
                        if (m == 1 && n == 1)
                        {
                            var adj = new Vector3Int(j, i, k + 1);
                            adjacencyMask[atomIndex] += 1 << shift;
                            if (!adjacencyMask.Keys.Contains(adj)) adjacencyMask[adj] = 0;
                            adjacencyMask[adj] += 1 << (shift - 1);
                        }
                    }
                }
            }
        }

        return adjacencyMask;
    }

    public Dictionary<Vector3Int, HashSet<Edge>> GetEdgesPerAtom(bool[,,] mask)
    {
        var intMasks = ToIntAdjacencyMask(mask);
        var atomEdges = new Dictionary<Vector3Int, HashSet<Edge>>();
        
        foreach (var (atomCoord, intMask) in intMasks)
        {
            atomEdges[atomCoord] = _borderOutlines[intMask];
        }

        return atomEdges;

    }
}
 
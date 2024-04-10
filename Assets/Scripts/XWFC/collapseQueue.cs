using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace XWFC
{
    // public class CollapseQueue : PriorityQueue<Vector3, float>
    // {
    //     public List<(Vector3, float)> Copy()
    //     {
    //         var clone = (CollapseQueue)MemberwiseClone();
    //         var copy = new List<(Vector3, float)>();
    //         while (clone.Count > 0)
    //         {
    //             clone.TryDequeue(out Vector3 val, out float priority);
    //             copy.Add((new Vector3(val.x, val.y, val.z), priority));
    //         }
    //
    //         return copy;
    //     }
    //
    //     public static CollapseQueue FromList(List<(Vector3, float)> list)
    //     {
    //         var q = new CollapseQueue();
    //         foreach (var (coord, priority) in list)
    //         {
    //             q.Enqueue(coord, priority);
    //         }
    //
    //         return q;
    //     }
    // }
    //
    // public class CollapseList : SkipList<Node<Collapse>>
    // {
    //     
    //     public Dictionary<Vector3, Node<Collapse>> EnqueuedCells = new();
    //     
    //     #nullable enable
    //     private Node<Collapse>? _root;
    //     private int _nnodes;
    //
    //     public void Insert(Vector3 coord, float entropy)
    //     {
    //         Insert(new Node<Collapse>(new Collapse(coord, entropy)));
    //     }
    //     
    //     public override void Insert(Node<Collapse> value)
    //     {
    //         _root ??= value;
    //
    //         if (EnqueuedCells.Keys.Contains(value.Value.Coord))
    //         {
    //             var entry = EnqueuedCells[value.Value.Coord];
    //             // If an entry already exists, only update it if the current value is lower.
    //             if (entry.Value > value.Value) Delete(entry); else return;
    //         }
    //         // Check # layers. Update layer for root if needed.
    //         // Go in express lane until we find otherValue > value.
    //         
    //         // If the given value is < root, said value should be the new root.
    //         if(_root.Value < value.Value)
    //         {
    //             UpdateRoot(value);
    //             return;
    //         }
    //
    //         // If the location is after root, find last encountered nodes per layer.
    //         // Find path to location for insertion.
    //         var lastNodePerLayer = LayeredPath(value);
    //         
    //         int maxLayers = _root.GetNLayers() + 1; // Do not exceed the number of layers of root by more than 1.
    //         int nLayers = 1; // At least one layer exists.
    //         
    //         // Decide number of layers for the node.
    //         var coinFlip = new Random().NextDouble();
    //         while (coinFlip >= 0.5 && nLayers < maxLayers)
    //         {
    //             coinFlip = new Random().NextDouble();
    //             nLayers++;
    //         }
    //         
    //         // If a new layer is added beyond root, update root's layers.
    //         // if (_root.GetNLayers() < nLayers) _root.UpdateLayer(nLayers, value);
    //         
    //         // If appending layers, add root and point root to added value.
    //         while (nLayers >= lastNodePerLayer.Length)
    //         {
    //             value.UpdateLayer(nLayers, null);
    //             _root.UpdateLayer(nLayers, value);
    //             nLayers--;
    //         }
    //         
    //         // Make the last encountered nodes point to the inserted value. 
    //         while (nLayers >= 0)
    //         {
    //             var current = lastNodePerLayer[nLayers];
    //             var next = current.Next(nLayers);
    //             value.UpdateLayer(nLayers, next);
    //             current.UpdateLayer(nLayers, value);
    //             nLayers--;
    //         }
    //         
    //         if (!EnqueuedCells.Keys.Contains(value.Value.Coord))
    //         {
    //             EnqueuedCells[value.Value.Coord] = value;
    //         }
    //     }
    //
    //     private Node<Collapse>[] LayeredPath(Node<Collapse> value)
    //     {
    //         /*
    //          * Find the layered path to the location where value should be.
    //          * The node in the lowest layer is the last node encountered prior to finding the location.
    //          */
    //         if (_root == null) return new Node<Collapse>[]{};
    //         
    //         int currentLayer = _root.GetNLayers(); // Start at the highest layer.
    //         var lastNodePerLayer = new Node<Collapse>[_root.GetNLayers()];
    //         Node<Collapse> currentNode = _root;
    //         
    //         while (currentLayer > 0)
    //         {
    //             // Keep track of the last valid nodes we encounter, makes updating after insert easier.
    //             lastNodePerLayer[currentLayer - 1] = currentNode;
    //             var nextNode = currentNode.Next(currentLayer);
    //             
    //             // If the current node does not point to another node, then continue.
    //             if (nextNode == null || nextNode.Value.LessThan(value.Value))
    //             {
    //                 currentLayer--;
    //                 continue;
    //             }
    //             currentNode = nextNode;
    //         }
    //
    //         return lastNodePerLayer;
    //     }
    //
    //     private void UpdateRoot(Node<Collapse> value)
    //     {
    //         /*
    //          * If the value is less than root's, value is new root. 
    //          */
    //         if (_root == null)
    //         {
    //             _root = value;
    //             return;
    //         }
    //         
    //         for (int i = 0; i < _root.GetNLayers(); i++)
    //             value.UpdateLayer(i, _root);
    //         _root = value;
    //     }
    //
    //     public override bool Delete(Node<Collapse> value)
    //     {
    //         var layeredPath = LayeredPath(value);
    //         if (layeredPath.Length < 1) return false;
    //         
    //         for (int i = 0; i < value.GetNLayers(); i++)
    //         {
    //             var node = layeredPath[i];
    //             var next = value.Next(i);
    //             if (next != null) node.UpdateLayer(i, next);
    //         }
    //         
    //         if (!EnqueuedCells.Keys.Contains(value.Value.Coord))
    //         {
    //             EnqueuedCells.Remove(value.Value.Coord);
    //         }
    //
    //         return true;
    //     }
    //
    //     public override Node<Collapse>? DeleteHead()
    //     {
    //         if (_root == null) return null;
    //         var newRoot = _root.Next(0);
    //         // Make the new root point to the old root's contents, except for the layers that the new root already covers.
    //         if (newRoot != null)
    //         {
    //             for (int i = newRoot.GetNLayers() - 1; i < _root.GetNLayers(); i++)
    //                 newRoot.UpdateLayer(i, _root.Next(i));
    //         }
    //
    //         Delete(_root);
    //         return _root;
    //     }
    //
    //     public int Count()
    //     {
    //         return EnqueuedCells.Count;
    //     }
    //
    //     // public Node<Collapse>? CopyNodes()
    //     // {
    //     //     if (_root == null) return null;
    //     //     var node = _root;
    //     //     var layers = node?.NextPerLayer;
    //     //     
    //     //     return new Node<Collapse>(node.Value.Copy(), );
    //     // }
    //
    //     // public CollapseList Copy()
    //     // {
    //     //     
    //     // }
    // }
    //
    // public record Collapse
    // {
    //     public Vector3 Coord;
    //     public float Entropy;
    //
    //     public Collapse(Vector3 coord, float entropy)
    //     {
    //         Coord = coord;
    //         Entropy = entropy;
    //     }
    //
    //     public static bool operator <(Collapse c0, Collapse c1)
    //     {
    //         return c0.Entropy < c1.Entropy;
    //     }
    //
    //     public static bool operator >(Collapse c0, Collapse c1)
    //     {
    //         return c0.Entropy > c1.Entropy;
    //     }
    //
    //     public bool LessThan(Collapse other)
    //     {
    //         return Entropy < other.Entropy;
    //     }
    //
    //     public Collapse Copy()
    //     {
    //         return new Collapse(new Vector3(Coord.x, Coord.y, Coord.z), Entropy);
    //     }
    // }
    
    public class CollapsePriorityQueue
    {
        public List<Collapse> List { get; private set; }
        private Bidict<Vector3, Collapse> _enqueuedCells;

        public CollapsePriorityQueue()
        {
            List = new List<Collapse>();
            _enqueuedCells = new Bidict<Vector3, Collapse>();
        }
        
        private CollapsePriorityQueue(List<Collapse> list)
        {
            List = list;
            _enqueuedCells = new Bidict<Vector3, Collapse>();
            foreach (var t in list)
            {
                _enqueuedCells.AddPair(t.Coord, t);
            }

        }
        
        public void Insert(Vector3 coord, float entropy) { Insert(new Collapse(coord, entropy));}
        public void Insert(Collapse collapse)
        {
            var enqueued = _enqueuedCells.Dict.Keys.Contains(collapse.Coord);
            if (enqueued)
            {
                var enqueuedCollapse = _enqueuedCells.GetValue(collapse.Coord);
                
                // No need to update if the to be inserted item is worse.
                if (collapse.Entropy > enqueuedCollapse.Entropy) return;
                List.Remove(enqueuedCollapse);
            }
            
            int i = 0;
            while (i < List.Count && List[i].Entropy <= collapse.Entropy)
            {
                i++;
            }

            if (i == List.Count)
            {
                List.Add(collapse);
            }
            else
            {
                List.Insert(i, collapse);
            }
            
            _enqueuedCells.AddPair(collapse.Coord, new Collapse(collapse.Coord, collapse.Entropy));
        }

        private int FindIndex(Collapse collapse, bool findInsertion=false)
        {
            // Divide and Conquer Log_2(n)
            var (start, end) = (0, List.Count);
            while (true)
            {
                int center = (int)(end - start * 0.5);
                var current = List[center];
                if (Math.Abs(current.Entropy - collapse.Entropy) < 0.0001)
                {
                    // found location.
                    return center;
                }

                // If divided region is one element,
                // and that element is not the element we're looking for,
                // then the location for insertion has been found.
                // If that is not what one is looking for, return -1, since the element is not in the list.
                if (end - start <= 1) return findInsertion ? -1 : start; 
                
                if (current.Entropy > collapse.Entropy)
                {
                    // Go to left.
                    end = center;
                }
                else
                {
                    // Go to right.
                    start = center;
                }
            }
        }

        public Collapse PeekHead()
        {
            return List[0];
        }

        public bool IsDone()
        {
            return List.Count == 0;
        }

        public Collapse DeleteHead()
        {
            var output = List[0];
            List.RemoveAt(0);
            return output;
        }

        public CollapsePriorityQueue Copy()
        {
            var copy = new List<Collapse>();
            foreach (var c in List) 
                copy.Add(new Collapse(new Vector3(c.Coord.x, c.Coord.y, c.Coord.z), c.Entropy));
            var output = new CollapsePriorityQueue(copy);
            return output;
        }
        
    }
    
public class CollapseList : SkipList<Node>
{
    public HashSet<Vector3> EnqueuedCells = new();
    public Dictionary<Vector3, Node> Cells = new();
    private List<Vector3> removed = new();
    
    #nullable enable
    private static readonly Vector3 DefaultCoord = new(-1, -1, -1);
    private Vector3 _root = DefaultCoord;

    public void Insert(Vector3 coord, float entropy)
    {
        Insert(new Node(new Collapse(coord, entropy)));
    }

    private bool ConditionalCellAdd(Node value)
    {
        /*
         * Cell only needs be updated if no node exists yet, or if the value's entropy is lower than the existing one.
         */
        if (!Cells.ContainsKey(value.Value.Coord) || Cells[value.Value.Coord].Value.Entropy > value.Value.Entropy)
        {
            Cells[value.Value.Coord] = value;
            return true;
        }
        return false;
    }

    public static bool IsDefaultCoord(Vector3 coord)
    {
        const float t = 0.1f;
        return Math.Abs(coord.x - DefaultCoord.x) < t && Math.Abs(coord.y - DefaultCoord.y) < t && Math.Abs(coord.z - DefaultCoord.z) < t;
    }
    /*
     * DS:
     * - cell list: vector3 to node. Stores all encountered cells, from inserting.
     * - enqueued list: vector 3. Lists ids of nodes, referencing cells. Keeps track of which cells have pending collapse.
     * IDEA:
     * In collapse: fetch and remove head from list. Remove all refs in enq and cells.
     * For each neighbour of head, enqueue them. --> need to keep track in insert function.
     *  Do not enqueue already collapsed nodes! This check is done by caller.
     */
    public override void Insert(Node value)
    {
        /*
         * Several cases for inserting value:
         * 0. If the value is already enqueued (e.g. with different entropy), remove that entry first.
         * 1. Root is null: root = value.
         *      Update root to value.
         * 2. value < root: value -> root.
         *      value is new root.
         * 3. value >= root: root -> value. value -> root.next
         *      Find path to where value should be inserted.
         */
        
        /*
         * CASE 0.
         */
        if (EnqueuedCells.Contains(value.Value.Coord)) Delete(value);
        ConditionalCellAdd(value);
        
        /*
         * CASE 1: Root == null: root = value.
         */
        if (IsDefaultCoord(_root))
        {
            _root = value.Value.Coord;
            EnqueuedCells.Add(value.Value.Coord);
            return;
        }

        /*
         * CASE 2: value <= root: value -> root.
         */
        if (value.Value <= Cells[_root].Value)
        {
            UpdateRoot(value);
            return;
        }
        
        /*
         * CASE 3: value > root.
         * A: Find path to location.
         * B: Insert value and update references.
         */
        
        /*
         * 3 A: find path.
         */
        var lastNodePerLayer = LayeredPath(value);
        // Here path cannot be null. Need check due to function.
        if (lastNodePerLayer == null) return;
        
        /*
         * 3 B: Insert value.
         */
        // Decide on the number of express layers for the new node.
        int nLayers = CalcNLayers(Cells[_root]);
        
        // If appending layers, add root and point root to added value.
        while (nLayers >= lastNodePerLayer.Length)
        {
            value.UpdateLayer(nLayers, null);
            Cells[_root].UpdateLayer(nLayers, value.Value);
            nLayers--;
            Debug.Log("HERE 86");
        }
        
        // Make the last encountered nodes point to the inserted value. 
        while (nLayers >= 0)
        {
            var current = lastNodePerLayer[nLayers];
            var next = current.Next(nLayers);
            value.UpdateLayer(nLayers, next);
            current.UpdateLayer(nLayers, value.Value);
            nLayers--;
        }

        ConditionalCellAdd(value);
        EnqueuedCells.Add(value.Value.Coord);
    }

    private static int CalcNLayers(Node node)
    {
        int maxLayers = node.GetNLayers() + 1; // Do not exceed the number of layers of root by more than 1.
        int nLayers = 1; // At least one layer exists.
        
        // Decide number of layers for the node.
        var coinFlip = new Random().NextDouble();
        while (coinFlip >= 0.5 && nLayers < maxLayers)
        {
            coinFlip = new Random().NextDouble();
            nLayers++;
        }

        return nLayers;
    }

    private Node[]? LayeredPath(Node value)
    {
        /*
         * Find the layered path to the location where the value should be.
         * The node in the lowest layer is the last node encountered prior to finding the location.
         *
         * Start at root, in the upper layer.
         * Then several cases:
         * 1. value < node:
         *      A. If node == root: value does not exist.
         *      B. Else: enter lower layer and continue.
         * 2. value == root: value has no preceding nodes
         * 3. value > node: set last node at given layer in list. 
         *      A. If node.next == null: Enter lower layer.
         *      B. If node.next != null: continue and repeat with node.next.
         */

        Node rootNode = Cells[_root];
        /*
         * CASE 1A: value < root.
         * CASE 2: value == root.
         */
        
        // If the target is root, then there are no preceding nodes.
        if (IsDefaultCoord(_root) || value.Value.Coord == _root || value.Value.Entropy < Cells[_root].Value.Entropy) return null;
        
        var lastNodePerLayer = new Node[Cells[_root].GetNLayers()];
        
        
        /*
         * Start at root, in the upper layer.
         */
        int currentLayer = rootNode.GetNLayers() - 1; 
        Node currentNode = rootNode;
        
        /*
         * CASE 3.
         */
        while (currentLayer >= 0)
        {
            // Keep track of the last valid nodes we encounter, makes updating after insert easier.
            lastNodePerLayer[currentLayer] = currentNode;
            var nextNode = currentNode.Next(currentLayer);
            
            // Continue if the next node (a) does not point to another node or (b) is still less than the target value.
            
            /*
             * CASE 1B, CASE 3A.
             */
            if (nextNode == null || nextNode.LessThanEquals(value.Value))
            {
                currentLayer--;
                continue;
            }

            /*
             * CASE 3B.
             */
            if (!Cells.ContainsKey(nextNode.Coord))
            {
                Debug.Log("FOUND ISSUE; cell not found...");
            }
            currentNode = Cells[nextNode.Coord];
        }

        return lastNodePerLayer;
    }

    private void UpdateRoot(Node value)
    {
        /*
         * If the value is less than root's, value is new root. 
         */
        if (IsDefaultCoord(_root))
        {
            Cells[_root] = value;
            _root = value.Value.Coord;
            return;
        }
        
        for (int i = 0; i < Cells[_root].GetNLayers(); i++)
            value.UpdateLayer(i, Cells[_root].Value);
        _root = value.Value.Coord;
    }

    public override bool Delete(Node value)
    {
        if (IsDefaultCoord(_root) || value.Value.Coord == Cells[_root].Value.Coord)
        {
            DeleteHead();
            return true;
        }
        // Disassociate root from (enqueued) cells.
        if (EnqueuedCells.Contains(value.Value.Coord)) 
            EnqueuedCells.Remove(value.Value.Coord);
        Cells.Remove(value.Value.Coord);
        var layeredPath = LayeredPath(value);
        removed.Add(value.Value.Coord);

        
        if (layeredPath == null) return false;
        
        for (int i = 0; i < value.GetNLayers(); i++)
        {
            var node = layeredPath[i];
            var next = value.Next(i);
            node.UpdateLayer(i, next);
        }
        return true;
    }

    public override Node DeleteHead()
    {
        var defaultNode = new Node(new Collapse(DefaultCoord, -1));
        if (IsDefaultCoord(_root)) return defaultNode;
        
        Node rootNode = Cells[_root];
        var newRootPtr = rootNode.Next(0);
        
        // Make the new root point to the old root's contents, except for the layers that the new root already covers.
        Node newRoot = new Node(new Collapse(DefaultCoord, -1));
        if (newRootPtr != null)
        {
            newRoot = Cells[newRootPtr.Coord];
            for (int i = newRoot.GetNLayers() - 1; i < rootNode.GetNLayers(); i++)
            {
                var next = rootNode.Next(i);
                if (next != newRootPtr) newRoot.UpdateLayer(i, next); // Do not allow references to self.
            }
        }

        var output = Cells[_root];

        Cells.Remove(rootNode.Value.Coord);
        EnqueuedCells.Remove(rootNode.Value.Coord);
        removed.Add(rootNode.Value.Coord);
        
        Cells[_root] = newRoot;
        _root = newRoot.Value.Coord;
        
        return output;
    }

    public int Count()
    {
        return EnqueuedCells.Count;
    }

    // public Node<Collapse>? CopyNodes()
    // {
    //     if (_root == null) return null;
    //     var node = _root;
    //     var layers = node?.NextPerLayer;
    //     
    //     return new Node<Collapse>(node.Value.Copy(), );
    // }

    // public CollapseList Copy()
    // {
    //     
    // }
    public bool IsDone()
    {
        return !EnqueuedCells.Any() && IsDefaultCoord(_root);
    }
}

public record Collapse
{
    public Vector3 Coord;
    public float Entropy;

    public Collapse(Vector3 coord, float entropy)
    {
        Coord = coord;
        Entropy = entropy;
    }

    public static bool operator <=(Collapse c0, Collapse c1)
    {
        return c0.Entropy <= c1.Entropy;
    }

    public static bool operator >=(Collapse c0, Collapse c1)
    {
        return c0.Entropy >= c1.Entropy;
    }

    public bool LessThan(Collapse other)
    {
        return Entropy < other.Entropy;
    }
    
    public bool LessThanEquals(Collapse other)
    {
        return Entropy <= other.Entropy;
    }

    public Collapse Copy()
    {
        return new Collapse(new Vector3(Coord.x, Coord.y, Coord.z), Entropy);
    }

    public override string ToString()
    {
        return $"{Coord},{Entropy}";
    }
}

    public abstract class SkipList<T>
    {

        // Keep value (coord and entropy) and index.
        // number of layers should equal log n.
        private T[]? _layers;
        private T? _root;

        public abstract void Insert(T value);
        public abstract bool Delete(T value);

        public abstract Node? DeleteHead();
    }
    
    public class Node
    {
        public Collapse Value;
    
#nullable enable
        public List<Collapse?> NextPerLayer { get; private set; }

        public Node(Collapse value, List<Collapse?>? layers = null)
        {
            Value = value;
            NextPerLayer = layers ?? new List<Collapse?> { null };;
        }

        public Collapse? Next(int layer)
        {
            return layer < GetNLayers() ? NextPerLayer[layer] : null;
        }

        public void InsertLayer(Collapse? next)
        {
            
            NextPerLayer.Add(next);
        }

        public void UpdateLayer(int layer, Collapse? next)
        {
            if (layer > GetNLayers() - 1)
            {
                while (layer > GetNLayers())
                {
                    InsertLayer(next);
                    layer--;
                }
            }
            else
            {
                NextPerLayer[layer] = next;
            }
        }

        public int GetNLayers()
        {
            return NextPerLayer.Count;
        }
    }
    //
    // public class Node<T>
    // {
    //     public T Value;
    //     
    //     #nullable enable
    //     public List<Node<T>?> NextPerLayer { get; private set; }
    //
    //     public Node(T value, List<Node<T>?>? layers = null)
    //     {
    //         Value = value;
    //         NextPerLayer = layers ?? new List<Node<T>?> { null };;
    //     }
    //
    //     public Node<T>? Next(int layer)
    //     {
    //         return layer < GetNLayers() ? NextPerLayer[layer] : null;
    //     }
    //
    //     public void InsertLayer(Node<T>? next)
    //     {
    //         NextPerLayer.Add(next);
    //     }
    //
    //     public void UpdateLayer(int layer, Node<T>? next)
    //     {
    //         if (layer > GetNLayers() - 1)
    //         {
    //             while (layer > GetNLayers())
    //             {
    //                 InsertLayer(next);
    //                 layer--;
    //             }
    //         }
    //         else
    //         {
    //             NextPerLayer[layer] = next;
    //         }
    //     }
    //
    //     public int GetNLayers()
    //     {
    //         return NextPerLayer.Count;
    //     }
    // }
}
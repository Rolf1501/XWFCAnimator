using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XWFC
{
    public class ComponentManager
    {
        public Component[] Components;
        public Dictionary<int, List<(int componentId, Range3D range)>> Intersections = new();
        private int _currentComponentId = -1;
        private Component _currentComponent;
        private int _orderIndex = -1;
        private List<int> _order;

        public ComponentManager(Component[] components)
        {
            Components = components;
            CalcIntersections();
            _order = ComponentOrder();
        }

        public int Next()
        {
            if (!HasNext()) return -1;

            _orderIndex++;
            
            // Upon selecting the next component, the current one is considered to be solved.
            _currentComponentId = _order[_orderIndex];
            
            _currentComponent = Components[_currentComponentId];
            
            return _currentComponentId;
        }

        public (Vector3Int min, Vector3Int max) BoundingBox()
        {
            var min = Components[0].Origin;
            var max = Components[0].Origin + Components[0].Extent;
            
            foreach (var component in Components)
            {
                min = Vector3Util.PairWiseMin(min, component.Origin);
                max = Vector3Util.PairWiseMax(max, component.Origin + component.Extent);
            }

            return (min, max);
        }

        private List<int> ComponentOrder()
        {
            /*
             * Compute component order with DFS.
             * Ensures acyclic graph and thus no circular dependencies.
             */
            var order = new List<int>();

            if (Components.Length == 0) return order;
            
            var pending = Enumerable.Range(0,Components.Length).ToHashSet();
            
            /*
             * Start with component with lowest origin.
             */
            var minOrigin = Components[0].Origin;
            var minId = 0;
            
            foreach (int i in pending)
            {
                var origin = Components[i].Origin;
                if (origin.magnitude < minOrigin.magnitude)
                {
                    minOrigin = origin;
                    minId = i;
                }
            }

            ComponentOrderDfs(minId, ref pending, ref order);
            return order;
        }

        private void ComponentOrderDfs(int id, ref HashSet<int> pending, ref List<int> order)
        {
            if (pending.Count == 0) return;
            
            order.Add(id);
            pending.Remove(id);
            /*
             * Find intersections
             */
            var intersections = Intersections[id];
            
            // Find all components connected to the id component.
            foreach (var (componentId, _) in intersections)
            {
                if (pending.Contains(componentId))
                {
                    ComponentOrderDfs(componentId, ref pending, ref order);
                }
            }

            // If there are disconnected components, start DFS from there.
            if (pending.Count > 0)
            {
                ComponentOrderDfs(pending.First(), ref pending, ref order);
            }
        }

        public bool HasNext()
        {
            return _orderIndex < _order.Count - 1;
        }

        public void CalcIntersections()
        {
            var intersections = new Dictionary<int, List<(int id, Range3D ranges)>>();
            for (int id = 0; id < Components.Length; id++)
            {
                intersections[id] = new List<(int id, Range3D ranges)>();
            }
            
            /*
             * For all unique component pairs, compute and store potential intersections. 
             */
            for (int i = 0; i < Components.Length; i++)
            {
                var source = Components[i];
                for (int j = i + 1; j < Components.Length; j++)
                {
                    var other = Components[j];
                    var (intersects, ranges) = ComponentIntersect(source, other);
                    
                    if (!intersects) continue;
                    
                    // Intersection is a bidirectional relation.
                    intersections[i].Add((j, ranges));
                    intersections[j].Add((i, ranges));
                }
            }

            Intersections = intersections;
        }

        private static (bool intersects, Range3D ranges) ComponentIntersect(Component source, Component other)
        {
            return Range3D.Intersect3D(source.Ranges(), other.Ranges());
        }

        public void TranslateUnsolved()
        {
            if (_currentComponentId < 0 || _currentComponentId >= Components.Length) return;
            
            var curComponent = Components[_currentComponentId];
            var intersections = Intersections[_currentComponentId];

            var solved = GetSolvedIds();

            var visited = new HashSet<int>() {_currentComponentId};
            for (int j = 0; j < intersections.Count; j++)
            {
                var (id, range) = intersections[j];
                
                // Allowing translation of the same component multiple times could result in endless loop.
                if (visited.Contains(id) || solved.Contains(id)) continue;
                
                var (offset, direction) = curComponent.CalcOffset(range);
                
                if (offset == 0) continue;
                
                // Translate the unsolved component by the offset in the given direction.
                visited = PropagateTranslation(id, direction, offset, visited, solved);
            }
            
            // Translation can result in different intersections, so recompute them.
            CalcIntersections();
        }

        private HashSet<int> GetSolvedIds()
        {
            var solved = new HashSet<int>();
            for (int i = 0; i < _orderIndex; i++)
            {
                solved.Add(_order[i]);
            }

            return solved;
        }

        public void SeedComponent(int componentId)
        {
            /*
             * To seed a component, find the solved components it intersects with.
             * Then, map the region of overlap to the local grid coordinate system.
             */
            var intersections = Intersections[componentId];
            var component = Components[componentId];
            var blockedCellId = XWFC.BlockedCellId(component.Grid.DefaultFillValue,
                component.AdjacencyMatrix.TileSet.Keys);

            var solved = GetSolvedIds();
            foreach (var (id, range) in intersections)
            {
                // Seeding is only when the component intersects with a solved component.
                if (!solved.Contains(id)) continue;
                
                // Map range to relative coordinates to reference items in grid.
                var other = Components[id];
                var startOther = range.Min() - other.Origin;

                var startThis = range.Min() - component.Origin;

                var xLength = range.GetXLength();
                var yLength = range.GetYLength();
                var zLength = range.GetZLength();

                var otherDefaultFillValue = other.Grid.DefaultFillValue;
                for (int x = 0; x < xLength; x++)
                {
                    for (int y = 0; y < yLength; y++)
                    {
                        for (int z = 0; z < zLength; z++)
                        {
                            var relativeCoord = new Vector3Int(x, y, z);
                            var otherCoord = startOther + relativeCoord;
                            var thisCoord = startThis + relativeCoord; 
                            if (other.Grid.Get(otherCoord) != otherDefaultFillValue)
                            {
                                component.Grid.Set(thisCoord, blockedCellId);
                            }
                        }
                    }
                }
            }
        }

        public HashSet<int> PropagateTranslation(int componentId, Vector3Int direction, int offset, HashSet<int> visited, HashSet<int> solved)
        {
            /*
             * Propagate translation DFS order.
             */
            Translate(componentId, direction, offset);
            visited.Add(componentId);
            
            var intersections = Intersections[componentId];
            foreach (var (i, range) in intersections)
            {
                if (visited.Contains(i) || solved.Contains(i)) continue;
                return PropagateTranslation(i, direction, offset, visited, solved);
            }

            return visited;
        }
        
        public void Translate(int componentId, Vector3Int direction, int offset)
        {
            /*
             * Given an offset and a direction, the translation should be the negated offset (hence -offset).
             */
            var translation = Vector3Util.VectorToVectorInt(Vector3Util.Scale(direction, -offset));
            Components[componentId].Origin += translation;
        }
        
    }
}
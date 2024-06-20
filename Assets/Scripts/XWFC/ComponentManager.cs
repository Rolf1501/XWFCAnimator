using System;
using System.Collections.Generic;
using System.Linq;

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

        public (int id, Component component) Next()
        {
            if (!HasNext()) return (-1, null);

            _orderIndex++;
            
            // Upon selecting the next component, the current one is considered to be solved.
            _currentComponentId = _order[_orderIndex];
            
            _currentComponent = Components[_currentComponentId];
            
            return (_currentComponentId, _currentComponent);
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
        public void SeedComponentGrid(ref Component component)
        {
            /*
             * Find other components adjacent to passed component.
             * Find number of layers required to fully seed the grid. --> same as calculation of void masks!
             * Insert layers into component grid.
             * Update component's origin and extent?
             */
            
            /*
             * Determining direction for void mask calculation: find intersecting dimension ranges.
             * To be considered intersecting, at least two dimensions should intersect and at most one should touch.
             * Choose the dimension not part of intersection.
             *
             * If the region of overlap is void, then shifting is needed.
             * How to determine shifting direction? --> dimension not involved in intersecting region.
             */
            
            /*
             * Foreach solved adjacent component, obtain void masks and then offset the component. 
             */
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
    }
}
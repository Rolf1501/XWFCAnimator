using System;
using System.Collections.Generic;
using System.Linq;

namespace XWFC
{
    public class ComponentManager
    {
        public Dictionary<int, Component> Components = new();
        public Dictionary<int, List<(int componentId, Range3D range)>> Intersections = new();
        private Queue<int> _componentOrder = new();
        private int _currentComponentId;
        private Component _currentComponent;

        public Component Next()
        {
            if (!HasNext()) return null;
            
            _currentComponentId = _componentOrder.Dequeue();
            _currentComponent = Components[_currentComponentId];
            return _currentComponent;
        }

        public bool HasNext()
        {
            return _componentOrder.Count > 0;
        }
        public void SeedComponentGrid(ref Component component)
        {
            if (!Components.ContainsValue(component)) return;
            
            /*
             * Find other components adjacent to passed component.
             * Find number of layers required to fully seed the grid. --> same as calculation of void masks!
             * Insert layers into component grid.
             * Update components' sources and extents?
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

        
        public void AddComponents(Component[] components)
        {
            var key = Components.Count;
            foreach (var component in components)
            {
                Components[key] = component;
                _componentOrder.Enqueue(key);
                key++;
            }
        }

        public void ComputeIntersections()
        {
            var intersections = new Dictionary<int, List<(int id, Range3D ranges)>>();
            foreach (var componentsKey in Components.Keys)
            {
                intersections[componentsKey] = new List<(int id, Range3D ranges)>();
            }
            
            /*
             * For all unique component pairs, compute and store potential intersections. 
             */
            for (int i = 0; i < Components.Keys.Count; i++)
            {
                var source = Components[i];
                for (int j = i + 1; j < Components.Keys.Count; j++)
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
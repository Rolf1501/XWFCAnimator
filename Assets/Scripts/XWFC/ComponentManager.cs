using System;
using System.Collections.Generic;
using System.Linq;

namespace XWFC
{
    public class ComponentManager
    {
        public Dictionary<int, Component> Components;
        public Dictionary<int, List<(int componentId, Range3D range)>> Intersections;

        public ComponentManager()
        {
            Components = new Dictionary<int, Component>();
            Intersections = new Dictionary<int, List<(int componentId, Range3D range)>>();
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
        }

        public void AddComponents(Component[] components)
        {
            var key = Components.Count;
            foreach (var component in components)
            {
                Components[key] = component;
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
             * For all unique component pairs, compute and store potential intersection. 
             */
            for (int i = 0; i < Components.Keys.Count; i++)
            {
                var source = Components[i];
                for (int j = i + 1; j < Components.Keys.Count; j++)
                {
                    var other = Components[j];
                    var (intersects, ranges) = Intersect3D(source, other);
                    
                    if (!intersects) continue;
                    
                    // Intersection is a bidirectional relation.
                    intersections[i].Add((j, ranges));
                    intersections[j].Add((i, ranges));
                }
            }

            Intersections = intersections;
        }

        private (bool intersects, Range3D ranges) Intersect3D(Component source, Component other)
        {
            /*
             * Intersection 3D makes use of Components being represented as AABBs.
             * Two AABBs are intersecting iff all their axis projections overlap.
             */
            var noIntersect = (false, new Range3D());
            var sourceRanges = source.Ranges();
            var otherRanges = other.Ranges();

            var (xIntersects, xRange) = RangeIntersection(sourceRanges.XRange, otherRanges.XRange);
            var (yIntersects, yRange) = RangeIntersection(sourceRanges.YRange, otherRanges.YRange);
            var (zIntersects, zRange) = RangeIntersection(sourceRanges.ZRange, otherRanges.ZRange);
            
            if (!(xIntersects && yIntersects && zIntersects)) return noIntersect;
            
            // If all ranges only overlap just barely, then it is not a true overlap. Corner adjacency is not considered adjacent.
            if (xRange.Start == xRange.End && yRange.Start == yRange.End && zRange.Start == zRange.End) return noIntersect;
            
            return (true, new Range3D(xRange, yRange, zRange));
        }

        private (bool intersects, Range) RangeIntersection(Range rangeSource, Range rangeOther)
        {
            // If two ranges intersect, the region of intersection is given by the maximum of the minima and the minimum of the maxima.
            return Intersect1D(rangeSource, rangeOther)
                ? (true, new Range(Math.Max(rangeSource.Start, rangeOther.Start), Math.Min(rangeSource.End, rangeOther.End)))
                : (false, new Range());
        }

        private static bool Intersect1D(Range r0, Range r1)
        {
            /*
             * Two regions intersect if there exists a number N in both ranges.
             * This means that r0_start <= N <= r0_end and r1_start <= N <= r1_end.
             * Here follows that r0_start <= r1_end and r1_start <= r0_end.
             * Note the <= sign. If two ranges touch, they should be considered intersecting.
             */
            return r0.Start <= r1.End && r1.Start <= r0.End;
        }
    }
}
using System;
using System.Collections.Generic;

namespace XWFC
{
    public class ComponentManager
    {
        public Dictionary<int, Component> Components;

        public ComponentManager()
        {
            Components = new Dictionary<int, Component>();
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

        public void GetAdjacentComponents(int componentId)
        {
            /*
             * Given a component C0, find all other components whose face(s) touch or intersect with C0.
             * To specify the location of other components relative to the source,
             * vectors are used to describe the relative direction from source to other component origin.
             */
            if (!Components.ContainsKey(componentId)) return;
            
            var sourceComponent = Components[componentId];
            foreach (var (id, component) in Components)
            {
                if (id == componentId) continue;
                // Find location relative to source.
                var translation = component.Origin - sourceComponent.Origin;
                // There's an intersection if either:
                // a) translation is negative and other origin + other extent >= source origin
                // b) translation is positive and other origin <= source origin + source extent
                
            }
        }

        private bool IntersectingDimensions(Component source, Component other)
        {
            /*
             * If, for all dimensions, a component A's dimensional ranges intersect with all dimensional ranges of B, then A and B touch.
             * Each dimensional range denotes the d_min and d_max for each x, y and z and is derived from a components source and extent.
             * This can be precomputed.
             */
            var (xRangeSource, yRangeSource, zRangeSource) = source.Ranges();
            var (xRangeOther, yRangeOther, zRangeOther) = other.Ranges();

            var (xIntersects, xRange) = RangeIntersection(xRangeSource, xRangeOther);
            var (yIntersects, yRange) = RangeIntersection(yRangeSource, yRangeOther);
            var (zIntersects, zRange) = RangeIntersection(zRangeSource, zRangeOther);
            
            if (!(xIntersects && yIntersects && zIntersects)) return false;
            return true;
        }

        private (bool intersects, Range) RangeIntersection(Range rangeSource, Range rangeOther)
        {
            // If two ranges intersect, the region of intersection is given by the maximum of the minima and the minimum of the maxima.
            return Intersects(rangeSource, rangeOther)
                ? (true, new Range(Math.Max(rangeSource.Start, rangeOther.Start), Math.Min(rangeSource.End, rangeOther.End)))
                : (false, new Range());
        }

        private bool Intersects(Range rangeSource, Range rangeOther)
        {
            /*
             * If the maximum of the range with the smallest min is equal to or larger than the largest min, then two ranges intersect.
             * Note the >= sign for the latter part. If two ranges touch, they should be considered intersecting.
             */
            return (rangeSource.Start < rangeOther.Start && rangeSource.End >= rangeOther.Start) || 
                   (rangeSource.Start >= rangeOther.Start && rangeOther.End >= rangeSource.Start);
        }
    }
}
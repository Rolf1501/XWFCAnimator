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

    }
}
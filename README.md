# Expressive Wave Function Collapse
Wave Function Collapse (WFC) is a tile-based local constraint solver commonly applied to world and map generation for grid-based content; it is able to create varied output from the same set of rules. While WFC is versatile, content generated with it is i) limited to one grid, ii) based on uniform tiles and iii) must use the same tile set and constraints throughout the grid. Due to these limitations, large classes of content are excluded, such as structured objects. Structured objects consist of an assembly of multiple components, each possibly based on a different tile set. We therefore propose Expressive Wave Function Collapse (XWFC), a major extension of WFC that enables solving and combining multiple grids with different Non-Uniform Tile (NUT) sets. Additionally, we can guarantee NUT shape and size preservation even under WFC’s Overlapping Model. With these generalizations, new domains are within reach for structured objects based on NUT sets, such as Tetris or LEGO.

My [Master's thesis](http://resolver.tudelft.nl/uuid:cc11b0d7-82b5-48e5-adc2-d5033a6ab661) discusses XWFC in greater detail.


## How to run
- Clone the repo.
- Set up Unity (XWFC is verified to work with Unity editor version  2022.3.19f1).
- Open the project in Unity.
- Add the XWFC scene to the Hierarchy.
- Run the scene.

### Interacting with the scene
The menu on the right has several options:
- Width, height, depth: size of the grid to be filled.
  - Make sure to press 'Update  Grid' after changing.
- Delay: time in seconds between frames.
- Step size: number of collapses per frame. Set to -1 to collapse all at once.
- Run: run XWFC
- Collapse once: perform one single collapse, i.e. sets the tile of one cell.
- Reset: clear the grid.
- For addition relevant options, such as setting STM or OM, select 'XWFC animator' in the hierarchy while running the scene.

The other options are less straight forward and related to creating structured components (see thesis) or running XWFC with the simple tiled model.

### Code
Point of entry is Assets/Scripts/XWFC/XWFCAnimator.cs > Start.


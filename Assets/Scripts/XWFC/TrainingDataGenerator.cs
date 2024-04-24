using System.Collections.Generic;
using UnityEngine;

namespace XWFC
{
    public class TrainingDataGenerator
    {
        public void Start()
        {
            var tileSet = new TileSet();

            var defaultWeights = new Dictionary<int, float>();
            
            var t0 = new Terminal(
                new Vector3(2,1, 2), 
                new Color(.8f, 0, .2f) ,
                null,
                null
                );
            var t1 = new Terminal(new Vector3(2, 1, 2), new Color(.2f, 0, .8f), new bool[,,]{ { {true, true}, {true, false} } }, null);
            var t2 = new Terminal(new Vector3(2,1,1), new Color(.2f, .4f, .3f), null, null);
            
            tileSet.Add(0, t0);
            tileSet.Add(1, t1);
            tileSet.Add(2, t2);
            
            var NORTH = new Vector3(0, 0, 1);
            var SOUTH = new Vector3(0, 0, -1);
            var EAST = new Vector3(1, 0, 0);
            var WEST = new Vector3(-1, 0, 0);
            var TOP = new Vector3(0, 1, 0);
            var BOTTOM = new Vector3(0, -1, 0);
            
            var adjacency = new HashSetAdjacency(){
                // 0-0
                new(0, new List<Relation>() { new(0, null) }, NORTH),
                new(0, new List<Relation>() { new(0, null) }, EAST),
                new(0, new List<Relation>() { new(0, null) }, SOUTH),
                new(0, new List<Relation>() { new(0, null) }, WEST),
                new(0, new List<Relation>() { new(0, null) }, TOP),
                new(0, new List<Relation>() { new(0, null) }, BOTTOM),
                // 1-0
                new(1, new List<Relation>() { new(0, null) }, NORTH),
                new(1, new List<Relation>() { new(0, null) }, EAST),
                new(1, new List<Relation>() { new(0, null) }, SOUTH),
                new(1, new List<Relation>() { new(0, null) }, WEST),
                new(1, new List<Relation>() { new(0, null) }, TOP),
                new(1, new List<Relation>() { new(0, null) }, BOTTOM),
                // 1-1
                new(1, new List<Relation>() { new(1, null) }, NORTH),
                new(1, new List<Relation>() { new(1, null) }, EAST),
                new(1, new List<Relation>() { new(1, null) }, SOUTH),
                new(1, new List<Relation>() { new(1, null) }, WEST),
                new(1, new List<Relation>() { new(1, null) }, TOP),
                new(1, new List<Relation>() { new(1, null) }, BOTTOM),
                // 2-0
                new(2, new List<Relation>() { new(0, null) }, NORTH),
                new(2, new List<Relation>() { new(0, null) }, EAST),
                new(2, new List<Relation>() { new(0, null) }, SOUTH),
                new(2, new List<Relation>() { new(0, null) }, WEST),
                new(2, new List<Relation>() { new(0, null) }, TOP),
                new(2, new List<Relation>() { new(0, null) }, BOTTOM),
                // 2-1
                new(2, new List<Relation>() { new(1, null) }, NORTH),
                new(2, new List<Relation>() { new(1, null) }, EAST),
                new(2, new List<Relation>() { new(1, null) }, SOUTH),
                new(2, new List<Relation>() { new(1, null) }, WEST),
                new(2, new List<Relation>() { new(1, null) }, TOP),
                new(2, new List<Relation>() { new(1, null) }, BOTTOM),
                // 2-2
                new(2, new List<Relation>() { new(2, null) }, NORTH),
                new(2, new List<Relation>() { new(2, null) }, EAST),
                new(2, new List<Relation>() { new(2, null) }, SOUTH),
                new(2, new List<Relation>() { new(2, null) }, WEST),
                new(2, new List<Relation>() { new(2, null) }, TOP),
                new(2, new List<Relation>() { new(2, null) }, BOTTOM),
            };

            var extent = new Vector3Int(3, 3, 3);
            
            var xwfc = new ExpressiveWFC(tileSet, adjacency, extent,  writeResults:true, allowBacktracking:true);
            Debug.Log("Initialized XWFC");

            xwfc.Run((int)(500000 / Vector3Util.Product(extent)));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Auxiliary : MonoBehaviour
{
    private List<int> _tiles;
    private Dictionary<int, Camera> _cameras = new();
    // Start is called before the first frame update
    void Start()
    {
        _tiles = XWFCAnimator.Instance.TileSet.Keys.ToList();
        // var camWidth = 1.0f / _tiles.Count;
        var camWidth = 1.0f;
        var camHeight = 0.2f;
        
        // for (int i = 0; i < _tiles.Count; i++)
        // {
        var obj = new GameObject();
        obj.name = $"AuxCameraContainer";
        var cameraMovement = obj.AddComponent<CameraMovement>();
        var cam = obj.AddComponent<Camera>();
        cam.name = $"AuxCamera";
        cameraMovement.SetCamera(cam);
        obj.transform.parent = gameObject.transform;
        _cameras.Add(0, cam);
        cam.rect = new Rect(camWidth, 0, camWidth, camHeight);
        // var tile = _tiles[i];
        // var obj = new GameObject();
        // obj.name = $"AuxCamera{tile}Container";
        // var cameraMovement = obj.AddComponent<CameraMovement>();
        // var cam = obj.AddComponent<Camera>();
        // cam.name = $"AuxCamera{tile}";
        // cameraMovement.SetCamera(cam);
        // obj.transform.parent = gameObject.transform;
        // _cameras.Add(tile, cam);
        // cam.rect = new Rect(camWidth, 0, camWidth, camHeight);
        // cam.rect = new Rect(camWidth * i, 0, camWidth, camHeight);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

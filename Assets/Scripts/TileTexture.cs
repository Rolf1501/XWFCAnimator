using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using XWFC;

public class TileTexture
{
    private Camera _camera;
    private Vector3 _basePos;
    public RenderTexture RenderTexture { get; }

    public TileTexture(Vector3 basePos, float offset, Vector2 angle)
    {
        _basePos = basePos;
        var obj = new GameObject();
        obj.name = $"TileTexture{basePos}";

        var cam = obj.AddComponent<Camera>();
        _camera = cam;
        _camera.nearClipPlane = 0.01f;
        _camera.farClipPlane = 20;
        _camera.transform.position = basePos;
        _camera.transform.Rotate(Vector3.right, angle.x);
        _camera.transform.Rotate(Vector3.up, angle.y, Space.World);
        _camera.transform.Translate(new Vector3(0,0,-offset));
        obj.transform.position = _camera.transform.position;
        
        RenderTexture = new RenderTexture(128,128,16,RenderTextureFormat.ARGB32);
        RenderTexture.name = "RenderTexture1";
        _camera.targetTexture = RenderTexture;
    }
}

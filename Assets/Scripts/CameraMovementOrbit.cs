using System;
using UnityEngine;

public class CameraMovementOrbit : MonoBehaviour
{
    [SerializeField] private Camera Camera;
    private bool _mouseLocked;

    public Vector3 orbitOrigin;
    private Vector3 _gridCenter;
    private float _distanceFromOrigin = 10;
    private bool _manualOrigin;
    

    private void Update()
    {
        MouseInput();
        if (!_gridCenter.Equals(XWFCAnimator.Instance.GetGridCenter()))
        {
            _gridCenter = XWFCAnimator.Instance.GetGridCenter();
            orbitOrigin = _gridCenter;
        }
        if (_manualOrigin) return;
        // var gridCenter = XWFCAnimator.Instance.GetGridCenter();
        // if (orbitOrigin.Equals(gridCenter)) return;
        // orbitOrigin = gridCenter;
        Camera.transform.position = orbitOrigin;
        TranslateCameraViewDistance();
    }

    private void MouseInput()
    {
        if (Input.GetMouseButton(0)) PanCamera(); // Left.
        if (Input.GetMouseButton(1) && CameraMayMove()) OrbitCamera(CalcDeltaPos()); // Right.
        else if (Input.mouseScrollDelta.y != 0 && CameraMayMove()) ZoomCamera(Input.mouseScrollDelta.y);
    }

    private void PanCamera()
    {
        var delta = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0);
        var t = Camera.transform;
        orbitOrigin += delta.x * t.right + delta.y * t.up;
        Camera.transform.position = orbitOrigin;

    }

    private void OrbitCamera(Vector2 angle)
    {
        /*
         * Rotate around object:
         * translate to orbit origin.
         * rotate camera.
         * offset: translate for distance, i.e. -dist.
         * offset depends on grid. can zoom automatically, depending on the grid size. Or just use scroll wheel. 
         */
        Camera.transform.position = orbitOrigin;
        
        Camera.transform.Rotate(Vector3.right, angle.x);
        Camera.transform.Rotate(Vector3.up, angle.y, Space.World);
        TranslateCameraViewDistance();
    }

    private void TranslateCameraViewDistance()
    {
        Camera.transform.Translate(new Vector3(0,0,-_distanceFromOrigin));
    }

    private bool CameraMayMove()
    {
        return MouseInViewport();
    }

    private bool MouseInViewport()
    {
        var (mX, mY) = (Input.mousePosition.x, Input.mousePosition.y);
        var x = Camera.rect.x;
        var y = Camera.rect.y;
        var width = Camera.rect.width;
        var height = Camera.rect.height;

        var sWidth = Screen.width;
        var sHeight = Screen.height;

        if (!InRange(mX, x * sWidth, width * sWidth)) return false;
        if (!InRange(mY, y * sHeight, height * sHeight)) return false;
        return true;
    }

    private static bool InRange(float value, float start, float length)
    {
        return value >= start && value <= start + length;
    }

    private void ZoomCamera(float delta)
    {
        var t = Camera.transform;
        var pos = t.position;
        pos += delta * t.forward;
        t.position = pos;
        _distanceFromOrigin = (pos - orbitOrigin).magnitude;
    }

    private void PanCameraX()
    {
        var t = Camera.transform;
        orbitOrigin += 0.05f * t.right;
    }

    private Vector3 CalcDeltaPos()
    {
        return new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
    }
}
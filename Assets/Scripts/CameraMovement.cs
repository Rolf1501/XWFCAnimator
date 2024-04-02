using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Camera Camera;
    [SerializeField] private float PanSpeed;
    [SerializeField] private float Tolerance = 0.001f;
    private bool _mouseLocked;
    private Vector3 PrevPos;

    private void Update()
    {
        MouseInput();
    }

    private void MouseInput()
    {
        // if (EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.GetMouseButton(0)) return; // Left.
        if (Input.GetMouseButton(1) && CameraMayMove()) MouseRightClick(); // Right.
        else if (Input.GetMouseButton(2) && CameraMayMove()) MouseMiddleClick(); // Middle.
        else if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) UnlockMouse();
        else if (Input.mouseScrollDelta.y != 0) ZoomCamera(Input.mouseScrollDelta.y);
    }

    private bool CameraMayMove()
    {
        return MouseInViewport() || _mouseLocked;
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

    private bool InRange(float value, float start, float length)
    {
        return value >= start && value <= start + length;
    }

    private void MouseMiddleClick()
    {
        LockMouse();
        Vector3 deltaPos = GetDeltaMousePos();
        PanCamera(deltaPos);
        PrevPos = Input.mousePosition;
    }

    private void PanCamera(Vector3 deltaPos)
    {
        /*
         * Pan Camera
         */
        var camPos = Camera.transform.position;
        var (signX, signY) = CalcSigns(deltaPos);
        
        if (Math.Abs(deltaPos.x) > Tolerance) camPos += Camera.transform.right * (signX * PanSpeed);
        if (Math.Abs(deltaPos.y) > Tolerance) camPos += Camera.transform.up * (signY * PanSpeed);
        Camera.transform.position = camPos;
    }

    private void ZoomCamera(float delta)
    {
        var camPos = Camera.transform.position;
        camPos += delta * Camera.transform.forward;
        Camera.transform.position = camPos;
    }

    private void RotateCamera(Vector3 deltaPos)
    {
        Camera.transform.eulerAngles += deltaPos;
    }

    private Vector3 GetDeltaMousePos()
    {
        var mousePos = Input.mousePosition;
        return PrevPos - mousePos;
    }

    private static (int signX, int signY) CalcSigns(Vector3 deltaPos)
    {
        int signX = deltaPos.x > 0 ? 1 : -1;
        int signY = deltaPos.y > 0 ? 1 : -1;
        return (signX, signY);
    }

    private void MouseRightClick()
    {
        LockMouse();
        var deltaPos = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        RotateCamera(deltaPos);
        PrevPos = Input.mousePosition;
    }

    private void LockMouse()
    {
        if (_mouseLocked) return;
        _mouseLocked = true;
    }

    private void UnlockMouse()
    {
        if (!_mouseLocked) return;
        _mouseLocked = false;
    }

    public void SetCamera(Camera cam)
    {
        Camera = cam;
    }
}
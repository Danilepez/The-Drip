using UnityEngine;

public class CameraWallClip : MonoBehaviour
{
    private float nearClipPlane = 0.01f;

    private void Awake()
    {
        Camera cam = GetComponentInChildren<Camera>();

        if (cam != null)
        {
            cam.nearClipPlane = nearClipPlane;
        }
    }
}

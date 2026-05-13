using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SeatedXRDriverAnchor : MonoBehaviour
{
    [Header("Required")]
    public Transform driverEyeAnchor;
    public Transform xrOriginRoot;
    public Camera xrCamera;

    [Header("Alignment")]
    public bool alignOnStart = true;
    public bool keepCameraOnAnchor = true;
    public bool alignYawToAnchor = true;
    public bool forceScaleOne = true;
    public KeyCode recenterKey = KeyCode.C;

    void Reset()
    {
        xrOriginRoot = transform;
        xrCamera = GetComponentInChildren<Camera>();
    }

    IEnumerator Start()
    {
        if (xrOriginRoot == null)
            xrOriginRoot = transform;

        if (xrCamera == null)
            xrCamera = GetComponentInChildren<Camera>();

        if (forceScaleOne && xrOriginRoot != null)
            xrOriginRoot.localScale = Vector3.one;

        if (alignOnStart)
        {
            yield return null;
            AlignNow();
        }
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(recenterKey))
            AlignNow();

        if (keepCameraOnAnchor)
            AlignNow();
    }

    public void AlignNow()
    {
        if (driverEyeAnchor == null || xrOriginRoot == null || xrCamera == null)
            return;

        if (alignYawToAnchor)
        {
            Vector3 euler = xrOriginRoot.eulerAngles;
            euler.y = driverEyeAnchor.eulerAngles.y;
            xrOriginRoot.eulerAngles = euler;
        }

        Vector3 correction = driverEyeAnchor.position - xrCamera.transform.position;
        xrOriginRoot.position += correction;
    }
}

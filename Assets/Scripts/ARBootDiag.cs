using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARBootDiag : MonoBehaviour
{
    IEnumerator Start()
    {
        Debug.Log($"[ARBootDiag] Unity {Application.unityVersion}");
        Debug.Log("[ARBootDiag] Checking AR availability...");

        yield return ARSession.CheckAvailability();
        Debug.Log("[ARBootDiag] ARSession.state = " + ARSession.state);

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            Debug.Log("[ARBootDiag] Requesting install of AR services...");
            yield return ARSession.Install();
            Debug.Log("[ARBootDiag] After install, ARSession.state = " + ARSession.state);
        }

        // List available XR subsystem descriptors in a version-agnostic way
        var camDescs   = new List<XRCameraSubsystemDescriptor>();
        var planeDescs = new List<XRPlaneSubsystemDescriptor>();
        var rayDescs   = new List<XRRaycastSubsystemDescriptor>();

        SubsystemManager.GetSubsystemDescriptors(camDescs);
        SubsystemManager.GetSubsystemDescriptors(planeDescs);
        SubsystemManager.GetSubsystemDescriptors(rayDescs);

        Debug.Log($"[ARBootDiag] Camera descriptors:  {camDescs.Count}");
        Debug.Log($"[ARBootDiag] Plane descriptors:   {planeDescs.Count}");
        Debug.Log($"[ARBootDiag] Raycast descriptors: {rayDescs.Count}");

        // Optional: try to start an ARSession if not already running
        var arSession = FindObjectOfType<ARSession>();
        if (arSession != null && ARSession.state == ARSessionState.Ready)
        {
            Debug.Log("[ARBootDiag] Starting AR session...");
            arSession.enabled = true;
        }

        // Helpful hints based on state
        switch (ARSession.state)
        {
            case ARSessionState.Unsupported:
                Debug.LogError("[ARBootDiag] Device not supported or Google Play Services for AR missing/outdated.");
                break;
            case ARSessionState.None:
            case ARSessionState.CheckingAvailability:
                Debug.LogWarning("[ARBootDiag] AR not initialized yet (try again after a moment).");
                break;
            default:
                Debug.Log("[ARBootDiag] AR should be ready. If camera is black, check URP background or Graphics API.");
                break;
        }
    }
}

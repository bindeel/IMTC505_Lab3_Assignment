// TimerPlacer_NewInput.cs
// Attach to XR Origin (same GO that has ARRaycastManager + ARAnchorManager).
// NEW INPUT SYSTEM ONLY (uses EnhancedTouch). No Reticle required.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// New Input System (Enhanced Touch)
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TimerPlacer : MonoBehaviour
{
    [Header("Refs")]
    public Camera arCamera;                  // Assign your AR Camera (tag MainCamera)
    public ARRaycastManager raycaster;       // Auto-filled from this GO if null
    public ARAnchorManager anchorManager;    // Auto-filled from this GO if null

    [Tooltip("Timer prefab with TimerSimple + Billboard + ButtonAction colliders")]
    public GameObject timerPrefab;

    [Header("Placement")]
    [Tooltip("Meters above the plane to avoid z-fighting")]
    public float heightOffset = 0.08f;

    [Header("Initial Time")]
    public int initialMinutes = 3;
    public int initialSeconds = 0;

    // Internal
    static readonly List<ARRaycastHit> s_Hits = new();

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
    }

    void OnDisable()
    {
        TouchSimulation.Disable();
        EnhancedTouchSupport.Disable();
    }

    void Awake()
    {
        if (!arCamera)      arCamera      = Camera.main;
        if (!raycaster)     raycaster     = GetComponent<ARRaycastManager>();
        if (!anchorManager) anchorManager = GetComponent<ARAnchorManager>();
    }

    void Update()
    {
        foreach (var t in Touch.activeTouches)
        {
            if (t.phase != UnityEngine.InputSystem.TouchPhase.Began)
                continue;

            // 1) If tap is on an existing timer/card/button, DO NOT place a new timer
            if (TapHitsExistingTimer(t.screenPosition))
                continue;

            // 2) AR raycast from touch to plane
            if (!raycaster || !raycaster.Raycast(t.screenPosition, s_Hits, TrackableType.PlaneWithinPolygon))
                continue;

            var hit     = s_Hits[0];
            var pose    = hit.pose;
            var plane   = hit.trackable as ARPlane;

            // 3) Create an anchor (attach to plane if possible)
            var anchor = CreateAnchor(pose, plane);
            if (!anchor) continue;

            // 4) Spawn timer slightly above plane, facing camera
            var pos = anchor.transform.position + Vector3.up * heightOffset;
            var rot = Quaternion.LookRotation(arCamera.transform.forward, Vector3.up);

            var go = Instantiate(timerPrefab, pos, rot, anchor.transform);
            go.name = "Timer (spawned)";

            // 5) Optional: initialize a starting time
            var timer = go.GetComponent<TimerSimple>();
            if (timer)
                timer.SetTimeMMSS(initialMinutes, initialSeconds);

            // One placement per tap
            break;
        }
    }

    bool TapHitsExistingTimer(Vector2 screenPos)
    {
        if (!arCamera) return false;

        var ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, 15f))
        {
            // If the hit object (or any parent) belongs to a Timer, ignore this tap for placement
            if (hit.transform.GetComponentInParent<TimerSimple>() != null)
                return true;
        }
        return false;
    }

    ARAnchor CreateAnchor(Pose pose, ARPlane planeIfAny)
    {
        // Prefer attaching to the hit plane (more stable over time)
        if (planeIfAny && anchorManager)
        {
            var a = anchorManager.AttachAnchor(planeIfAny, pose);
            if (a) return a;
        }

        // Fallback: world-space anchor at pose
        var go = new GameObject("WorldAnchor");
        go.transform.SetPositionAndRotation(pose.position, pose.rotation);
        return go.AddComponent<ARAnchor>();
    }
}

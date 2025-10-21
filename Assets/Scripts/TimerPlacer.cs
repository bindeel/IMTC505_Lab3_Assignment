// TimerPlacer_Input.cs
// Place this on your XR Origin (same GO as ARRaycastManager + ARAnchorManager).
// NEW INPUT SYSTEM ONLY: uses EnhancedTouch (no legacy Input).

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
    public Camera arCamera;                  // assign your AR Camera (tag MainCamera)
    public ARRaycastManager raycaster;       // (auto-fills from same GO if left null)
    public ARAnchorManager anchorManager;    // (auto-fills from same GO if left null)

    [Tooltip("Timer prefab with TimerSimple + Billboard, etc.")]
    public GameObject timerPrefab;           // assign your Timer prefab

    [Header("Optional Reticle")]
    [Tooltip("If assigned, will try using the reticle pose first.")]
    public ReticleController reticleController;
    public bool useReticleFirst = true;

    [Header("Placement")]
    [Tooltip("Meters above the plane so the card never z-fights.")]
    public float heightOffset = 0.08f;

    [Header("Initial Time (optional)")]
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
        // Handle first touch began this frame
        foreach (var t in Touch.activeTouches)
        {
            if (t.phase != UnityEngine.InputSystem.TouchPhase.Began) continue;

            // 1) If tap hits an existing timer (its card or buttons), DO NOT place a new timer
            if (TapHitsExistingTimer(t.screenPosition)) continue;

            // 2) Get a placement pose
            Pose pose;
            ARPlane hitPlane = null;

            if (useReticleFirst && reticleController && reticleController.TryGetPose(out pose))
            {
                // Using reticle pose (may not know which plane; that's okay)
            }
            else
            {
                // Raycast directly from the touch point to a plane
                if (!raycaster || !raycaster.Raycast(t.screenPosition, s_Hits, TrackableType.PlaneWithinPolygon))
                    continue;

                var hit = s_Hits[0];
                pose = hit.pose;
                hitPlane = hit.trackable as ARPlane;
            }

            // 3) Create an anchor (prefer attaching to the plane if we have it)
            var anchor = CreateAnchor(pose, hitPlane);
            if (!anchor) continue;

            // 4) Spawn the timer, lifted slightly, facing the camera
            var pos = anchor.transform.position + Vector3.up * heightOffset;
            var rot = Quaternion.LookRotation(arCamera.transform.forward, Vector3.up);

            var go = Instantiate(timerPrefab, pos, rot, anchor.transform);
            go.name = "Timer (spawned)";

            // 5) Optionally set a starting time so you can see it count
            var timer = go.GetComponent<TimerSimple>();
            if (timer)
                timer.SetTimeMMSS(initialMinutes, initialSeconds);

            // Only place ONE per tap
            break;
        }
    }

    bool TapHitsExistingTimer(Vector2 screenPos)
    {
        if (!arCamera) return false;

        var ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, 10f))
        {
            var timer = hit.transform.GetComponentInParent<TimerSimple>();
            if (timer != null) return true; // tap was on a timer/card/button
        }
        return false;
    }

    ARAnchor CreateAnchor(Pose pose, ARPlane planeIfAny)
    {
        // Prefer attaching to the hit plane if provided (more stable)
        if (planeIfAny && anchorManager)
        {
            var a = anchorManager.AttachAnchor(planeIfAny, pose);
            if (a) return a;
        }

        // Fallback: world-space anchor at the pose
        var go = new GameObject("WorldAnchor");
        go.transform.SetPositionAndRotation(pose.position, pose.rotation);
        return go.AddComponent<ARAnchor>();
    }
}

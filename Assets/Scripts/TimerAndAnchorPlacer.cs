using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System.Collections.Generic;

public class TimerAndAnchorPlacer : MonoBehaviour
{
    public Camera arCamera;
    public GameObject timerTagPrefab;        // assign
    public GameObject cookwareAnchorPrefab;  // assign (Tag=Cookware, BoxCollider)
    public float anchorSnapRadius = 0.25f;   // 25 cm
    public float timerHeightOffset = 0.04f;  // 4 cm

    ARRaycastManager raycaster;
    ARAnchorManager anchorManager;
    static readonly List<ARRaycastHit> hits = new();

    void OnEnable()  { EnhancedTouchSupport.Enable(); TouchSimulation.Enable(); }
    void OnDisable() { TouchSimulation.Disable(); EnhancedTouchSupport.Disable(); }

    void Awake()
    {
        raycaster     = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        if (!arCamera) arCamera = Camera.main;
        Debug.Log($"[Placer] raycaster={(raycaster?"ok":"NULL")} anchorMgr={(anchorManager?"ok":"NULL")} cam={(arCamera?"ok":"NULL")}");
    }

    void Update()
    {
        foreach (var t in Touch.activeTouches)
        {
            if (t.phase != UnityEngine.InputSystem.TouchPhase.Began) continue;
            var pos = t.screenPosition;
            Debug.Log($"[Placer] Tap {pos}");

            // If we hit an existing Cookware collider, attach timer there.
            var ray = arCamera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out var physHit, 10f) && physHit.collider.CompareTag("Cookware"))
            {
                AttachTimer(physHit.collider.transform);
                continue;
            }

            // Otherwise, use AR plane hit.
            if (!raycaster.Raycast(pos, hits, TrackableType.PlaneWithinPolygon))
            {
                Debug.Log("[Placer] AR hit: none");
                continue;
            }

            var pose = hits[0].pose;

            // Snap to nearby cookware if present.
            var nearest = FindNearestCookware(pose.position, anchorSnapRadius);
            if (nearest != null) { AttachTimer(nearest); continue; }

            // Create an ARAnchor at the plane pose, then spawn a cookware anchor under it.
            var anchor = CreateAnchorAtHit(hits[0]);
            if (!anchor) { Debug.LogError("[Placer] Failed to create anchor"); continue; }

            var cookware = Instantiate(cookwareAnchorPrefab, pose.position, pose.rotation, anchor.transform);
            cookware.name = "CookwareAnchor (spawned)";
            AttachTimer(cookware.transform);
        }
    }

    Transform FindNearestCookware(Vector3 pos, float maxMeters)
    {
        Transform best = null; float bestDist = maxMeters;
        foreach (var go in GameObject.FindGameObjectsWithTag("Cookware"))
        {
            float d = Vector3.Distance(pos, go.transform.position);
            if (d < bestDist) { best = go.transform; bestDist = d; }
        }
        return best;
    }

    ARAnchor CreateAnchorAtHit(ARRaycastHit hit)
    {
        if (!anchorManager) return null;
        var plane = hit.trackable as ARPlane;
        if (plane)
        {
            var a = anchorManager.AttachAnchor(plane, hit.pose);
            if (a) { Debug.Log("[Placer] AttachAnchor OK"); return a; }
        }
        // Fallback anchor
        var go = new GameObject("WorldAnchor");
        go.transform.SetPositionAndRotation(hit.pose.position, hit.pose.rotation);
        return go.AddComponent<ARAnchor>();
    }

    void AttachTimer(Transform anchor)
    {
        if (!timerTagPrefab) { Debug.LogError("[Placer] timerTagPrefab not assigned"); return; }
        var pos = anchor.position + Vector3.up * timerHeightOffset;
        var rot = Quaternion.LookRotation(arCamera.transform.forward, Vector3.up);

        var tag = Instantiate(timerTagPrefab, pos, rot, anchor);
        tag.name = "TimerTag (spawned)";

        var logic = tag.GetComponent<TimerTag>();
        if (logic) { logic.SetLabel("Timer"); logic.SetTime(8, 0); logic.StartTimer(); Debug.Log("[Placer] Timer started"); }
        else       { Debug.LogWarning("[Placer] Spawned timer missing TimerTag component"); }
    }
}

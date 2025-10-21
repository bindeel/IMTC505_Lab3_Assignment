// ARTapToPlaceTimer_OnlyOnCookware.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceTimer_OnlyOnCookware : MonoBehaviour
{
    public Camera arCamera;
    public GameObject timerTagPrefab;
    public float heightOffset = 0.04f; // lift timer above the pot

    ARRaycastManager raycaster;
    static List<ARRaycastHit> hits = new();

    void Awake()
    {
        raycaster = GetComponent<ARRaycastManager>();
        if (!arCamera) arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0) return;
        var t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        // 1) Physics ray to find a Cookware collider
        Ray r = arCamera.ScreenPointToRay(t.position);
        if (Physics.Raycast(r, out RaycastHit hit, 5f))
        {
            if (hit.collider && hit.collider.CompareTag("Cookware"))
            {
                AttachTimerToAnchor(hit.collider.transform);
                return;
            }
        }

        // 2) Optional: snap to nearest cookware to plane hit
        if (raycaster.Raycast(t.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Transform nearest = FindNearestCookware(hits[0].pose.position, 0.25f);
            if (nearest) { AttachTimerToAnchor(nearest); return; }
        }

        // 3) Otherwise ignore or prompt user
        Debug.Log("Tap a cookware anchor to place a timer.");
    }

    Transform FindNearestCookware(Vector3 pos, float maxMeters)
    {
        Transform best = null; float bestDist = maxMeters;
        foreach (var go in GameObject.FindGameObjectsWithTag("Cookware"))
        {
            float d = Vector3.Distance(pos, go.transform.position);
            if (d < bestDist) { bestDist = d; best = go.transform; }
        }
        return best;
    }

    void AttachTimerToAnchor(Transform anchor)
    {
        Vector3 pos = anchor.position + Vector3.up * heightOffset;
        Quaternion rot = Quaternion.LookRotation(arCamera.transform.forward, Vector3.up);
        var tag = Instantiate(timerTagPrefab, pos, rot, anchor);
        var logic = tag.GetComponent<TimerTag>();
        if (logic) { logic.SetLabel("Timer"); logic.SetTime(8, 0); logic.StartTimer(); }
    }
}

// PlaceCookwareAnchor.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceCookwareAnchor : MonoBehaviour
{
    public Camera arCamera;
    public GameObject cookwareAnchorPrefab; // assign in Inspector
    public bool placementMode = true;       // toggle via UI button

    ARRaycastManager raycaster;
    ARAnchorManager anchorManager;
    static List<ARRaycastHit> hits = new();

    void Awake()
    {
        raycaster = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        if (!arCamera) arCamera = Camera.main;
    }

    void Update()
    {
        if (!placementMode || Input.touchCount == 0) return;
        var t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        // Raycast to plane
        if (!raycaster.Raycast(t.position, hits, TrackableType.PlaneWithinPolygon)) return;
        var hit = hits[0];

        // Attach ARAnchor to the hit plane
        var plane = hit.trackable as ARPlane;
        var anchor = anchorManager.AttachAnchor(plane, hit.pose);
        if (anchor == null) return;

        // Spawn the anchor visual as a child (keeps collider/tag with the anchor)
        Instantiate(cookwareAnchorPrefab, anchor.transform.position, anchor.transform.rotation, anchor.transform);
    }
}

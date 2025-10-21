// Put on XR Origin temporarily if needed
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class OneTapCubePlacer : MonoBehaviour
{
    public Camera arCamera;
    ARRaycastManager raycaster;
    ARAnchorManager anchorManager;
    static readonly List<ARRaycastHit> hits = new();

    void Awake()
    {
        raycaster = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        if (!arCamera) arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0) return;
        var t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        if (!raycaster.Raycast(t.position, hits, TrackableType.PlaneWithinPolygon)) return;
        var hit = hits[0];
        var plane = hit.trackable as ARPlane;
        var anchor = anchorManager.AttachAnchor(plane, hit.pose);
        if (!anchor) return;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(anchor.transform, false);
        cube.transform.localPosition = new Vector3(0, 0.05f, 0);
        cube.transform.localScale = Vector3.one * 0.06f;
        cube.GetComponent<Renderer>().material.color = Color.magenta;
        Debug.Log("[CubePlacer] Spawned cube");
    }
}
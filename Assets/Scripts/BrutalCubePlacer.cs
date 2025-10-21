using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System.Collections.Generic;

public class BrutalCubePlacer : MonoBehaviour
{
    public Camera arCamera;
    ARRaycastManager raycaster;
    static readonly List<ARRaycastHit> hits = new();

    void OnEnable()  { EnhancedTouchSupport.Enable(); TouchSimulation.Enable(); }
    void OnDisable() { TouchSimulation.Disable(); EnhancedTouchSupport.Disable(); }

    void Awake()
    {
        raycaster = GetComponent<ARRaycastManager>();
        if (!arCamera) arCamera = Camera.main;
        Debug.Log($"[Brutal] Awake raycaster={(raycaster?"ok":"NULL")} cam={(arCamera?"ok":"NULL")}");
    }

    void Update()
    {
        foreach (var t in Touch.activeTouches)
        {
            if (t.phase != UnityEngine.InputSystem.TouchPhase.Began) continue;

            var pos = t.screenPosition;
            if (!raycaster.Raycast(pos, hits, TrackableType.PlaneWithinPolygon))
            {
                Debug.Log("[Brutal] AR hit: none");
                continue;
            }

            var pose = hits[0].pose;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position   = pose.position + Vector3.up * 0.05f;
            cube.transform.rotation   = Quaternion.identity;
            cube.transform.localScale = Vector3.one * 0.06f;

            var r = cube.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Unlit/Color"));
            r.material.color = Color.magenta;

            Debug.Log($"[Brutal] Spawned cube at {cube.transform.position}");
        }
    }
}
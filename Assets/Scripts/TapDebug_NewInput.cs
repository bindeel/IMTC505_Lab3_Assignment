// TapDebug_NewInput.cs
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch; // New Input System
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System.Collections.Generic;

public class TapDebug_NewInput : MonoBehaviour
{
    public Camera arCamera;
    ARRaycastManager raycaster;
    static List<ARRaycastHit> hits = new();

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }
    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Awake()
    {
        raycaster = GetComponent<ARRaycastManager>();
        if (!arCamera) arCamera = Camera.main;
    }

    void Update()
    {
        if (Touch.activeTouches.Count == 0) return;

        foreach (var t in Touch.activeTouches)
        {
            if (t.phase != UnityEngine.InputSystem.TouchPhase.Began) continue;

            Vector2 pos = t.screenPosition;

            // Physics ray (Cookware?)
            Ray r = arCamera.ScreenPointToRay(pos);
            if (Physics.Raycast(r, out RaycastHit hit, 10f))
                Debug.Log($"[TapDebug] Physics hit: {hit.collider.name}, tag={hit.collider.tag}");
            else
                Debug.Log("[TapDebug] Physics hit: none");

            // AR plane raycast
            if (raycaster.Raycast(pos, hits, TrackableType.PlaneWithinPolygon))
                Debug.Log($"[TapDebug] AR hit pose at {hits[0].pose.position}");
            else
                Debug.Log("[TapDebug] AR hit: none");
        }
    }
}
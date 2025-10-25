using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlaceOnPlane : MonoBehaviour
{
    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] GameObject prefab;

    static List<ARRaycastHit> hits = new();
    GameObject placed;

    void Update()
    {
        if (Input.touchCount == 0) return;
        Touch t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        if (raycastManager.Raycast(t.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            if (placed == null)
                placed = Instantiate(prefab, pose.position, pose.rotation);
            else
                placed.transform.position = pose.position;
        }
    }
}
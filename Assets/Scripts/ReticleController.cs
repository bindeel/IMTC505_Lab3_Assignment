using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ReticleController : MonoBehaviour
{
    [Header("Refs")]
    public Camera arCamera;
    public ARRaycastManager raycaster;
    public GameObject reticle;   // assign your Reticle prefab instance

    [Header("Behavior")]
    public bool useCenterScreen = true; // false = use first touch position when touching
    public bool requireNearCookware = false;
    public float cookwareRadiusMeters = 0.35f; // show only if near cookware

    static readonly List<ARRaycastHit> hits = new();
    Pose lastPose;
    bool hasPose;

    void Awake()
    {
        if (!arCamera) arCamera = Camera.main;
        if (!raycaster) raycaster = GetComponent<ARRaycastManager>();
        if (reticle) reticle.SetActive(false);
    }

    void Update()
    {
        if (!raycaster || !reticle) return;

        // Choose screen position: center or first touch
        Vector2 screenPos;
        if (useCenterScreen || Input.touchCount == 0)
            screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        else
            screenPos = Input.GetTouch(0).position; // (New Input System version: swap if needed)

        hasPose = raycaster.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon);
        if (!hasPose)
        {
            reticle.SetActive(false);
            return;
        }

        var pose = hits[0].pose;

        // Optional: only show reticle if near a cookware anchor
        if (requireNearCookware && !IsNearCookware(pose.position, cookwareRadiusMeters))
        {
            reticle.SetActive(false);
            return;
        }

        lastPose = pose;
        reticle.transform.SetPositionAndRotation(
            pose.position + Vector3.up * 0.01f, // tiny lift to avoid z-fighting
            Quaternion.LookRotation(arCamera.transform.forward, Vector3.up)
        );
        reticle.SetActive(true);
    }

    bool IsNearCookware(Vector3 pos, float radius)
    {
        var pots = GameObject.FindGameObjectsWithTag("Cookware");
        foreach (var p in pots)
            if (Vector3.Distance(pos, p.transform.position) <= radius) return true;
        return false;
    }

    public bool TryGetPose(out Pose pose)
    {
        pose = lastPose;
        return hasPose && (!requireNearCookware || IsNearCookware(lastPose.position, cookwareRadiusMeters));
    }
}


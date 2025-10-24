using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Tap-to-place & tap-to-move behavior for ARFoundation.
/// - On the first valid tap on a detected plane, this script instantiates a dog prefab.
/// - On subsequent taps, it tells the dog to move to the tapped position.
/// 
/// Requirements:
/// - Attach this script to the object that has ARRaycastManager (typically AR Session Origin / XR Origin).
/// - Assign <see cref="dogPrefab"/> in the Inspector. The prefab should ideally already contain:
///     * DogMover component (handles movement/animation parameter switching)
///     * Animator component with a bound RuntimeAnimatorController (e.g., DogController)
/// - Ensure an ARPlaneManager exists in the scene (for plane detection). You can leave
///   <see cref="planeManager"/> unassigned unless you want to disable plane visuals after placement.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class TapToPlaceAndMoveDog : MonoBehaviour
{
    // =========================
    // Inspector-assigned fields
    // =========================

    [Header("Assign in Inspector")]
    [Tooltip("Prefab of the dog to spawn. Recommended: a Prefab Variant that already has DogMover + Animator + Controller configured.")]
    public GameObject dogPrefab;

    [Header("Optional")]
    [Tooltip("Optional reference to ARPlaneManager. If provided, you can disable plane visualization after placement.")]
    public ARPlaneManager planeManager;

    // ==============
    // Private fields
    // ==============

    /// <summary>Cached ARRaycastManager used to raycast against tracked planes.</summary>
    private ARRaycastManager raycastManager;

    /// <summary>Main AR camera used to face the dog toward the user when spawning.</summary>
    private Camera arCamera;

    /// <summary>Reference to the spawned dog instance. Null until the first successful tap.</summary>
    private GameObject spawnedDog;

    /// <summary>Reusable hit list to avoid per-frame allocations.</summary>
    private static readonly List<ARRaycastHit> hits = new();

    // =========
    // Lifecycle
    // =========

    private void Awake()
    {
        // Cache required components. ARRaycastManager is guaranteed by the RequireComponent attribute.
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main; // Safe for ARFoundation projects where MainCamera is the AR Camera.
    }

    private void Update()
    {
        // No active touches -> nothing to do.
        if (Input.touchCount == 0) return;

        // We only react to the initial touch (Tap).
        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // Raycast the touch position against detected planes (within their polygon).
        if (!raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon)) return;

        // Use the closest hit (index 0). ARFoundation sorts by distance.
        var hitPose = hits[0].pose;

        // First tap: spawn the dog
        if (spawnedDog == null)
        {
            // Compute a rotation so the dog faces the camera horizontally (keeping its Y aligned to the plane).
            var lookAtCam = Quaternion.LookRotation(
                new Vector3(arCamera.transform.position.x, hitPose.position.y, arCamera.transform.position.z) - hitPose.position
            );

            // Instantiate the prefab at the hit pose.
            spawnedDog = Instantiate(dogPrefab, hitPose.position, lookAtCam);

            // Retrieve (or add as a fallback) the DogMover on the spawned instance.
            var mover = spawnedDog.GetComponent<DogMover>();
            if (mover == null) mover = spawnedDog.AddComponent<DogMover>(); // Safety net if the prefab missed it.

            // Ensure we have an Animator and that it doesn't apply root motion,
            // since DogMover handles translation/rotation explicitly.
            var anim = spawnedDog.GetComponentInChildren<Animator>();
            if (anim) anim.applyRootMotion = false;
            if (mover.animator == null) mover.animator = anim;

            // Optional: stop showing new/old planes after placing the dog.
            // if (planeManager) planeManager.enabled = false;
        }
        // Subsequent taps: move the existing dog
        else
        {
            var mover = spawnedDog.GetComponent<DogMover>();
            if (mover != null)
            {
                // In our DogMover we lock Y internally, but it's fine to pass the raw hit pose here.
                mover.MoveTo(hitPose.position);
            }
            else
            {
                // Absolute fallback: if no DogMover exists, teleport to the tapped pose.
                spawnedDog.transform.position = hitPose.position;
            }
        }
    }
}

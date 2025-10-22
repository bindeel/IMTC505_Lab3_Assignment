using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DropOnPlane : MonoBehaviour
{
    public GameObject objectToDrop;
    private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager; // used to verify horizontal alignment
    
    // Drop Settings
    [SerializeField] private float spawnDistance = 0.5f;       // meters in front of the camera along the touch ray
    [SerializeField] private float gravity = 9.81f;            // m/s^2
    [SerializeField] private float initialHorizontalSpeed = 0; // m/s (0 = pure drop)
    [SerializeField] private bool useCameraForwardForHorizontal = true; // if false, uses camera.right
    [SerializeField] private float planeSnapEpsilon = 0.01f;   // snap tolerance (meters)

    // Falling Visuals
    [SerializeField] private float shrinkRatePerSecond = 0.5f; // shrink ratio per second (1 -> 0)
    [SerializeField] private float minScaleRatio = 0.4f;       // don't shrink below 40% original

    // Lifetime
    [SerializeField] private float disappearAfterSecondsIfNoPlane = 1.5f;
    
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Camera arCamera;
    
    private GameObject placedObject;
    private Vector3 velocity;          // current velocity (m/s)
    private bool isFalling;
    private Vector3 originalScale;     // prefab's original scale
    private float scaleRatio = 1f;     // current uniform scale ratio
    private float aliveTimer;          // time since spawn (s)

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager   = GetComponent<ARPlaneManager>();
        arCamera = Camera.main;
    }

    void Update()
    {
        // Begin: spawn & drop from touch
        if (Input.touchCount > 0)
        {   
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                SpawnAndDropFromTouch(touch.position);
            }
        }

        // Update falling motion
        if (isFalling && placedObject)
        {
            float dt = Time.deltaTime;
            aliveTimer += dt;

            // Gravity
            velocity += Vector3.down * gravity * dt;

            // Predict next position
            Vector3 curPos = placedObject.transform.position;
            Vector3 nextPos = curPos + velocity * dt;

            // Raycast straight down from *next* position to find planes
            hits.Clear();
            var downRay = new Ray(nextPos + Vector3.up * 0.001f, Vector3.down);
            bool hitPlane = raycastManager && raycastManager.Raycast(downRay, hits, TrackableType.PlaneWithinPolygon);

            if (hitPlane)
            {
                // Get nearest hit
                var hit = hits[0];
                bool horizontal = IsHorizontalPlane(hit);

                // Land if we're essentially at the plane and moving downward
                if (horizontal && velocity.y <= 0f && Mathf.Abs(nextPos.y - hit.pose.position.y) <= planeSnapEpsilon)
                {
                    placedObject.transform.position = hit.pose.position;
                    velocity = Vector3.zero;
                    isFalling = false; // stick to plane
                    return;
                }
            }

            // Keep falling
            placedObject.transform.position = nextPos;

            // Shrink while falling
            scaleRatio = Mathf.Max(minScaleRatio, scaleRatio - shrinkRatePerSecond * dt);
            placedObject.transform.localScale = originalScale * scaleRatio;

            // Despawn if no plane caught us within the time limit
            if (aliveTimer >= disappearAfterSecondsIfNoPlane)
            {
                Destroy(placedObject);
                placedObject = null;
                isFalling = false;
            }
        }
    }

    private void SpawnAndDropFromTouch(Vector2 screenPos)
    {
        if (!arCamera || !objectToDrop) return;

        // If an old object exists, replace it
        if (placedObject) Destroy(placedObject);

        // Convert touch to a world position along the camera ray at a fixed distance
        Ray touchRay = arCamera.ScreenPointToRay(screenPos);
        Vector3 startPos = touchRay.origin + touchRay.direction.normalized * spawnDistance;

        // Face roughly the camera yaw (looks nicer)
        Quaternion rot = Quaternion.Euler(0f, arCamera.transform.eulerAngles.y, 0f);

        placedObject = Instantiate(objectToDrop, startPos, rot);

        // Cache scale & reset state
        originalScale = placedObject.transform.localScale;
        scaleRatio = 1f;
        aliveTimer = 0f;

        // Initial horizontal throw (optional)
        Vector3 horizDir = useCameraForwardForHorizontal
            ? Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up).normalized
            : Vector3.ProjectOnPlane(arCamera.transform.right,   Vector3.up).normalized;

        velocity = horizDir * Mathf.Max(0f, initialHorizontalSpeed); // no initial vertical component
        isFalling = true;

        // Ensure physics (if any) won't fight us
        var rb = placedObject.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;  // we handle motion manually
            rb.useGravity = false;
        }
    }

    private bool IsHorizontalPlane(ARRaycastHit hit)
    {
        // Prefer an explicit alignment check via ARPlaneManager if available
        if (planeManager)
        {
            ARPlane plane = planeManager.GetPlane(hit.trackableId);
            if (plane != null)
            {
                return plane.alignment == PlaneAlignment.HorizontalUp ||
                       plane.alignment == PlaneAlignment.HorizontalDown;
            }
        }

        // Fallback: use the hit pose's up vector
        return Vector3.Dot(hit.pose.up, Vector3.up) > 0.95f;
    }
}

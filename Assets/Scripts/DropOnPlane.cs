using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DropOnPlane : MonoBehaviour
{
    public GameObject objectToDrop;

    // Drop Settings
    private float spawnDistance = 0.5f;       // meters in front of the camera along the touch ray
    private float gravity = 9.81f;            // m/s^2
    private float initialHorizontalSpeed = 5; // m/s (0 = pure drop)
    private bool useCameraForwardForHorizontal = true; // if false, uses camera.right
    private float planeSnapEpsilon = 0.01f;   // snap tolerance (meters)

    // Falling Visuals
    private float shrinkRatePerSecond = 0.5f; // shrink ratio per second (1 -> 0)
    private float minScaleRatio = 0.6f;       // don't shrink below 60% original

    // Lifetime
    private float disappearAfterSecondsIfNoPlane = 0.5f;
    
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Camera camera;
    
    private GameObject placedObject;
    private Vector3 velocity;          // current velocity (m/s)
    private bool isFalling;
    private Vector3 originalScale;     // prefab's original scale
    private float scaleRatio = 0.8f;     // current uniform scale ratio
    private float aliveTimer;          // time since spawn (s)

    void Start()
    {
        camera = Camera.main;
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
            
            hits.Clear();

            // object footprint for the cast
            float radius = 0.05f;
            if (placedObject && placedObject.TryGetComponent<Collider>(out var col))
                radius = Mathf.Max(0.02f, Mathf.Max(col.bounds.extents.x, col.bounds.extents.z));

            // cover the whole movement this frame to avoid tunneling
            float   fallDist = Mathf.Max((curPos - nextPos).magnitude + radius * 0.5f, radius * 2f);

            // cast downward to see if we touch a horizontal ARPlane
            if (Physics.SphereCast(curPos + Vector3.up * radius,
                    radius,
                    Vector3.down,
                    out var physHit,
                    fallDist,
                    ~0,
                    QueryTriggerInteraction.Ignore))
            {
                var plane = physHit.collider.GetComponentInParent<ARPlane>();
                if (plane != null &&
                    (plane.alignment == PlaneAlignment.HorizontalUp || plane.alignment == PlaneAlignment.HorizontalDown) &&
                    velocity.y <= 0f)
                {
                    float contactY = physHit.point.y;

                    // snap if we're essentially on the plane
                    if ((curPos.y - contactY) <= Mathf.Max(planeSnapEpsilon, radius * 0.25f))
                    {
                        placedObject.transform.position = new Vector3(nextPos.x, contactY, nextPos.z);
                        velocity = Vector3.zero;
                        isFalling = false; // stick to plane
                        return;
                    }
                }
            }

            // Keep falling
            placedObject.transform.position = nextPos;

            // Shrink while falling
            scaleRatio = Mathf.Max(minScaleRatio, scaleRatio - shrinkRatePerSecond * dt);
            placedObject.transform.localScale = originalScale * scaleRatio;
            
            if (aliveTimer >= disappearAfterSecondsIfNoPlane)
            {
                velocity = Vector3.zero;
                isFalling = false; // stop moving/shrinking; it stays at its current position/scale
            }
        }
    }

    private void SpawnAndDropFromTouch(Vector2 screenPos)
    {
        if (!camera || !objectToDrop) return;

        // If an old object exists, replace it
        if (placedObject) Destroy(placedObject);

        // Convert touch to a world position along the camera ray at a fixed distance
        Ray touchRay = camera.ScreenPointToRay(screenPos);
        Vector3 startPos = touchRay.origin + touchRay.direction.normalized * spawnDistance;

        // Face roughly the camera yaw (looks nicer)
        Quaternion rot = Quaternion.Euler(0f, camera.transform.eulerAngles.y, 0f);

        placedObject = Instantiate(objectToDrop, startPos, rot);

        // Cache scale & reset state
        originalScale = placedObject.transform.localScale;
        scaleRatio = 1f;
        aliveTimer = 0f;

        // Initial horizontal throw (optional)
        Vector3 horizDir = useCameraForwardForHorizontal
            ? Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized
            : Vector3.ProjectOnPlane(camera.transform.right,   Vector3.up).normalized;

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
}

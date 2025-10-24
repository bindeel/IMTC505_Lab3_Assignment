using UnityEngine;

/// <summary>
/// Moves a dog character toward tapped positions on the ground (XZ plane).
/// Intended to be used by TapToPlaceAndMoveDog: after the dog is spawned,
/// call <see cref="MoveTo(Vector3)"/> with the world position from an AR raycast.
///
/// Features:
/// - Smooth rotation toward travel direction.
/// - Optional use of CharacterController (for simple collision/grounding).
/// - Graceful fallback to transform-based movement if no CharacterController is present/enabled.
/// - Optional Animator integration: toggles a boolean parameter (e.g., "IsWalking") while moving.
/// - Optional auto-fit of CharacterController dimensions to the renderers' bounds.
///
/// Notes:
/// - If you use an Animator, uncheck "Apply Root Motion" so this script fully controls translation/rotation.
/// - Movement is horizontal (XZ). Y is locked to the current height when a destination is set.
/// </summary>
public class DogMover : MonoBehaviour
{
    // -----------------------
    // Movement configuration
    // -----------------------

    /// <summary>
    /// Linear speed in meters per second.
    /// </summary>
    [Header("Movement")]
    public float moveSpeed = 0.8f; // m/s

    /// <summary>
    /// Rotation interpolation speed (higher = snappier turning).
    /// </summary>
    public float rotateSpeed = 8f;

    /// <summary>
    /// Distance threshold (in meters) to consider the target "reached".
    /// </summary>
    public float stoppingDistance = 0.05f;

    // -----------------------
    // Animator (optional)
    // -----------------------

    /// <summary>
    /// Optional Animator. If assigned, this script will toggle a boolean parameter while moving.
    /// Tip: disable "Apply Root Motion" on the Animator so motion is controlled by this script.
    /// </summary>
    [Header("Optional Animation")]
    public Animator animator;

    /// <summary>
    /// Name of the Animator bool parameter that indicates the walking state.
    /// The controller should have an Idle ↔ Walk transition driven by this bool.
    /// </summary>
    public string isWalkingBool = "IsWalking";

    // -----------------------
    // CharacterController
    // -----------------------

    /// <summary>
    /// If true and a CharacterController exists and is enabled, use it for movement.
    /// If false or no controller is present, falls back to transform-based movement.
    /// </summary>
    [Header("Controller")]
    public bool useCharacterControllerIfPresent = true;

    /// <summary>
    /// If true, attempts to auto-fit the CharacterController dimensions to the
    /// renderers' combined bounds on Awake().
    /// </summary>
    public bool autoFitCharacterController = true;

    // -----------------------
    // Internals
    // -----------------------

    /// <summary>
    /// Target world position (Y will be locked to the current transform height
    /// at the time <see cref="MoveTo(Vector3)"/> is called).
    /// </summary>
    private Vector3? target;

    /// <summary>
    /// Cached CharacterController (optional).
    /// </summary>
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Find an Animator if none was assigned in the Inspector.
        if (!animator) animator = GetComponentInChildren<Animator>();

        // Optionally fit the CharacterController capsule to the rendered bounds.
        if (autoFitCharacterController && controller)
            FitCharacterControllerToRenderers();

        // Important: let this script control translation/rotation.
        if (animator) animator.applyRootMotion = false;
    }

    private void Update()
    {
        if (target == null) return;

        // --- Planar (XZ) movement only ---
        Vector3 cur = transform.position;
        Vector3 tgt = target.Value; // Y was locked in MoveTo()
        Vector3 toTarget = new Vector3(tgt.x - cur.x, 0f, tgt.z - cur.z);
        float dist = toTarget.magnitude;

        // Reached destination?
        if (dist <= stoppingDistance)
        {
            if (animator) animator.SetBool(isWalkingBool, false);
            target = null;
            return;
        }

        // Face travel direction smoothly.
        Vector3 dir = toTarget / Mathf.Max(dist, 1e-5f);
        if (dir.sqrMagnitude > 1e-6f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotateSpeed);
        }

        // Compute step for this frame.
        Vector3 move = dir * moveSpeed * Time.deltaTime;
        bool moved = false;

        // Prefer CharacterController if present/enabled (basic collision/grounding).
        if (useCharacterControllerIfPresent && controller && controller.enabled)
        {
            // Light downward bias keeps the capsule seated on surfaces.
            move.y = Physics.gravity.y * Time.deltaTime * 0.2f;
            controller.Move(move);

            // Consider "moved" if controller reported any meaningful motion.
            moved = controller.velocity.sqrMagnitude > 1e-6f || move.sqrMagnitude > 1e-8f;
        }

        // Fallback: direct transform translation (no physics).
        if (!moved)
        {
            transform.position += new Vector3(move.x, 0f, move.z);
        }

        // Drive Animator state while moving.
        if (animator) animator.SetBool(isWalkingBool, true);
    }

    /// <summary>
    /// Sets a new destination. Y is locked to the current transform height to keep motion planar.
    /// Call this with the AR raycast hit position when the user taps.
    /// </summary>
    /// <param name="worldPos">World-space destination (Y will be overridden).</param>
    public void MoveTo(Vector3 worldPos)
    {
        // Lock to current height to avoid situations where vertical offsets prevent horizontal motion.
        worldPos.y = transform.position.y;
        target = worldPos;

        if (animator) animator.SetBool(isWalkingBool, true);
    }

    /// <summary>
    /// Fits the CharacterController capsule to the combined bounds of all child renderers.
    /// This is a best-effort estimate intended to prevent oversized capsules from intersecting the ground.
    /// </summary>
    private void FitCharacterControllerToRenderers()
    {
        var rends = GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;

        Bounds b = new Bounds(transform.position, Vector3.zero);
        foreach (var r in rends) b.Encapsulate(r.bounds);

        // Height approximates total visual height; radius is a fraction of min XZ size.
        float height = Mathf.Max(0.1f, b.size.y);
        float radius = Mathf.Max(0.02f, Mathf.Min(b.size.x, b.size.z) * 0.25f);

        controller.center = new Vector3(0f, height * 0.5f, 0f);
        controller.height = height;
        controller.radius = radius;

        // Reasonable defaults for small characters in AR-scale scenes.
        controller.stepOffset = Mathf.Min(0.3f, height * 0.3f);
        controller.minMoveDistance = 0.0005f;
    }
}

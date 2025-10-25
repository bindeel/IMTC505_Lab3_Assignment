using UnityEngine;

public class PinchToScale : MonoBehaviour
{
    private float startDistance;
    private Vector3 startScale;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // gesture start
            if (t1.phase == TouchPhase.Began)
            {
                startDistance = Vector2.Distance(t0.position, t1.position);
                startScale = transform.localScale;
            }

            // while fingers move
            if (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(t0.position, t1.position);
                if (Mathf.Approximately(startDistance, 0)) return;

                float scaleFactor = currentDistance / startDistance;
                scaleFactor = Mathf.Clamp(scaleFactor, 0.3f, 3f); // avoid extremes
                transform.localScale = startScale * scaleFactor;
            }
        }
    }
}
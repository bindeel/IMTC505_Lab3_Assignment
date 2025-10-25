using UnityEngine;

public class TwoFingerRotate : MonoBehaviour
{
    private float startAngle;
    private Quaternion startRotation;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            if (t1.phase == TouchPhase.Began)
            {
                startAngle = AngleBetween(t0.position, t1.position);
                startRotation = transform.rotation;
            }

            float currentAngle = AngleBetween(t0.position, t1.position);
            float delta = currentAngle - startAngle;

            // rotate around Y-axis
            transform.rotation = Quaternion.Euler(0, -delta, 0) * startRotation;
        }
    }

    float AngleBetween(Vector2 a, Vector2 b)
    {
        Vector2 from = b - a;
        return Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg;
    }
}
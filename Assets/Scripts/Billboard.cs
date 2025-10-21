using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main; if (!cam) return;
        transform.LookAt(cam.transform.position);
        transform.Rotate(0f, 180f, 0f);
    }
}
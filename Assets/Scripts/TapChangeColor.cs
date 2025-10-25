using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class TapChangeColor : MonoBehaviour
{
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void OnMouseDown()
    {
        // Pick a random color each tap
        Color newColor = new Color(Random.value, Random.value, Random.value);
        rend.material.color = newColor;
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ButtonAction : MonoBehaviour
{
    public enum Action { PlayPause, Up, Down }
    public Action action = Action.PlayPause;
}
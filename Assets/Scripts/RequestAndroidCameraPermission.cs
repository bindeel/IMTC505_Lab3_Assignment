// RequestAndroidCameraPermission.cs
using UnityEngine;
public class RequestAndroidCameraPermission : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    void Start()
    {
        var cam = "android.permission.CAMERA";
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(cam))
            UnityEngine.Android.Permission.RequestUserPermission(cam);
    }
#endif
}
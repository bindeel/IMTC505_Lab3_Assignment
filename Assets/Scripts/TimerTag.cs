using System.Collections;
using UnityEngine;
using TMPro;

// NEW INPUT SYSTEM
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TimerTag : MonoBehaviour
{
    [Header("UI (optional but recommended)")]
    public TextMeshPro labelText;   // big title (e.g., “Pasta”)
    public TextMeshPro statusText;  // “Running / Paused / Done”
    public TextMeshPro timeText;    // “MM:SS”
    public Renderer    cardRenderer;

    [Header("Audio")]
    public AudioSource beep;

    [Header("Colors")]
    public Color runningColor = new Color(0.2f, 0.6f, 0.9f);
    public Color pausedColor  = new Color(0.9f, 0.75f, 0.2f);
    public Color doneColor    = new Color(0.2f, 0.8f, 0.3f);

    [Header("Behavior")]
    public bool startOnEnable = false; // if true, starts when spawned (if time was set)
    public float longPressTime = 0.7f;

    int  totalSeconds;
    int  remaining;
    bool running;
    bool done;

    float pressTimer;
    bool  pressing;

    float tickAcc; // unscaled accumulator

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
    }

    void OnDisable()
    {
        TouchSimulation.Disable();
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        UpdateTimeText();
        UpdateVisuals();
        Debug.Log("[TimerTag] Start: total=" + totalSeconds + " remaining=" + remaining);
        if (startOnEnable && remaining > 0) StartTimer();
    }

    void Update()
    {
        // --- Tick in Update with unscaled time (robust) ---
        if (running && !done && remaining > 0)
        {
            tickAcc += Time.unscaledDeltaTime;
            while (tickAcc >= 1f && remaining > 0)
            {
                tickAcc -= 1f;
                remaining--;
                UpdateTimeText();

                if (remaining <= 0)
                {
                    done = true; running = false;
                    UpdateVisuals();
                    OnTimerDone();
                }
            }
        }

        // --- Long press (New Input System) to pause/resume/reset ---
        foreach (var t in Touch.activeTouches)
        {
            if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (IsTouchOverMe(t.screenPosition)) { pressing = true; pressTimer = 0f; }
            }
            else if ((t.phase == UnityEngine.InputSystem.TouchPhase.Moved || t.phase == UnityEngine.InputSystem.TouchPhase.Stationary) && pressing)
            {
                pressTimer += Time.unscaledDeltaTime;
                if (pressTimer >= longPressTime)
                {
                    pressing = false;
                    TogglePause();
                    HapticLight();
                }
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended || t.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                pressing = false;
            }
        }
    }

    bool IsTouchOverMe(Vector2 screenPos)
    {
        var cam = Camera.main; if (!cam) return false;
        Ray r = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(r, out RaycastHit hit, 10f))
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        return false;
    }

    // -------- Public API used by the placer --------
    public void SetLabel(string label)
    {
        if (labelText) labelText.text = label;
    }

    public void SetTime(int minutes, int seconds)
    {
        totalSeconds = Mathf.Max(0, minutes * 60 + seconds);
        remaining = totalSeconds;
        done = false;
        running = false;
        tickAcc = 0f;
        UpdateTimeText();
        UpdateVisuals();
        Debug.Log($"[TimerTag] SetTime: {minutes:00}:{seconds:00} ({totalSeconds}s)");
    }

    public void StartTimer()
    {
        if (remaining <= 0) { Debug.LogWarning("[TimerTag] StartTimer ignored: remaining <= 0"); return; }
        done = false;
        running = true;
        UpdateVisuals();
        Debug.Log("[TimerTag] Started");
    }

    public void PauseTimer()
    {
        running = false;
        UpdateVisuals();
        Debug.Log("[TimerTag] Paused at " + remaining + "s");
    }

    public void ResetTimer()
    {
        remaining = totalSeconds;
        done = false;
        tickAcc = 0f;
        UpdateTimeText();
        UpdateVisuals();
        Debug.Log("[TimerTag] Reset");
    }

    public void TogglePause()
    {
        if (done) { ResetTimer(); StartTimer(); return; }
        if (running) PauseTimer(); else StartTimer();
    }
    // ------------------------------------------------

    void OnTimerDone()
    {
        if (beep) beep.Play();
        HapticStrong();
        StartCoroutine(PulseCo());
        Debug.Log("[TimerTag] Done");
    }

    IEnumerator PulseCo()
    {
        Vector3 s = transform.localScale;
        transform.localScale = s * 1.12f;
        yield return null; // one frame
        yield return new WaitForSecondsRealtime(0.12f);
        transform.localScale = s;
    }

    void UpdateTimeText()
    {
        int m = Mathf.Max(0, remaining) / 60;
        int s = Mathf.Max(0, remaining) % 60;
        string t = $"{m:00}:{s:00}";
        if (timeText) timeText.text = t;
        else if (labelText) labelText.text = $"{(string.IsNullOrEmpty(labelText.text) ? "Timer" : labelText.text.Split('—')[0])} — {t}";
    }

    void UpdateVisuals()
    {
        if (statusText)
            statusText.text = done ? "Done" : (running ? "Running" : "Paused");

        if (cardRenderer)
            cardRenderer.material.color = done ? doneColor : (running ? runningColor : pausedColor);
    }

    // Simple Android haptics
    void HapticLight()  { Handheld.Vibrate(); }
    void HapticStrong() { Handheld.Vibrate(); }
}

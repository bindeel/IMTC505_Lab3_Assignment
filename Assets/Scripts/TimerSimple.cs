using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TimerSimple : MonoBehaviour
{
    [Header("UI (3D TMP objects)")]
    public TextMeshPro timeText;      // center MM:SS
    public TextMeshPro playPauseText; // left glyph ▶ / ⏸
    public TextMeshPro upText;        // right-top ▲
    public TextMeshPro downText;      // right-bottom ▼
    public Renderer cardRenderer;     // Quad background

    [Header("Colors")]
    public Color idleColor    = new(0.15f, 0.15f, 0.18f);
    public Color runningColor = new(0.10f, 0.35f, 0.60f);
    public Color pausedColor  = new(0.55f, 0.45f, 0.15f);
    public Color doneColor    = new(0.15f, 0.55f, 0.20f);

    [Header("Timing")]
    public int totalSeconds = 0;
    public bool running = false;
    public int stepSeconds = 30;          // ▲/▼ increment
    public float holdRepeatDelay = 0.5f;  // hold start
    public float holdRepeatRate  = 0.15f; // hold repeat

    float accUnscaled;
    bool  done;

    ButtonAction holdButton;
    float holdTimer;
    float holdRepeatTimer;

    void OnEnable()  { EnhancedTouchSupport.Enable(); TouchSimulation.Enable(); }
    void OnDisable() { TouchSimulation.Disable(); EnhancedTouchSupport.Disable(); }

    void Start()
    {
        RefreshText();
        RefreshVisuals();
    }

    void Update()
    {
        // Tick in unscaled time
        if (running && !done && totalSeconds > 0)
        {
            accUnscaled += Time.unscaledDeltaTime;
            while (accUnscaled >= 1f && totalSeconds > 0)
            {
                accUnscaled -= 1f;
                totalSeconds--;
                RefreshTimeOnly();
                if (totalSeconds <= 0)
                {
                    running = false; done = true;
                    RefreshVisuals();
                    // (optional) beep/haptic here
                }
            }
        }

        // Handle taps/holds on buttons via physics ray
        foreach (var t in Touch.activeTouches)
        {
            if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                var btn = RayHitButton(t.screenPosition);
                if (btn != null)
                {
                    PressButton(btn);
                    holdButton = btn; holdTimer = 0f; holdRepeatTimer = 0f;
                }
            }
            else if ((t.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                      t.phase == UnityEngine.InputSystem.TouchPhase.Stationary) && holdButton != null)
            {
                holdTimer += Time.unscaledDeltaTime;
                if (holdTimer >= holdRepeatDelay &&
                    (holdButton.action == ButtonAction.Action.Up || holdButton.action == ButtonAction.Action.Down))
                {
                    holdRepeatTimer += Time.unscaledDeltaTime;
                    if (holdRepeatTimer >= holdRepeatRate)
                    {
                        holdRepeatTimer = 0f;
                        PressButton(holdButton);
                    }
                }
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                     t.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                holdButton = null;
            }
        }
    }

    ButtonAction RayHitButton(Vector2 screenPos)
    {
        var cam = Camera.main; if (!cam) return null;
        var ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, 10f))
            return hit.collider.GetComponent<ButtonAction>();
        return null;
    }

    void PressButton(ButtonAction btn)
    {
        switch (btn.action)
        {
            case ButtonAction.Action.PlayPause:
                if (done && totalSeconds <= 0) totalSeconds = 60; // safety default
                if (done) done = false;
                running = !running;
                RefreshVisuals();
                break;

            case ButtonAction.Action.Up:
                totalSeconds = Mathf.Clamp(totalSeconds + stepSeconds, 0, 99 * 60 + 59);
                RefreshTimeOnly();
                break;

            case ButtonAction.Action.Down:
                totalSeconds = Mathf.Max(0, totalSeconds - stepSeconds);
                if (totalSeconds == 0 && running) running = false;
                RefreshTimeOnly();
                break;
        }
    }

    public void SetTimeMMSS(int mm, int ss)
    {
        totalSeconds = Mathf.Max(0, mm * 60 + ss);
        running = false; done = false; accUnscaled = 0f;
        RefreshText();
        RefreshVisuals();
    }

    void RefreshText()
    {
        RefreshTimeOnly();
        if (playPauseText) playPauseText.text = running ? "⏸" : "▶";
        // Up/Down glyphs are static (“▲”, “▼”) set in prefab
    }

    void RefreshTimeOnly()
    {
        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        if (timeText) timeText.text = $"{m:00}:{s:00}";
    }

    void RefreshVisuals()
    {
        if (cardRenderer)
        {
            var c = done ? doneColor : (running ? runningColor : (totalSeconds > 0 ? idleColor : pausedColor));
            cardRenderer.material.color = c;
        }
        if (playPauseText) playPauseText.text = running ? "⏸" : "▶";
    }
}

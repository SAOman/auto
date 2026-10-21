using UnityEngine;

// Wrzuć na First Person Controller (ten sam obiekt co Rigidbody).
public class GameTimer : MonoBehaviour
{
    public float moveThreshold = 0.1f;   // minimalna prędkość pozioma uznawana za ruch
    public int fontSize = 36;            // rozmiar w rogu; po mecie x2 na środku

    private Rigidbody rb;
    private float time;
    private bool running;
    private bool finished;
    private GUIStyle style;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (finished) return;

        if (!running)
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            if (v.magnitude > moveThreshold)
                running = true;
        }

        if (running)
            time += Time.deltaTime;
    }

    // Meta: zatrzymuje zegar i pokazuje go na środku ekranu.
    public void StopTimer()
    {
        if (finished) return;
        running = false;
        finished = true;
    }

    // Reset: zeruje zegar i czeka na pierwszy ruch.
    public void ResetTimer()
    {
        time = 0f;
        running = false;
        finished = false;
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
        }

        int minutes = (int)(time / 60f);
        float seconds = time - minutes * 60f;
        string text = string.Format("{0:00}:{1:00.00}", minutes, seconds);

        if (finished)
        {
            style.fontSize = fontSize * 2;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), text, style);
        }
        else
        {
            style.fontSize = fontSize;
            style.alignment = TextAnchor.UpperRight;
            GUI.Label(new Rect(Screen.width - 320, 10, 300, 60), text, style);
        }
    }
}
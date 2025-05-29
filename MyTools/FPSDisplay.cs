using System.Text;
using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    [Header("Settings")]
    public float updateInterval = 0.5f;

    [Header("UI")]
    public TextMeshProUGUI fpsText;

    private float accumulatedTime = 0f;
    private int frameCount = 0;
    private float nextUpdate = 0f;

    private StringBuilder sb = new StringBuilder();

    void Start()
    {
        nextUpdate = Time.realtimeSinceStartup + updateInterval;
    }

    void Update()
    {
        accumulatedTime += Time.unscaledDeltaTime;
        frameCount++;

        if (Time.realtimeSinceStartup >= nextUpdate)
        {
            float fps = frameCount / accumulatedTime;
            float ms = 1000f / Mathf.Max(fps, 0.0001f); // avoid division by zero

            sb.Clear();
            sb.AppendFormat("FPS: {0:F1}\n", fps);
            sb.AppendFormat("MS: {0:F2} ms", ms);

            fpsText.text = sb.ToString();

            // Reset counters
            frameCount = 0;
            accumulatedTime = 0f;
            nextUpdate = Time.realtimeSinceStartup + updateInterval;
        }
    }
}

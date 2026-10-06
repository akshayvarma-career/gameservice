using UnityEngine;
using UnityEngine.UI;

namespace SilentLedger.UI
{
    /// <summary>Average FPS and frame time, shown only in development builds and the Editor.</summary>
    public class DevStats : MonoBehaviour
    {
        [SerializeField] Text label;
        [SerializeField] float sampleSeconds = 0.5f;

        int frames;
        float elapsed;

        void Awake()
        {
            if (!Debug.isDebugBuild) gameObject.SetActive(false);
        }

        void Update()
        {
            frames++;
            elapsed += Time.unscaledDeltaTime;
            if (elapsed < sampleSeconds) return;
            float fps = frames / elapsed;
            label.text = $"{fps:0} FPS  {1000f / fps:0.0} ms";
            label.color = fps >= 55f ? new Color(0.6f, 1f, 0.6f) : fps >= 28f ? new Color(1f, 0.85f, 0.4f) : new Color(1f, 0.45f, 0.4f);
            frames = 0;
            elapsed = 0f;
        }
    }
}

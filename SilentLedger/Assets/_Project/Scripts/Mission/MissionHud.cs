using System.Collections;
using SilentLedger.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace SilentLedger.Mission
{
    /// <summary>
    /// Story-facing HUD: current objective, subtitles, chapter banners and an on-screen waypoint
    /// that points at the objective (clamped to the screen edge when it is off screen).
    /// </summary>
    public class MissionHud : MonoBehaviour
    {
        [SerializeField] RectTransform canvasRect;
        [SerializeField] Text objectiveText;
        [SerializeField] Text subtitleText;
        [SerializeField] Text bannerText;
        [SerializeField] CanvasGroup bannerGroup;
        [SerializeField] RectTransform waypoint;
        [SerializeField] Text waypointDistance;
        [Tooltip("Gap kept between an off-screen waypoint and the screen edge, in canvas units.")]
        [SerializeField] float edgeMargin = 90f;

        Transform waypointTarget;
        float waypointHeight;
        Camera viewCamera;

        void Awake()
        {
            SetObjective(null);
            subtitleText.text = "";
            bannerGroup.alpha = 0f;
            waypoint.gameObject.SetActive(false);
        }

        public void SetObjective(string objective)
        {
            string text = string.IsNullOrEmpty(objective)
                ? ""
                : $"<size=20><color=#F2A33A>OBJECTIVE</color></size>\n{objective}";
            if (text == objectiveText.text) return;
            objectiveText.text = text;
            if (text.Length > 0 && Sfx.Library != null) Sfx.Play2D(Sfx.Library.objectiveUpdated, 0.35f, 0f);
        }

        /// <summary>Points the waypoint at a target, or hides it when target is null.</summary>
        public void SetWaypoint(Transform target, float heightAboveTarget = 2.2f)
        {
            waypointTarget = target;
            waypointHeight = heightAboveTarget;
            waypoint.gameObject.SetActive(target != null);
        }

        /// <summary>Shows one subtitle line and waits long enough to read it.</summary>
        public IEnumerator Say(string speaker, Color color, string line)
        {
            subtitleText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{speaker}</color>   {line}";
            yield return new WaitForSeconds(Mathf.Clamp(1.2f + line.Length * 0.055f, 2.2f, 7f));
            subtitleText.text = "";
        }

        public IEnumerator ShowBanner(string title, string subtitle, float seconds)
        {
            bannerText.text = $"{title}\n<size=30>{subtitle}</size>";
            for (float t = 0f; t < 1f; t += Time.deltaTime * 2f) { bannerGroup.alpha = t; yield return null; }
            bannerGroup.alpha = 1f;
            yield return new WaitForSeconds(seconds);
            for (float t = 1f; t > 0f; t -= Time.deltaTime * 2f) { bannerGroup.alpha = t; yield return null; }
            bannerGroup.alpha = 0f;
        }

        void LateUpdate()
        {
            if (waypointTarget == null) return;
            if (viewCamera == null) viewCamera = Camera.main;
            if (viewCamera == null) return;

            Vector3 world = waypointTarget.position + Vector3.up * waypointHeight;
            Vector3 local = viewCamera.transform.InverseTransformPoint(world);
            Vector3 screen = viewCamera.WorldToScreenPoint(world);
            Vector2 half = canvasRect.rect.size * 0.5f - Vector2.one * edgeMargin;

            Vector2 position;
            bool onScreen = local.z > 0f
                            && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out position)
                            && Mathf.Abs(position.x) <= half.x && Mathf.Abs(position.y) <= half.y;
            if (!onScreen)
            {
                // Off screen or behind: push to the edge in the direction of the target.
                Vector2 direction = new Vector2(local.x, local.y);
                if (direction.sqrMagnitude < 0.0001f) direction = Vector2.down;
                direction.Normalize();
                float scale = Mathf.Min(half.x / Mathf.Max(Mathf.Abs(direction.x), 0.0001f),
                                        half.y / Mathf.Max(Mathf.Abs(direction.y), 0.0001f));
                position = direction * scale;
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out position);
            }

            waypoint.anchoredPosition = position;
            float distance = Vector3.Distance(viewCamera.transform.position, waypointTarget.position);
            waypointDistance.text = $"{distance:0} m";
        }
    }
}

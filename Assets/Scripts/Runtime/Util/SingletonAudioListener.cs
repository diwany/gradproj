using UnityEngine;

namespace Museum.Util
{
    /// <summary>
    /// Drop this on any always-loaded GameObject (e.g. XR Origin) to ensure only one AudioListener
    /// is active at runtime. Disables every other AudioListener it finds, prefers the one nearest
    /// to (or under) this GameObject.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class SingletonAudioListener : MonoBehaviour
    {
        void Awake() => Reconcile();
        void OnEnable() => Reconcile();

        public void Reconcile()
        {
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude);
            if (listeners.Length <= 1) return;

            // Prefer one under this GameObject's hierarchy; else first found.
            AudioListener winner = null;
            foreach (var l in listeners)
            {
                if (l.transform.IsChildOf(transform)) { winner = l; break; }
            }
            if (winner == null) winner = listeners[0];

            int disabled = 0;
            foreach (var l in listeners)
            {
                if (l == winner) continue;
                l.enabled = false;
                disabled++;
                Debug.Log($"[SingletonAudioListener] Disabled duplicate AudioListener on {GetPath(l.gameObject)}");
            }
            if (disabled > 0)
                Debug.Log($"[SingletonAudioListener] Kept AudioListener on {GetPath(winner.gameObject)}, disabled {disabled} other(s).");
        }

        static string GetPath(GameObject go)
        {
            var t = go.transform;
            var sb = new System.Text.StringBuilder(go.name);
            while (t.parent != null) { t = t.parent; sb.Insert(0, t.name + "/"); }
            return sb.ToString();
        }
    }
}

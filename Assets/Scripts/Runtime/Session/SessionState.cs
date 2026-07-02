using Museum.Persistence;
using UnityEngine;

namespace Museum.Session
{
    /// <summary>
    /// DontDestroyOnLoad singleton holding the active visitor row, the OpenAI API key,
    /// and (later) the live Realtime API client. Survives lobby -> museum scene transition.
    /// </summary>
    public class SessionState : MonoBehaviour
    {
        public static SessionState Instance { get; private set; }

        public VisitorRecord Visitor { get; set; }
        public string OpenAiApiKey { get; set; }

        public int ArtifactsIdentifiedThisSession { get; set; }
        public float OpenAiSecondsUsed { get; set; }

        public static SessionState GetOrCreate()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("[SessionState]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<SessionState>();
            return Instance;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}

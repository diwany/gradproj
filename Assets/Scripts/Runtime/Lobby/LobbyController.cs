using System;
using Museum.Config;
using Museum.Persistence;
using Museum.Session;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Museum.Lobby
{
    /// <summary>
    /// Wires the lobby form to SQLite + the Realtime API key + the museum scene transition.
    /// Bind the field references in the Inspector after Set Up Lobby Scene generates the scene.
    /// </summary>
    public class LobbyController : MonoBehaviour
    {
        [Header("Form")]
        public TMP_InputField nameField;
        public Slider ageSlider;
        public TMP_Text ageDisplay;
        public TMP_Dropdown countryDropdown;
        public Button submitButton;

        [Header("Status")]
        public TMP_Text statusText;
        public TMP_Text apiKeyStatusText;

        [Header("Data")]
        public CountryList countries;

        [Header("Transition")]
        [Tooltip("Scene to load on submit. Leave blank to use 'Museum'.")]
        public string museumSceneName = "Museum";

        [Tooltip("If true, loads the museum scene additively (preserves SessionState + future Realtime client). If false, uses Single mode.\n\nDefault is FALSE because additive mode briefly co-exists two XR Origins/cameras/AudioListeners during the load → controllers vanish, HMD shows a white render streak, movement breaks. SessionState is DontDestroyOnLoad, so Single mode preserves visitor + API key just fine.")]
        public bool loadAdditively = false;

        SessionState _session;
        MuseumDatabase _db;

        void Start()
        {
            _session = SessionState.GetOrCreate();
            _db = new MuseumDatabase();

            PopulateCountries();
            SetupAgeSlider();

            if (nameField != null) nameField.onValueChanged.AddListener(_ => UpdateSubmitInteractable());
            if (submitButton != null) submitButton.onClick.AddListener(OnSubmit);

            LoadAndDisplayKey();
            UpdateSubmitInteractable();
        }

        void OnDestroy()
        {
            _db?.Dispose();
        }

        void PopulateCountries()
        {
            if (countryDropdown == null) return;
            var list = (countries != null && countries.countries.Count > 0)
                ? new System.Collections.Generic.List<CountryList.Country>(countries.countries)
                : CountryList.Default();

            // Sort alphabetically, but pin "Other / Prefer not to say" to the bottom.
            list.Sort((a, b) =>
            {
                bool aOther = a.code == "XX";
                bool bOther = b.code == "XX";
                if (aOther && !bOther) return 1;
                if (!aOther && bOther) return -1;
                return string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase);
            });
            _sortedCountries = list;

            countryDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            foreach (var c in list) options.Add(c.name);
            countryDropdown.AddOptions(options);

            // Default to Egypt, since this is an Egyptian museum app.
            int egyptIdx = list.FindIndex(c => c.code == "EG");
            countryDropdown.value = egyptIdx >= 0 ? egyptIdx : 0;
            countryDropdown.RefreshShownValue();
        }

        System.Collections.Generic.List<CountryList.Country> _sortedCountries;

        void SetupAgeSlider()
        {
            if (ageSlider == null) return;
            ageSlider.minValue = 5;
            ageSlider.maxValue = 99;
            ageSlider.wholeNumbers = true;
            if (ageSlider.value < ageSlider.minValue) ageSlider.value = 25;
            ageSlider.onValueChanged.AddListener(_ => RefreshAgeDisplay());
            RefreshAgeDisplay();
        }

        void RefreshAgeDisplay()
        {
            if (ageDisplay != null && ageSlider != null)
                ageDisplay.text = $"Age: {(int)ageSlider.value}";
        }

        void LoadAndDisplayKey()
        {
            if (OpenAIConfig.TryLoadKey(out var key, out var err))
            {
                _session.OpenAiApiKey = key;
                if (apiKeyStatusText != null)
                {
                    apiKeyStatusText.text = "OpenAI key loaded.";
                    apiKeyStatusText.color = new Color(0.55f, 0.85f, 0.55f);
                }
            }
            else
            {
                _session.OpenAiApiKey = null;
                if (apiKeyStatusText != null)
                {
                    apiKeyStatusText.text = err;
                    apiKeyStatusText.color = new Color(0.95f, 0.55f, 0.45f);
                }
                Debug.LogWarning($"[Lobby] {err}");
            }
        }

        void UpdateSubmitInteractable()
        {
            if (submitButton == null) return;
            bool hasName = nameField != null && !string.IsNullOrWhiteSpace(nameField.text);
            bool hasKey = !string.IsNullOrEmpty(_session?.OpenAiApiKey);
            submitButton.interactable = hasName && hasKey;
            if (statusText != null && !hasName)
                statusText.text = "Enter your name to begin.";
            else if (statusText != null && !hasKey)
                statusText.text = "OpenAI key missing — see Status above.";
            else if (statusText != null)
                statusText.text = string.Empty;
        }

        void OnSubmit()
        {
            if (submitButton != null) submitButton.interactable = false;

            var country = ResolveCountry();
            var record = new VisitorRecord
            {
                Name = (nameField != null ? nameField.text : "Anonymous").Trim(),
                Age = ageSlider != null ? (int)ageSlider.value : 0,
                CountryCode = country.code,
                CountryName = country.name,
                StartedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            try
            {
                _db.InsertVisitor(record);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Lobby] DB insert failed: {e}");
                if (statusText != null) statusText.text = "Database error. See Console.";
                if (submitButton != null) submitButton.interactable = true;
                return;
            }

            _session.Visitor = record;
            Debug.Log($"[Lobby] Visitor #{record.Id}: {record.Name}, age {record.Age}, {record.CountryName}. Loading museum scene.");

            var sceneName = string.IsNullOrEmpty(museumSceneName) ? "Museum" : museumSceneName;
            if (loadAdditively)
            {
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).completed += _ =>
                {
                    var nextScene = SceneManager.GetSceneByName(sceneName);
                    if (nextScene.IsValid()) SceneManager.SetActiveScene(nextScene);
                    SceneManager.UnloadSceneAsync(gameObject.scene);
                };
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
        }

        CountryList.Country ResolveCountry()
        {
            // Resolve against the SORTED list so the dropdown index matches what the player actually selected.
            var list = _sortedCountries ?? ((countries != null && countries.countries.Count > 0)
                ? new System.Collections.Generic.List<CountryList.Country>(countries.countries)
                : CountryList.Default());
            if (countryDropdown == null || list.Count == 0) return list.Count > 0 ? list[0] : new CountryList.Country("XX", "Unknown");
            int idx = Mathf.Clamp(countryDropdown.value, 0, list.Count - 1);
            return list[idx];
        }
    }
}

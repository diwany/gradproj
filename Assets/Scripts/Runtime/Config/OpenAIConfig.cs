using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Museum.Config
{
    /// <summary>
    /// Reads the OpenAI API key from %APPDATA%/MuseumVR/config.json.
    /// File format: { "openai_api_key": "sk-..." }
    /// Operator owns the key; visitors never see or enter it.
    /// </summary>
    public static class OpenAIConfig
    {
        const string FileName = "config.json";

        [Serializable]
        class ConfigFile
        {
            [JsonProperty("openai_api_key")]
            public string OpenAiApiKey { get; set; }
        }

        public static string GetConfigPath()
        {
            // Portable path first: <exe folder>/MuseumVR/config.json. Lets the build run on any
            // machine just by copying the folder, no per-user setup needed.
            var portable = GetPortablePath();
            if (!string.IsNullOrEmpty(portable) && File.Exists(portable)) return portable;

            string baseDir;
            try { baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); }
            catch { baseDir = Application.persistentDataPath; }
            if (string.IsNullOrEmpty(baseDir)) baseDir = Application.persistentDataPath;
            return Path.Combine(baseDir, "MuseumVR", FileName);
        }

        static string GetPortablePath()
        {
            try
            {
                // In a built player Application.dataPath is "<exe>_Data"; one level up is the exe folder.
                // In the editor it's "<project>/Assets" — also fine, MuseumVR sibling there is honored too.
                var dataPath = Application.dataPath;
                if (string.IsNullOrEmpty(dataPath)) return null;
                var parent = Directory.GetParent(dataPath)?.FullName;
                if (string.IsNullOrEmpty(parent)) return null;
                return Path.Combine(parent, "MuseumVR", FileName);
            }
            catch { return null; }
        }

        public static bool TryLoadKey(out string apiKey, out string errorMessage)
        {
            apiKey = null;
            errorMessage = null;
            var path = GetConfigPath();
            if (!File.Exists(path))
            {
                var portable = GetPortablePath() ?? "<unavailable>";
                errorMessage = $"Config file not found.\n\nLooked at (in order):\n  1. {portable} (portable, next to the .exe)\n  2. {path}\n\nCreate either with:\n{{ \"openai_api_key\": \"sk-...\" }}";
                return false;
            }
            try
            {
                var raw = File.ReadAllText(path);
                var cfg = JsonConvert.DeserializeObject<ConfigFile>(raw);
                if (cfg == null || string.IsNullOrWhiteSpace(cfg.OpenAiApiKey))
                {
                    errorMessage = $"Config file at {path} is missing 'openai_api_key'.";
                    return false;
                }
                apiKey = cfg.OpenAiApiKey.Trim();
                return true;
            }
            catch (Exception e)
            {
                errorMessage = $"Failed to read config: {e.Message}";
                return false;
            }
        }
    }
}

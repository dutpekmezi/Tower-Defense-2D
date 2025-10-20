using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace dutpekmezi
{
    public static class SceneService
    {
        private static SceneServiceSettings _settings;
        private static readonly HashSet<string> _loadedSceneNames = new();
        private static bool _isInitialized;

        public static event Action<string> OnSceneLoaded;
        public static event Action<string> OnSceneUnloaded;

        // Called once to initialize the system with the settings asset
        public static void Initialize(SceneServiceSettings settings)
        {
            if (_isInitialized)
                return;

            if (settings == null)
            {
                Debug.LogError("[SceneService] Settings asset is missing!");
                return;
            }

            _settings = settings;
            _isInitialized = true;

            if (_settings.BaseSceneAsset == null)
            {
                Debug.LogError("[SceneService] Base scene reference is empty.");
                return;
            }

            string baseSceneName = _settings.BaseSceneAsset.name;

            // Always load the base scene as the main scene
            SceneManager.LoadScene(baseSceneName, LoadSceneMode.Single);
            _loadedSceneNames.Clear();
            _loadedSceneNames.Add(baseSceneName);

#if UNITY_EDITOR
            // If test mode is enabled, go directly to the test scene.
            if (_settings.TestMode && _settings.TestSceneAsset != null)
            {
                Load(_settings.TestSceneAsset);
                return;
            }
#endif

            // Otherwise, load the first listed scene
            if (_settings.SceneAssets != null && _settings.SceneAssets.Count > 0 && _settings.SceneAssets[0] != null)
            {
                Load(_settings.SceneAssets[0]);
            }

            Debug.Log($"[SceneService] Initialized with base scene: {baseSceneName}");
        }

        // Loads a scene additively on top of the base scene
        public static void Load(SceneAsset sceneAsset, Action onLoaded = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[SceneService] Cannot load scene before Initialize() is called.");
                return;
            }

            if (sceneAsset == null)
            {
                Debug.LogWarning("[SceneService] Load() called with a null scene reference.");
                return;
            }

            string sceneName = sceneAsset.name;

            if (_loadedSceneNames.Contains(sceneName))
            {
                Debug.LogWarning($"[SceneService] '{sceneName}' is already loaded.");
                onLoaded?.Invoke();
                return;
            }

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            op.completed += _ =>
            {
                var loadedScene = SceneManager.GetSceneByName(sceneName);
                _loadedSceneNames.Add(sceneName);
                SceneManager.SetActiveScene(loadedScene);

                OnSceneLoaded?.Invoke(sceneName);
                onLoaded?.Invoke();

                Debug.Log($"[SceneService] Loaded scene: {sceneName}");
            };
        }

        // Unloads a scene if its currently loaded
        public static void Unload(SceneAsset sceneAsset, Action onUnloaded = null)
        {
            if (sceneAsset == null)
            {
                Debug.LogWarning("[SceneService] Unload() called with a null scene reference.");
                onUnloaded?.Invoke();
                return;
            }

            string sceneName = sceneAsset.name;

            if (!_loadedSceneNames.Contains(sceneName))
            {
                Debug.LogWarning($"[SceneService] '{sceneName}' is not currently loaded.");
                onUnloaded?.Invoke();
                return;
            }

            var op = SceneManager.UnloadSceneAsync(sceneName);
            op.completed += _ =>
            {
                _loadedSceneNames.Remove(sceneName);
                OnSceneUnloaded?.Invoke(sceneName);
                onUnloaded?.Invoke();

                Debug.Log($"[SceneService] Unloaded scene: {sceneName}");
            };
        }

        // Closes all additive scenes, leaving only the base scene active
        public static void UnloadAllAdditives()
        {
            foreach (string sceneName in new List<string>(_loadedSceneNames))
            {
                if (_settings.BaseSceneAsset != null && sceneName == _settings.BaseSceneAsset.name)
                    continue;

                var op = SceneManager.UnloadSceneAsync(sceneName);
                op.completed += _ =>
                {
                    _loadedSceneNames.Remove(sceneName);
                    OnSceneUnloaded?.Invoke(sceneName);
                    Debug.Log($"[SceneService] Unloaded additive scene: {sceneName}");
                };
            }
        }

        // Checks if a specific scene is currently loaded in memory
        public static bool IsSceneLoaded(SceneAsset sceneAsset)
        {
            if (sceneAsset == null)
                return false;

            string name = sceneAsset.name;
            var scene = SceneManager.GetSceneByName(name);
            return scene.IsValid() && scene.isLoaded;
        }
    }
}

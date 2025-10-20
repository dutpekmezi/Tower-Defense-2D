using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace dutpekmezi
{
    [CreateAssetMenu(
        fileName = "SceneServiceSettings",
        menuName = "Game/Scriptable Objects/Services/Scene Service/SceneServiceSettings")]
    public class SceneServiceSettings : ScriptableObject
    {
        [Header("Base Scene")]
        [Tooltip("Persistent base scene (always loaded)")]
        public SceneAsset BaseSceneAsset;

        [Header("Scenes")]
        [Tooltip("All additive scenes that can be loaded after the base scene")]
        public List<SceneAsset> SceneAssets;

        [Header("Testing Mode")]
        [Tooltip("If enabled, the game will load the test scene directly.")]
        public bool TestMode = false;
        public SceneAsset TestSceneAsset;

        public Scene GetSceneFromAsset(SceneAsset asset)
        {
            if (asset == null)
                return default;

            // Scene name is the asset name
            string sceneName = asset.name;
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene;
        }

        public Scene GetBaseScene() => GetSceneFromAsset(BaseSceneAsset);

        public List<Scene> GetAllScenes()
        {
            var list = new List<Scene>();
            foreach (var asset in SceneAssets)
            {
                list.Add(GetSceneFromAsset(asset));
            }
            return list;
        }

        public Scene GetTestScene() => GetSceneFromAsset(TestSceneAsset);
    }
}

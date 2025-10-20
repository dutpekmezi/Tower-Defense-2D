using dutpekmezi;
using UnityEngine;

public class Boot : MonoBehaviour
{
    [SerializeField] private SceneServiceSettings settings;

    private void Awake()
    {
        SceneService.Initialize(settings);
    }
}
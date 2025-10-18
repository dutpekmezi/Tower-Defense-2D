using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Game/Scriptable Objects/Entity/EntityData")]
public class EntityData : ScriptableObject
{
    public string Id;
    public string Name;

    public int MaxHealth;
    public float AttackDamage;

    public GameObject Prefab;
}

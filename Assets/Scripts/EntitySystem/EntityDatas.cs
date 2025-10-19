using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace dutpekmezi
{
    [CreateAssetMenu(fileName = "EntityDatas", menuName = "Game/Scriptable Objects/Entity/EntityDatas")]
    public class EntityDatas : ScriptableObject
    {
        public List<EntityData> Entites;
    }
}

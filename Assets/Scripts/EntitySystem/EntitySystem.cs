using System.Collections.Generic;
using UnityEngine;

namespace dutpekmezi
{
    /// <summary>
    /// Global entity manager that provides runtime access to entity data.
    /// Handles lookup, caching and validation for EntityData assets.
    /// </summary>
    public class EntitySystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EntityDatas entityDatas;

        private static EntitySystem instance;
        public static EntitySystem Instance => instance;

        // Internal lookup cache for faster access
        private readonly Dictionary<string, EntityData> entityCache = new Dictionary<string, EntityData>();

        public EntityDatas EntityDatas => entityDatas;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            BuildCache();
        }

        /// <summary>
        /// Builds a dictionary cache for faster lookup by Id.
        /// </summary>
        private void BuildCache()
        {
            entityCache.Clear();

            if (entityDatas == null || entityDatas.Entites == null)
            {
                Debug.LogWarning("[EntitySystem] No entity data list assigned.");
                return;
            }

            foreach (var data in entityDatas.Entites)
            {
                if (data == null || string.IsNullOrEmpty(data.Id))
                {
                    Debug.LogWarning("[EntitySystem] Found null or invalid EntityData entry.");
                    continue;
                }

                if (entityCache.ContainsKey(data.Id))
                {
                    Debug.LogWarning($"[EntitySystem] Duplicate Entity Id detected: {data.Id}");
                    continue;
                }

                entityCache.Add(data.Id, data);
            }

            Debug.Log($"[EntitySystem] Cached {entityCache.Count} entities successfully.");
        }

        /// <summary>
        /// Retrieves an EntityData by its unique Id.
        /// Returns null if not found.
        /// </summary>
        public EntityData GetEntityById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            entityCache.TryGetValue(id, out var entity);
            return entity;
        }

        /// <summary>
        /// Returns a random entity from the dataset (useful for testing or spawning).
        /// </summary>
        public EntityData GetRandomEntity()
        {
            if (entityDatas == null || entityDatas.Entites == null || entityDatas.Entites.Count == 0)
                return null;

            int index = Random.Range(0, entityDatas.Entites.Count);
            return entityDatas.Entites[index];
        }

        /// <summary>
        /// Returns all registered EntityData objects.
        /// </summary>
        public IReadOnlyList<EntityData> GetAllEntities() => entityDatas?.Entites;
    }
}

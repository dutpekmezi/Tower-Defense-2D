using UnityEngine;

namespace dutpekmezi
{
    public class EntityCardsManager : MonoBehaviour
    {
        [SerializeField] private EntityCard entityCardPrefab;

        private void Start()
        {

        }

        private void DisplayEntityCards()
        {
            if (!entityCardPrefab) return;

            var displayingEntityCards = EntitySystem.Instance.GetAllEntities();

            foreach (var entity in displayingEntityCards)
            {
                var instance = Instantiate(entityCardPrefab);
                instance.Init(entity);
            }
        }
    }
}
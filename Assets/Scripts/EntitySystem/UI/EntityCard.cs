using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dutpekmezi
{
    public class EntityCard : MonoBehaviour
    {
        [SerializeField] private Image entityImage;

        [SerializeField] private TextMeshProUGUI entityNameText;

        private EntityData entityData;

        public void Init(EntityData data)
        {
            entityData = data;

            entityImage.sprite = data.Sprite;

            entityNameText.tag = data.Name;
        }
    }
}
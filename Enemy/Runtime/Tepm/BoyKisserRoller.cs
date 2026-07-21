using UnityEngine;

namespace ZigdarkS.ProjectB.Enemy.Tepm
{
    public class BoyKisserRoller : MonoBehaviour
    {
        [Header("Список материалов для выбора")]
        [SerializeField] private Material[] materials;

        private void Start()
        {
            if (!TryGetComponent<Renderer>(out var meshRenderer))
            {
                Debug.LogWarning($"[{gameObject.name}] BoyKisserRoller: Компонент Renderer не найден на объекте!", this);
                return;
            }

            if (materials == null || materials.Length == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] BoyKisserRoller: Список материалов пуст или не назначен в Инспекторе!", this);
                return;
            }

            int randomIndex = Random.Range(0, materials.Length);
            Material selectedMaterial = materials[randomIndex];

            if (selectedMaterial != null)
            {
                meshRenderer.material = selectedMaterial;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] BoyKisserRoller: Элемент под индексом {randomIndex} в списке материалов равен null!", this);
            }
        }
    }
}

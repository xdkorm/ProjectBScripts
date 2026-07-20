using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [SerializeField] private float _lifetime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }
}
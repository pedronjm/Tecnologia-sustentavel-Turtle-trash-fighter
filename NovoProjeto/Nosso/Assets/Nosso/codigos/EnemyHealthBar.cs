using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Referências")]
    public Damageable damageable;
    public Image fillImage;
    public Transform lookAtCamera;

    void Awake()
    {
        if (damageable != null)
        {
            damageable.OnHealthChanged.AddListener(UpdateBar);
        }
    }

    void OnDestroy()
    {
        if (damageable != null)
        {
            damageable.OnHealthChanged.RemoveListener(UpdateBar);
        }
    }

    void LateUpdate()
    {
        if (lookAtCamera != null)
        {
            transform.forward = lookAtCamera.forward;
        }
    }

    private void UpdateBar(float value01)
    {
        if (fillImage != null)
            fillImage.fillAmount = value01;
    }
}


using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("Referências")]
    public Damageable playerDamageable;
    public Image fillImage;
    public TMP_Text currentHealthText;
    public TMP_Text maxHealthText;

    void Awake()
    {
        if (playerDamageable != null)
        {
            playerDamageable.OnHealthChanged.AddListener(UpdateBar);
        }
    }

    void Start()
    {
        // Garante que a UI começa com o valor correto mesmo se o evento já tiver sido disparado
        if (playerDamageable != null)
        {
            float t = playerDamageable.maxHealth > 0
                ? (float)playerDamageable.currentHealth / playerDamageable.maxHealth
                : 0f;
            UpdateBar(t);
        }
    }

    void OnDestroy()
    {
        if (playerDamageable != null)
        {
            playerDamageable.OnHealthChanged.RemoveListener(UpdateBar);
        }
    }

    private void UpdateBar(float value01)
    {
        if (fillImage != null)
            fillImage.fillAmount = value01;

        if (playerDamageable != null)
        {
            if (currentHealthText != null)
                currentHealthText.text = playerDamageable.currentHealth.ToString();

            if (maxHealthText != null)
                maxHealthText.text = playerDamageable.maxHealth.ToString();
        }
    }
}


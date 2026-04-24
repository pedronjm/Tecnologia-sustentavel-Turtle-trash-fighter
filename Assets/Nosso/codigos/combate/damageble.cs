using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Eventos")]
    // Envia um valor entre 0 e 1 (vida / maxVida) para barras de vida
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDie;

    // Opcional: usado por inimigos para animação e knockback
    private enemy inimigoScript;

    void Awake()
    {
        inimigoScript = GetComponent<enemy>();

        if (currentHealth <= 0)
            currentHealth = maxHealth;

        OnHealthChanged?.Invoke(1f);
    }

    public void TakeDamage(int damage, Vector2 attackPosition)
    {
        if (currentHealth <= 0)
            return;

        bool isEnemy = GetComponent<enemy>() != null;
        bool isPlayer = GetComponent<Player>() != null;

        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;

        float t = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        OnHealthChanged?.Invoke(t);

        if (currentHealth == 0)
        {
            if (isPlayer)
            {
                if (GameControler.instance != null)
                    GameControler.instance.death();
            }
            else if (isEnemy && GameControler.instance != null)
            {
                GameControler.instance.PlayEnemyDeathSound();
            }

            if (inimigoScript != null)
                inimigoScript.Die();

            OnDie?.Invoke();
        }
        else
        {
            if (isPlayer)
            {
                if (GameControler.instance != null)
                    GameControler.instance.PlayPlayerHitSound();
            }
            else if (isEnemy && GameControler.instance != null)
            {
                GameControler.instance.PlayEnemyHitSound();
            }

            if (inimigoScript != null)
                inimigoScript.ApplyKnockback(attackPosition);
        }
    }
}

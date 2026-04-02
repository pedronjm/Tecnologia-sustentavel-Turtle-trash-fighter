using UnityEngine;

public class Damageable : MonoBehaviour
{
    public int health = 1;
    private sacola inimigoScript;

    void Start()
    {
        inimigoScript = GetComponent<sacola>();
    }

    public void TakeDamage(int damage, Vector2 attackPosition)
    {
        health -= damage;

        if (health <= 0)
        {
            inimigoScript.Die();
        }
        else
        {
            // Opcional: Adicionar um coice (knockback) ao receber dano
            inimigoScript.ApplyKnockback(attackPosition);
        }
    }
}

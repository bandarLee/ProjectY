using UnityEngine;

namespace EnemyHealth
{

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public GameObject deathEffectPrefab;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage, current health: " + currentHealth);

        // Instantiate hit effect
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
            Destroy(hitEffect, 1f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffectPrefab != null)
        {
            GameObject deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(deathEffect, 2f);
        }

        Debug.Log(gameObject.name + " has been destroyed.");
        Destroy(gameObject);
    }
}
}
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    public float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Player died!");

        // TODO: Здесь можно добавить анимацию смерти,
        // экран проигрыша и перезапуск уровня.
    }
}


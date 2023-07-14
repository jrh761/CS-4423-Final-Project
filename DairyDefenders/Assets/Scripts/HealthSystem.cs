using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 100;
    public HealthBar healthBar;

    public int currentHealth { get; private set; }

    private void Awake()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // if (gameObject.tag == "Player")
        // {
        // }
        // else if (gameObject.tag == "Enemy")
        // {
        // }
        SceneManager.LoadScene("Menu");
    }
}
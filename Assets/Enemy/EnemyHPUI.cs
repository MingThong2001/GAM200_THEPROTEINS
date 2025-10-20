using UnityEngine;
using UnityEngine.UI;

//Controls enemy's health bar UI above each of the enemy's head.
//It will updates the health bar fill then follows the enemy on screen.
public class EnemyHPUI : MonoBehaviour
{
    [Header("Enemy Health")]
    public float maxHealth;
    private float currentHealth;

    [Header("HP Bar UI")]
    public Image fillbar;      // Assign the Image component of the HP bar
    public Vector3 hpOffset = new Vector3(0, 1f, 0); // Offset above the enemy

    private Transform targetEnemy; // Enemy to follow

    private void Awake()
    {
        currentHealth = maxHealth;

        // Default to parent as the enemy
        if (transform.parent != null)
        {
            targetEnemy = transform.parent;
        }
    }

    private void LateUpdate()
    {
        FollowEnemy();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHPBar();

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateHPBar()
    {
        if (fillbar != null)
            fillbar.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        Destroy(gameObject); // Only destroys HP bar, enemy remains
    }

    private void FollowEnemy()
    {
        if (targetEnemy != null)
        {
            transform.position = targetEnemy.position + hpOffset;
            transform.rotation = Quaternion.identity; // Keep upright
        }
    }

    public void SetHealth(float current, float max)
    {
        currentHealth = current;
        maxHealth = max;

        if (fillbar != null)
        { 
            fillbar.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }
}



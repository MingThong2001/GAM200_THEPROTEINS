using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class Enemy : MonoBehaviour
{

    [Header("Enemy Stats")]
    public float maxHealth = 20f;
    private float currentHealth;

    [Header("Health Bar Settings")]
    public GameObject healthbarPrefab;    // Assign prefab with Image for Fill
    public Vector3 healthbarOffset = new Vector3(0, 1f, 0);
    private Image fillBar;
    private GameObject healthBarInstance;

   

    private void Start()
    {
        currentHealth = maxHealth;
        CreateHealthBar();
        UpdateHealthBar();
    }

    private void CreateHealthBar()
    {
        if (healthbarPrefab == null) return;

        // Instantiate health bar at enemy position + offset
        healthBarInstance = Instantiate(healthbarPrefab, transform.position + healthbarOffset, Quaternion.identity);
        fillBar = healthBarInstance.transform.Find("Fill").GetComponent<Image>();

        if (fillBar == null)
            Debug.LogError("Health bar prefab needs an Image named 'Fill'");
    }

    private void UpdateHealthBar()
    {
        if (fillBar != null)
            fillBar.fillAmount = currentHealth / maxHealth;
    }

    private void LateUpdate()
    {
        if (healthBarInstance != null)
            healthBarInstance.transform.position = transform.position + healthbarOffset;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (!collision.collider.CompareTag("Player")) return;

    //    if (Time.time - lastAttackTime >= attackCD)
    //    {
    //        PlayerStats stats = collision.collider.GetComponent<PlayerStats>();
    //        if (stats == null)
    //        {
    //            stats = collision.collider.GetComponentInParent<PlayerStats>();
    //        }

    //        if (stats != null)
    //        {
    //            stats.TakeDamage(damageAmt);
    //            lastAttackTime = Time.time;
    //        }
    //    }
    //}

    private void Die()
    {
        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        Destroy(gameObject);
    }
}
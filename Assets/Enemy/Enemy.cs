using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 10f;
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {

            Die();
        }
    }

    private void Die()
    { 
        Destroy(gameObject);
    
    }
}

using UnityEngine;

public class SegmentProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 3f;

    public void Start()
    {
       
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //Deal Damage.
        }
    }
}

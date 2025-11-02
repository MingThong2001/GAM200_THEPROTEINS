using UnityEngine;

public class ProjectileSegment : MonoBehaviour
{
    /* This script is to handle collision for projectile*/


    //Reference to the parent projectile since projectile has multiple children (points).
    public  Projectile parentProjectile;

    //Flag for collision.
    private bool hasCollided = false;

    //Rigidbody2D
    private Rigidbody2D segmentRigidbody;
    private static int colliderHitCount = 0;

    public void Awake()
    {
        //Find the parent projectile this segment belongs to.
        parentProjectile = GetComponentInParent<Projectile>();

        segmentRigidbody = GetComponent<Rigidbody2D>();
    }

    //Reset segment 
    public void resetSegment()
    {
        hasCollided = false;
        segmentRigidbody.simulated = true;
        segmentRigidbody.bodyType = RigidbodyType2D.Dynamic; //Restore physics.
        segmentRigidbody.constraints = RigidbodyConstraints2D.None; //Remove any freeze constraints;

    }

    //Handle collision.
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !hasCollided) 
        {

            hasCollided = true;
            if (parentProjectile != null)
            {
                parentProjectile.HandleEnemyHit(collision);
                // Increment collider hit count for this projectile
                colliderHitCount++;
                Debug.Log($"[ProjectileSegment] Collider '{gameObject.name}' touched Enemy '{collision.gameObject.name}'. Total colliders touching enemy for this projectile: {colliderHitCount}");
            }
            else
            {
                Debug.LogError($"[ProjectileSegment] parentProjectile is NULL on '{gameObject.name}'!");
            }
            Debug.Log($"[ProjectileSegment] Collider '{gameObject.name}' touched Enemy '{collision.gameObject.name}'. Total colliders touching enemy for this projectile: {colliderHitCount}");
        }

    }
    
    public void OnCollisionEnter2D (Collision2D collision)
    {
        if (hasCollided)
        {
            return;
        }

        //if (collision.gameObject.CompareTag("Surface"))
        //{

        //    hasCollided = true;
        //    parentProjectile.SticktoSurface(collision);
        //}

     
        if (collision.gameObject.CompareTag("BreakableObjs"))
        {

            hasCollided = true;
            parentProjectile.HandleBreakableHit(collision);
        }
    }
}
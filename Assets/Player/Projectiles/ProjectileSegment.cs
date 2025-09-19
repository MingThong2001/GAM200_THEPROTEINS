using UnityEngine;

public class ProjectileSegment : MonoBehaviour
{
    //Reference to the parent projectile since projectile has multiple children (points).
    private Projectile parentProjectile;

    //Flag for collision.
    private bool hasCollided = false;

    //Rigidbody2D
    private Rigidbody2D segmentRigidbody;

    public void Start()
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
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasCollided)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Surface"))
        {

            hasCollided = true;
            parentProjectile.SticktoSurface(collision);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {

            hasCollided = true;
            parentProjectile.HandleEnemyHit(collision);
        }

        if (collision.gameObject.CompareTag("BreakableObjs"))
        {

            hasCollided = true;
            parentProjectile.HandleBreakableHit(collision);
        }
    }
}
using UnityEngine;

public class SBSegmentCollider : MonoBehaviour
{/*This script is not in use. It was for experimental purposes*/

    public SquishEnergy squishEnergy;
    private Rigidbody2D rb;


    //Bounce Settings
    public float bounceMultiplier = 1f;
    public float minibounceSpeed = 1.3f;
    private void Update()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (squishEnergy == null || collision.contactCount == 0) 
        {
            return;
        }
        ContactPoint2D contactPoint = collision.GetContact(0);
        Vector2 collisionNormal = contactPoint.normal;

        float impactSpeed = Vector2.Dot(collision.relativeVelocity, -collisionNormal);

        if (impactSpeed > minibounceSpeed)
        { 
            //float bounceForce = impactSpeed * squishEnergy.currentCharge * bounceMultiplier;
          //  rb.AddForce(collisionNormal * bounceForce, ForceMode2D.Impulse);
        }


        if (collision.gameObject.CompareTag("Breakable"))
        { 
            Destroy(collision.gameObject);
        }
    }


}

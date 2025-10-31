using System.Collections.Generic;
using UnityEngine;

public class MovingPlatforms : MonoBehaviour
{
    //Platform Points.
    public Transform pointA;
    public Transform pointB;

    //Platform Settings.
    public float moveSpeed = 2f;
    private Vector2 nextPosition;
    private Rigidbody2D rb;
    private Vector2 platformVelocity;

    //Track which segments on the platform.
    private HashSet<Rigidbody2D> objectsOnPlatform = new HashSet<Rigidbody2D>();

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; //Platform is moved manually not physics so it does not interfere with our softbody physics
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        nextPosition = pointB.position; //Point A move to point b.
    }

    public void FixedUpdate()
    {
        Vector2 oldPos = rb.position; //Save current platform posiition.

        //Calculate the platform movement (move to target position).
        Vector2 newPos = Vector2.MoveTowards(rb.position, nextPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        //calculate the movmeent.
        Vector2 platformMovement = newPos - oldPos;

        //Move all the game object rb
        foreach (Rigidbody2D objRb in objectsOnPlatform)
        {
            if (objRb != null && objRb.bodyType != RigidbodyType2D.Kinematic)
            {
                //Move it by  movement.
                objRb.MovePosition(objRb.position + platformMovement);
            }
        }

        //Swapped target when reached the destination.
        if (Vector2.Distance(rb.position, nextPosition) <= 0.01f)
        {
            nextPosition = (nextPosition == (Vector2)pointA.position)
              ? pointB.position
              : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Detect if there is anything ontop of the object.
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If the contact normal points down, object is on top.
            if (contact.normal.y < -0.5f)
            {
                Rigidbody2D objRb = collision.rigidbody;
                if (objRb != null)
                {
                    objectsOnPlatform.Add(objRb); //Add the obect to the hash set.
                }
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //Remove when object leave the platform.
        Rigidbody2D objRb = collision.rigidbody;
        if (objRb != null)
        {
            objectsOnPlatform.Remove(objRb);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //Change to false when nth is on top.
        bool isOnTop = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                isOnTop = true;
                break;
            }
        }

        //If nothing is on top, we remove the obj.
        if (!isOnTop)
        {
            Rigidbody2D objRb = collision.rigidbody;
            if (objRb != null)
            {
                objectsOnPlatform.Remove(objRb);
            }
        }
    }
}
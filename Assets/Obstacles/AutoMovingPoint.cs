using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoMovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;

    private Vector2 nextPosition;
    private Rigidbody2D rb;

    // Track objects standing on the platform
    private HashSet<Rigidbody2D> objectsOnPlatform = new HashSet<Rigidbody2D>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        nextPosition = pointB.position; // Start moving towards B
    }

    void FixedUpdate()
    {
        Vector2 oldPos = rb.position;

        // Move platform
        Vector2 newPos = Vector2.MoveTowards(rb.position, nextPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Calculate movement delta
        Vector2 platformMovement = newPos - oldPos;

        // Move objects riding the platform
        foreach (Rigidbody2D objRb in objectsOnPlatform)
        {
            if (objRb != null && objRb.bodyType != RigidbodyType2D.Kinematic)
            {
                objRb.MovePosition(objRb.position + platformMovement);
            }
        }

        // Swap target when reaching destination
        if (Vector2.Distance(rb.position, nextPosition) <= 0.01f)
        {
            nextPosition = (nextPosition == (Vector2)pointA.position)
                ? pointB.position
                : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Object is on top if normal points down
            if (contact.normal.y < -0.5f)
            {
                Rigidbody2D objRb = collision.rigidbody;
                if (objRb != null)
                {
                    objectsOnPlatform.Add(objRb);
                }
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D objRb = collision.rigidbody;
        if (objRb != null)
        {
            objectsOnPlatform.Remove(objRb);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        bool isOnTop = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                isOnTop = true;
                break;
            }
        }

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

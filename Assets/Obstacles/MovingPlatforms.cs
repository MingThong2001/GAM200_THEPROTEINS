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

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        nextPosition = pointB.position;
    }

    public void FixedUpdate()
    {
        //transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

        //if (transform.position == nextPosition)
        //{ 
        //    nextPosition = (nextPosition == pointA.position) ? pointB.position : pointA.position;   
        //}

        Vector2 newPos = Vector2.MoveTowards(rb.position, nextPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        //Swap target when reaching destination
        if (Vector2.Distance(rb.position, nextPosition) <= 0.01f)
        {
            nextPosition = (nextPosition == (Vector2)pointA.position)
              ? pointB.position
              : pointA.position;
        }


    }

}

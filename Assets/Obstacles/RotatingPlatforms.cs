using UnityEngine;

public class RotatingPlatforms : MonoBehaviour
{
    //Rotation Setting
    private float rotationSpeed = 10f;
    private bool clockwise = false;

    private Rigidbody2D rb;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();


    }

    public void FixedUpdate()
    {
        float direction = clockwise ? -1f: 1f;

        if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
        { 
            rb.MoveRotation(rb.rotation + rotationSpeed * direction * Time.fixedDeltaTime);
        }
        else
        {
            transform.Rotate(0f, 0f, rotationSpeed * direction * Time.deltaTime);

        }
    }
}

using UnityEngine;

public class SegmentProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 3f;
    public float maxDistance = 5f;
    public float speed = 10f;
    public Transform returnPoints;
    public Transform firePoint;


    private Rigidbody2D rb;
    private bool isReturning = false;
    private bool hasHit = false;
    private Vector2 startPos;
    private Vector2 targetPos;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        startPos = firePoint.position;
        
       //Set Initial target based on cursor
       Vector2 mouseworldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPos = (mouseworldPos - (Vector2)firePoint.position).normalized * maxDistance *Time.deltaTime;
       Destroy(gameObject, lifetime);
        
    }

    public void Update()
    {
        if (hasHit)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            return;
        }
        if (!isReturning)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, speed * Time.deltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(rb.position, targetPos) < 0.05f)
            {
                isReturning = false;
            }

        }
        else
        { 
            //Move back to the return point.
            Vector2 newPos = Vector2.MoveTowards(rb.position, returnPoints.position, speed * Time.deltaTime);
            rb.MovePosition(newPos);
            if (Vector2.Distance((Vector2)rb.position, returnPoints.position) < 0.05f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Fire()
    {
        if (firePoint != null)
        { 
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 direction = (mouseWorldPosition - (Vector2)(firePoint.position).normalized);

            rb.AddForce(direction * speed,ForceMode2D.Impulse);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            hasHit = true;
        }
        else if (collision.gameObject.CompareTag("BreakableObjects"))
        {
            hasHit = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.MovePosition(collision.contacts[0].point);
        }
    }
}

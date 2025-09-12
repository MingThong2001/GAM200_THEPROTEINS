using UnityEngine;

public class SegmentProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 3f;
    public float speed = 10f;
    public Transform returnPoints;
    public Transform firePoint;


    private Rigidbody2D rb;
    private bool isReturning = false;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime );
        
    }

    public void Update()
    {
        Vector2 currentP = transform.position;
        Vector2 targetP = returnPoints.position;

        Vector2 newPosition = Vector2.MoveTowards( currentP, targetP, speed * Time.deltaTime );
        rb.MovePosition( newPosition );

        if (Vector2.Distance(transform.position, returnPoints.position) < 0.1f)
        {
            isReturning = false;
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
            //Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            /* if (enemy != null)
             * {
             * enemy.TakeDamage(damageAmount):
             *} */
        }
    }
}

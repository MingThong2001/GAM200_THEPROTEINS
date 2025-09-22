using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class Projectile : MonoBehaviour
{

    //Projectile Settings
    public float speed = 10f;
    public float maxDistance;
    public int damage = 2;

    //Just in case.
    public Transform FirePoint;
    public GameObject segmentPrefab;

    private Vector3 startPosition; //To track max distance.
    private Vector3 moveDirection; //Normalized direction of travel.

    //References
    private Rigidbody2D[] bodyParts;
    private SpringJoint2D[] joints;
    private Collider2D[] colliders; 
    private Transform projectileTransform;


    //Projectile States.
    private bool isActive = false;
    private bool isStuck = false;
    private float stickTime = 0f;
    private float stickDuration = 5f;


    private void OnEnable()
    {
        //Record initial position when activated.
        startPosition = transform.position;
        Debug.Log($"Projectile fired at position: {startPosition}");

        //Initialize all the body parts and joints.
        initializeProjectile();

    }
   
    public void initializeProjectile()
    {

        bodyParts = GetComponentsInChildren<Rigidbody2D>();
        joints = GetComponentsInChildren<SpringJoint2D>();
        colliders = GetComponentsInChildren<Collider2D>();

        //Make sure all segements are active physics objects.
        foreach (Rigidbody2D rb in bodyParts)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 0f; //Gravity setting but can be remove. Include this for debugging previously.
        }

        //Reactivate all the springJoints.
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i].enabled = true;
        }

        isActive = true;
    }
    public void Update()
    {

        //If projectile is flying and active.
        if (isActive && !isStuck)
        {
            //Track how far it travelled.
            float distancetravelled = Vector3.Distance(startPosition, transform.position);
          //  Debug.Log($"Distance travelled: {distancetravelled} / Max distance: {maxDistance}");
           // Debug.Log($"Current position: {transform.position}, Velocity: {bodyParts[0].linearVelocity}");

            if (distancetravelled > maxDistance) //If it exceed max ranged, we return to the pool.
            {
                returntoprojectilePool();
            }
        }
        else if (isStuck) //If the projectile sticks, check the stick duration.
        {
            if (Time.time - stickTime >= stickDuration)
            {
                returntoprojectilePool();
            }
        }

    }

    //Set projectile's direction and applies initial force.
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;

        //Rotate projectile to match movement direction.
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        if (bodyParts.Length > 0) //Apply force to the firstbody segment.
        { 
            Rigidbody2D body = bodyParts[0];
            body.AddForce(moveDirection * speed, ForceMode2D.Impulse);
            Debug.Log("Projectile velocity: " + body.linearVelocity);

        }
    }

    //Collision with tagged objects.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Surface"))
        {
            SticktoSurface(collision);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision);
        }
        else if (collision.gameObject.CompareTag("BreakableObjs"))
        { 
            HandleBreakableHit(collision);
        }
    }

    //Stick to system 
    public void SticktoSurface(Collision2D collision)
    {
        Debug.Log("SticktoSurface called");
        Debug.Log($"Collision object: {collision.gameObject.name}");
        Debug.Log($"Contact points: {collision.contactCount}");
        isStuck = true;
        isActive = false;
        stickTime = Time.time;

        //Align projectile with the surface.
        ContactPoint2D contactPoint = collision.GetContact(0);
        Vector2 normal = contactPoint.normal;
        float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        transform.position = contactPoint.point;

        //Disable physics for all body parts then attach them to the surface.
        foreach (Rigidbody2D body in bodyParts)
        {
            Collider2D coll = body.GetComponent<Collider2D>();
            if (coll != null)
            {   
                body.linearVelocity = Vector2.zero;
               
                Physics2D.IgnoreCollision(coll, collision.collider, true);
                body.bodyType = RigidbodyType2D.Kinematic;
                body.transform.SetParent(collision.collider.transform);
            }


            ////Store all contact points
            //List <Vector2> points = new List<Vector2>();
            //for (int i = 0; i < collision.contacts.Length; i++)
            //{
            //     points.Add(collision.contacts[i].point); 
            //}

            // foreach (Rigidbody2D body in bodyParts)
            // {
            //     bool anchorthis = false;

            //     foreach (Vector2 spoints in points)
            //     {
            //         float distance = Vector2.Distance(body.position, spoints);
            //         if (distance < 0.5f)
            //         {
            //             anchorthis = true;
            //             break;
            //         }
            //     }

            //     if (anchorthis)
            //     {

            //         body.bodyType = RigidbodyType2D.Kinematic;
            //         body.transform.SetParent(collision.collider.transform);

            //     }
            //     else
            //     {

            //         Physics2D.IgnoreCollision(body.GetComponent<Collider2D>(), collision.collider, true);
            //     }
            // }

            //for (int i = 0; i < bodyParts.Length; i++) 
            //{
            //    Rigidbody2D body = bodyParts[i];    
            //    float ditancetoHit = Vector2.Distance(body.position, hitPoint);
            //    if (ditancetoHit < 1.5f)
            //    { 
            //       segmetnstoStick.Add(body);   
            //    }
            //}
            //Debug.Log($"Closest segment found: {(segmetnstoStick != null ? "YES" : "NO")}");

            //if (segmetnstoStick.Count > 0)
            //{
            //    for (int i = 0; i < segmetnstoStick.Count; i++)
            //    { 
            //        Rigidbody2D segment = segmetnstoStick[i];

            //        if (segment.bodyType != RigidbodyType2D.Kinematic)
            //        {
            //            Debug.Log("Making segment kinematic and parenting...");
            //            segment.bodyType = RigidbodyType2D.Kinematic;
            //            segment.transform.position = hitPoint;
            //            segment.transform.SetParent(collision.collider.transform);
            //            Debug.Log($"Segment final position: {segment.transform.position}");

            //        }
            //    }
            //}
            //else
            //{
            //    Debug.Log("ERROR: No closest segment found!");
            //}


            //Disable joints so the projectile freezes in place.
            for (int i = 0; i < joints.Length; i++)
            {
                SpringJoint2D joint = joints[i];
                joint.enabled = false;

            }


        }
    }
    
    //Enemy Collision.
    public void HandleEnemyHit(Collision2D collision)
    {
        GameObject targetPoint = collision.gameObject;
        Enemy enemy = targetPoint.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        returntoprojectilePool();
    }

    //Breakable Object Collision.
    public void HandleBreakableHit(Collision2D collision)
    {
        BreakableObj breakable = collision.gameObject.GetComponent<BreakableObj>();
        if (breakable != null)
        {
            breakable.TakeDamage(damage);
        }
        returntoprojectilePool();
    }

    //Reset project states.
    public void ResetProjectile()
    {

        //Reset transform positions.
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;   

        //Reset root RigidBody.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.simulated = true;
        rb.constraints = RigidbodyConstraints2D.None;

        //Re-enable springjoints.
        foreach (SpringJoint2D joint2D in joints)
        {
            joint2D.enabled = true;
        }
        isActive = false;
        isStuck = false;
    }

    //Return projectile to the pool after resetting it.
    private void returntoprojectilePool()
    {
        ResetProjectile();
        // Return the projectile to the pool
        ProjectilePool.Instance.ReturnProjectile(this);
        Debug.Log("Projectile returning to pool at time: " + Time.time);

    }
}

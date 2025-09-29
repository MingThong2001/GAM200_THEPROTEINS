using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class Projectile : MonoBehaviour
{

    //Projectile Settings
    public float speed = 0.2f;
    public float maxDistance;
    public int damage = 2;
    public float delayTime = 0.5f;
    public float fireTime;
    public bool ignorePlayer = false;

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


    //Pickup Settings.
    public bool canBeCollected = false;

    public AudioClip pickupSound;
    public int segmentAmount = 1;

    //References.
    private Collider2D pickupCollider;
    public Projectile projectilePrefab;


    //Projectile States.
    private bool isActive = false;
    private bool isStuck = false;
    private float stickTime = 0f;
    private float stickDuration = 5f;
    private bool Hashit = false;

    //References
    private ProjectillePickup pickup;

    private void Awake()
    {
        //Reset Pickup
        pickup = GetComponent<ProjectillePickup>();
        if (pickup == null)
        {
            pickup = GetComponentInParent<ProjectillePickup>();
        }
        if (pickup != null)
        {
            pickup.ResetPickup();
        }
        else
        {
            Debug.LogError("Projectile: No ProjectillePickup found on this projectile or its parent!");
        }
    }
    private void OnEnable()
    {
        //Record initial position when activated.
        startPosition = transform.position;
        Debug.Log($"Projectile fired at position: {startPosition}");

        pickup = GetComponentInChildren<ProjectillePickup>(true);
        Debug.Log(pickup != null ? "Pickup found!" : "Pickup NOT found!");

        if (pickup == null)
            Debug.LogError("Projectile: No ProjectillePickup found on this projectile or its parent/children!");

        //Initialize all the body parts and joints.
        initializeProjectile();
        SwitchTag(gameObject, "Player");
        canBeCollected = false;
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
            rb.gravityScale = 0.8f;
            rb.linearDamping = 1f;
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
        //if (isActive && Time.time - fireTime >= delayTime && gameObject.tag != "Projectile")
        //{
        //    SwitchTag(gameObject, "Projectile");
        //}


        //If projectile is flying and active.
        if (isActive && !isStuck)
        {
            //Track how far it travelled.
            float distancetravelled = Vector3.Distance(startPosition, transform.position);
          //  Debug.Log($"Distance travelled: {distancetravelled} / Max distance: {maxDistance}");
           // Debug.Log($"Current position: {transform.position}, Velocity: {bodyParts[0].linearVelocity}");

            if (distancetravelled > maxDistance) //If it exceed max ranged, we return to the pool.
            {
                MakeCollectible();
            }
        }
        else if (isStuck) //If the projectile sticks, check the stick duration.
        {
            if (Time.time - stickTime >= stickDuration)
            {
               
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

        foreach (Rigidbody2D rb in bodyParts)
        {
       
            Rigidbody2D rootrb = bodyParts[0];
            //rootrb.linearDamping = 7f;
            //rootrb.AddForce(moveDirection * speed,ForceMode2D.Impulse);
            rootrb.AddForce(transform.up * 1000f);

            //Throw a spin for realism.
            rootrb.AddTorque(Random.Range(-0.5f,2f),ForceMode2D.Impulse);

        }
        fireTime = Time.time;

        //SwitchTag(gameObject, "Player");
        //pickup?.EnableCollection();
    }

    //Collision with tagged objects.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("groundLayer"))
        {
            //SticktoSurface(collision);
            Land();
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision);
        }
        //else if (collision.gameObject.CompareTag("BreakableObjs"))
        //{ 
        //    HandleBreakableHit(collision);
        //}
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
        if (Hashit) return;
        Hashit = true;

        GameObject targetPoint = collision.gameObject;
        Enemy enemy = targetPoint.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        //returntoprojectilePool();
    }

    //Breakable Object Collision.
    public void HandleBreakableHit(Collision2D collision)
    {
        if (Hashit) return;
        Hashit = true;

        BreakableObj breakable = collision.gameObject.GetComponent<BreakableObj>();
        if (breakable != null)
        {
            breakable.TakeDamage(damage);
        }
        //returntoprojectilePool();
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

        if (pickup != null)
        {

           pickup.ResetPickup();
        }

        SwitchTag(gameObject, "Player");
        canBeCollected = false;
    }

    //Return projectile to the pool after resetting it.
    //public void returntoprojectilePool()
    //{
    //    ResetProjectile();
    //    // Return the projectile to the pool
    //    ProjectilePool.Instance.ReturnProjectile(this);
    //    Debug.Log("Projectile returning to pool at time: " + Time.time);

    //}


    private void Land()
    {
        Debug.Log("landed");
        if (!isActive) return;
        isActive = true;

        foreach (Rigidbody2D rb in bodyParts)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 1f;
            rb.linearDamping = 3f;     
           

            float randomJitter = Random.Range(0.25f, 0.30f);
            rb.AddForce(Vector2.up * randomJitter);
        }
        MakeCollectible();
    }


    #region Pickup System

    public void EnableCollection()
    {
        canBeCollected = true;
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
        Debug.Log($"ProjectilePickup: Collection ENABLED on {gameObject.name}");
        //   CheckOverlap();
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    Debug.Log($"ProjectilePickup: Trigger entered by {other.name} with tag {other.tag}");
    //    Debug.Log($"Can be collected: {canBeCollected}");

    //    if (!canBeCollected) return;

    //    if (other.CompareTag("Player"))
    //    {
    //        Debug.Log($"Collecting via segment: {gameObject.name}");
    //        MassSegment massSegment = other.GetComponentInParent<MassSegment>();
    //        if (massSegment != null)
    //        {
    //            Collect(massSegment);
    //        }
    //    }


    //}
    public void Collect(MassSegment collector)
    {
        if (!canBeCollected) return;
        Projectile projectilePrefab = GetComponentInParent<Projectile>();
        if (projectilePrefab != null)
        {
            collector.CollectProjectile(projectilePrefab);
            //canBeCollected = false;
            //if (pickupCollider != null)
            //{
            //    pickupCollider.isTrigger = false;
            //}
            //Destroy(projectilePrefab);
        }
    }
    public void ResetPickup()
    {

        canBeCollected = false;

        // Reset the pickup collider back to non-trigger
        //if (pickupCollider != null)
        //{
        //    pickupCollider.isTrigger = false;

        //}
    }
    #endregion
    public void MakeCollectible()
    {
        if (canBeCollected) return;
        canBeCollected = true;
        Debug.Log($"Projectile: MakeCollectible called on {gameObject.name} at time {Time.time}");

        SwitchTag(gameObject, "Player");
        Debug.Log($"Projectile: Calling EnableCollection on {pickup.gameObject.name}");

        if (pickup != null) 
        {
            pickup.EnableCollection();
        }
    }

    private void SwitchTag(GameObject obj, string newTag)
    {
        obj.tag = newTag;
        foreach (Transform t in obj.transform)
        {
            SwitchTag(t.gameObject, newTag);

        }
    }
    
}

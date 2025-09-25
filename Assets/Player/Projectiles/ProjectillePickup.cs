using UnityEngine;

public class ProjectillePickup : MonoBehaviour
{
    //Pickup Settings.
    public bool canBeCollected = false;

    public AudioClip pickupSound;
    public int segmentAmount = 1;

    //References.
    private Collider2D pickupCollider;
    public Projectile projectilePrefab;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider2D>();//?? GetComponentInParent<Collider2D>();
        //if (pickupCollider == null)
        //    pickupCollider = GetComponentInParent<Collider2D>();
        //if (pickupCollider == null)
        //    pickupCollider = GetComponentInChildren<Collider2D>();

        if (pickupCollider == null)
            Debug.LogError("ProjectilePickup: No Collider2D found on this GameObject or its parent/children!");
        else
            Debug.Log($"ProjectilePickup: Found Collider2D on {pickupCollider.gameObject.name}");

        projectilePrefab = GetComponentInParent<Projectile>();

    }

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

    //Incase somehow it overlaps.
    //private void CheckOverlap()
    //{
    //    if (pickupCollider == null) return;
    //    Collider2D[] hits = Physics2D.OverlapBoxAll(pickupCollider.bounds.center, pickupCollider.bounds.size, 0f);
    //    for (int i = 0; i < hits.Length; i++)
    //    {
    //        Collider2D hit = hits[i];
    //        if (hit.CompareTag("Player"))
    //        {
    //            MassSegment massSegment = hit.GetComponentInParent<MassSegment>();
    //            if (massSegment != null)
    //            {
    //                Collect(massSegment);
    //            }
    //        }
    //    }

    //}
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"ProjectilePickup: Trigger entered by {other.name} with tag {other.tag}");
        Debug.Log($"Can be collected: {canBeCollected}");

        if (!canBeCollected) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"Collecting via segment: {gameObject.name}");
            MassSegment massSegment = other.GetComponentInParent<MassSegment>();
            if (massSegment != null)
            {
                Collect(massSegment);
            }
        }


    }
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
}
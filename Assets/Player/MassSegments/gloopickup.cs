using UnityEngine;

public class gloopickup : MonoBehaviour
{
    public bool isAdditive = true;
    public int segmentAmount = 1;
    public AudioClip pickupSound;
    public bool isBeingProcessed = false;
    private Rigidbody2D rb;
    private void Start()
    {


        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        { 
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        
        }
        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        //if (player == null)
        //{
        //    return;
        //}
        Collider2D pickupcollider = GetComponent<Collider2D>();
        Debug.Log($"{gameObject.name} spawned. Collider: {pickupcollider}, IsTrigger={pickupcollider?.isTrigger}");

        if (pickupcollider != null && !pickupcollider.isTrigger) 
        {
            pickupcollider.isTrigger = true;
        }




        //Collider2D[] pickupcollider = GetComponentsInChildren<Collider2D>();
        //Collider2D[] playersegments = player.GetComponentsInChildren<Collider2D>();

        //for (int i = 0; i < pickupcollider.Length; i++)
        //{
        //    Collider2D pickupCol = pickupcollider[i];
        //    for (int j = 0; j < playersegments.Length; j++)
        //    {
        //        Collider2D playerSegment = playersegments[j];
        //        if (playerSegment != null)
        //        {
        //            Physics2D.IgnoreCollision(pickupCol, playerSegment);
        //        }
        //    }
        //}


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) return;

        if (collision.CompareTag("Player"))
        {

        }
    }
   
    public void CollectPickup()
    {
        Debug.Log("Collecting pickup");
        if (pickupSound != null)
        { 
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            Debug.Log("Destroying obj: " + gameObject.name);

        }
        Destroy(gameObject);

    }

}
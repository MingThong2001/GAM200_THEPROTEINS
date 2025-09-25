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
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
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
            //MassSegment massSeg = collision.GetComponent<MassSegment>();
            //if (massSeg != null)
            //{
            //    massSeg.AddSegment(segmentAmount);
            //    CollectPickup();
            //}
        }
    }
    //public void OnTriggerEnter2D(Collider2D other)
    //{
    //    Debug.Log("Something entered the trigger area!");

    //    Debug.Log("Pickup Triggered: " + other.gameObject.name);
    //    foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
    //    {

    //        if (other.CompareTag("GlooSegment"))
    //        {
    //            Debug.DrawLine(transform.position, other.transform.position, Color.red, 1f);
    //            MassSegment massSegment = other.GetComponent<MassSegment>();

    //            Debug.Log("Processing Pickup...");

    //            if (massSegment != null && !isBeingProcessed)
    //            {
    //                isBeingProcessed = true;
    //                if (isAdditive && massSegment.canaddSegment())
    //                {
    //                    Debug.Log("Adding Segment");

    //                    massSegment.AddSegment(segmentAmount);
    //                    CollectPickup();
    //                }
    //                else if (!isAdditive && massSegment.canremoveSegment())
    //                {
    //                    Debug.Log("Removing Segment");

    //                    massSegment.RemoveSegment(segmentAmount);
    //                    CollectPickup();
    //                }

    //            }
    //            break;
    //        }
    //    }
    //}
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
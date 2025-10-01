using UnityEngine;

public class PickupDetector : MonoBehaviour
{
    private MassSegment parentMassSegment;

    public void Start()
    {
        parentMassSegment = GetComponentInParent<MassSegment>();

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"PickupDetector triggered by: {collision.gameObject.name}, tag: {collision.tag}");

        
        if (collision.CompareTag("Player"))
        {
            gloopickup pickup = collision.attachedRigidbody.GetComponentInParent<gloopickup>();
            //Debug.Log("Picupdetector works");
            if (pickup.isBeingProcessed == false)
            {
                pickup.isBeingProcessed = true;
                parentMassSegment.handlepickupCollision(collision);
            }
        }
            
    }
}



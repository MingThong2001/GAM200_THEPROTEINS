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
        Debug.Log("PickupDetector triggered by: " + collision.gameObject.name);

        if (collision.CompareTag("GlooSegment"))
        {
            parentMassSegment.handlepickupCollision(collision);
        }
    }
}

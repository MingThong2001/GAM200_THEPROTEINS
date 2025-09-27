using UnityEngine;

public class DoorButton : MonoBehaviour
{
    public Door connectedDoor;
    public bool hasbeenPressed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Button triggered by: " + other.name);

        if (!hasbeenPressed && other.GetComponentInParent<PlayerMovement>() != null )
        { 
            hasbeenPressed = true;
            Debug.Log("Button Pressed");
            connectedDoor.OpenDoor();
        }
    }
    
}

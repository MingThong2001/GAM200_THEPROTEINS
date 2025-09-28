using UnityEngine;

public class DoorButton : MonoBehaviour
{
    public Door connectedDoor;
    public bool hasbeenPressed = false;
    private Color pressedColor = Color.green;
    private Color unpressedColor = Color.red;
  
    private SpriteRenderer buttonRender;


    public void ResetButton()
    {
        hasbeenPressed = false;
        if (buttonRender != null)
        { 
            buttonRender.color = unpressedColor;
        }
    }
    private void Start()
    {
        buttonRender = GetComponent<SpriteRenderer>();

        if (buttonRender != null)
        {
            buttonRender.color = unpressedColor;
        }
      
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Button triggered by: " + other.name);

        if (!hasbeenPressed && other.GetComponentInParent<PlayerMovement>() != null)
        {
            hasbeenPressed = true;
            if (buttonRender != null)
            {
                buttonRender.color = pressedColor;

                Debug.Log("Button Pressed");
                connectedDoor.UnlockedDoor();
            }
        }
    }
}

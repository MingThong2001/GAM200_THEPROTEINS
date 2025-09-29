using TMPro; 
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    //Reference.
    public Door connectedDoor;

    //Track whether the button has been pressed.
    public bool hasbeenPressed = false;

    //Color Setting for visual feedback.
    private Color pressedColor = Color.green;
    private Color unpressedColor = new Color(186f / 255f, 79f / 255f, 97f / 255f);
  

    private SpriteRenderer buttonRender;
    public TextMeshProUGUI messageText;
    public string messageToShow = "Press W to leave the level";

    //Reset the button to its unpressed state.
    public void ResetButton()
    {
        hasbeenPressed = false;
        if (buttonRender != null)
        {
            buttonRender.color = unpressedColor;
        }
        if (messageText != null)
        {
            messageText.text = "";
        }

    }

    //Initialize component.
    private void Start()
    {
        buttonRender = GetComponent<SpriteRenderer>();

        if (buttonRender != null)
        {
            buttonRender.color = unpressedColor;
        }
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    //Triggered when collider enter the trigger area.
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Button triggered by: " + other.name);

        //React when the button hasn't been pressed and the object enter is the player.
        if (!hasbeenPressed && other.GetComponentInParent<PlayerMovement>() != null)
        {
            hasbeenPressed = true;
            if (buttonRender != null)
            {
                buttonRender.color = pressedColor;

                Debug.Log("Button Pressed");
                connectedDoor.UnlockedDoor();
            }
            if (messageText != null)
            {
                messageText.text = messageToShow;
            }
        }
    }
}

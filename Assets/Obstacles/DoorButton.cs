using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections.Generic;


public class DoorButton : MonoBehaviour
{
    //Reference.
    public Door connectedDoor;
    public MovingPlatforms platforms;
    public float requiredMass = 0.2f;


    //Object to open the door:
    public GameObject grabblebox;

    public float holdDuration = 5f; //How long the required mass must stay on the button.
    public float holdTimer = 0f; //Track how long mass has been on the button.

    //To reactivate the door
    public float lockbacktime = 2f; //Dekay before the button reset after the mass has been removed.
    public float locktimer = 0f; //Timer used for the reset delay.

    //Track whether the button has been pressed.
    public bool hasbeenPressed = false;
    public bool triggerDoor = false;
    public bool triggerPlatform = false;

    //Color Setting for visual feedback.
    private Color pressedColor = Color.green * 2f; //Color when button is pressed - required to multiply 2 to get back the bright green pigment.
    private Color unpressedColor; //Original color of the button.
    private SpriteRenderer spriteRenderer;


    //Track Objects and Mass.
    private List<MassSegment> segmentsonButton = new List<MassSegment>();
    private List<Projectile> projectilesOnButton = new List<Projectile>();
    private List<Rigidbody2D> objbutton = new List<Rigidbody2D>(); 

    //Player references.
    private PlayerMovement playermovement;

    //For visualization.
    private SpriteRenderer buttonRender;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI buttonText;
    public string messageToShow = "Press W to leave the level";
    public string buttonmessageToShow = "Hold the button to unlock the door";


    //Reset the button to its unpressed state.
    public void ResetButton()
    {
        hasbeenPressed = false; //Button is no longer triggered.

        if (buttonRender != null)
        {
            buttonRender.color = unpressedColor; //Restore button color.
        }
        if (messageText != null)
        {
            messageText.text = ""; //Remove any displayed mssage.
        }
        if (buttonText != null)
        {
            buttonText.text = ""; //Clear button insturction text.
        }

    }
    private void Awake()
    {
        buttonRender = GetComponent<SpriteRenderer>(); //Get the button's sprite renderer.

        if (buttonRender != null)
        {
            //To read the origional coloir from the sprite renderer.
            unpressedColor = buttonRender.color;

            // If it used black or transparent, we use the material color.
            if (unpressedColor == Color.black || unpressedColor.a == 0f)
            {
                unpressedColor = buttonRender.sharedMaterial.color;
            }
        }
    }

    //Initialize component.
    private void Start()
    {
        buttonRender = GetComponent<SpriteRenderer>();

        if (buttonRender != null)
        {
            buttonRender.color = unpressedColor; //Set the button to unpressed color at start.
        }
        if (messageText != null)
        {
            messageText.text = ""; //Clear any start up messages.
        }
        if (buttonText != null)
        {
            buttonText.text = ""; //Clear any button instructions.
        }
    }

    private void Update()
    {
        float totalMass = 0f; //Reset totalmass counter.

        //Find the sum mass of all objects on the button.
        for (int i = 0; i < segmentsonButton.Count; i++)
        {
            if (segmentsonButton[i] != null)
            {
                totalMass += segmentsonButton[i].GetTotalMass(); //Add the mass of a segmented object.
            }
        }

        //Find the sum mass of projectiles on the buttons.
        for (int i = 0; i < projectilesOnButton.Count; i++)
        {
            Projectile proj = projectilesOnButton[i];
            if (proj != null)
            {
                Rigidbody2D[] childSegments = proj.GetComponentsInChildren<Rigidbody2D>();
                for (int j = 0; j < childSegments.Length; j++) //Run through the array list.
                {
                    Rigidbody2D rb = childSegments[j]; //Get the index.
                    totalMass += rb.mass; //Add mass of each index.
                }
            }
        }

        //Same concept for the button.
        for (int i = 0; i < objbutton.Count; i++)
        {

            if ((objbutton[i] != null))
            {
                totalMass += objbutton[i].mass;
            }
        }
        //Unlocking sequence. If sum mass is more than what is required, unlocked.
        if (totalMass >= requiredMass)
        {
            holdTimer += Time.deltaTime; //Increase hold timer.
            locktimer = 0f; //Reset lock timer since force is present.

            if (buttonRender != null) buttonRender.color = pressedColor; //Change color to pressesd.

            //If held long enogh, trigger the activation event. 
            if (holdTimer >= holdDuration)
            {
                ActivateButton();
            }

        }
        else
        {
            holdTimer = 0f; //If no mass, reset hold timer.

            if (hasbeenPressed) //If the button is active but mass is removed.
            {
                locktimer += Time.deltaTime; //Start countdown.
                if (locktimer >= lockbacktime) //If enough time passed.
                {
                    DeactivateButton(); //Reset the button.
                }

            }
            if (buttonRender != null)
            {
                buttonRender.color = unpressedColor; //Set button back to original color.
            }
            if (messageText != null)
            {
                buttonText.text = buttonmessageToShow; //Show the message.
            }

        }
    }

    //Activation Logic.
    public void ActivateButton()
    {
        hasbeenPressed = true; //Button is now activated.

        if (buttonRender != null)
        {
            buttonRender.color = Color.white; //Reset the hue color. (To get better colourisation).
            buttonRender.color = pressedColor; //Set the finalc olor.
        }

        //If this button controls a door, unlock it.
        if (triggerDoor && connectedDoor != null)
        {
            connectedDoor.UnlockedDoor();
        }
       

        //If this button controls a platform, unlock it.
        if (triggerPlatform && platforms != null)
        {
            platforms.Startmoving();
        }

        if (messageText != null) messageText.text = messageToShow; //Show activation mesage.



    }

    //Triggered when collider enter the trigger area.
    private void OnTriggerStay2D(Collider2D other)
    {
        //Debug.Log("Button triggered by: " + other.name);

        //React when the button hasn't been pressed and the object enter is the player.
        //if (!hasbeenPressed && other.GetComponentInParent<PlayerMovement>() != null)
        //{
        //    hasbeenPressed = true;
        //    if (buttonRender != null)
        //    {
        //        buttonRender.color = pressedColor;

        //        Debug.Log("Button Pressed");
        //        connectedDoor.UnlockedDoor();
        //    }
        //    if (messageText != null)
        //    {
        //        messageText.text = messageToShow;
        //    }
        //}

        //if (other.name.StartsWith("projectileV3"))
        //{ 
        //    MassSegment projmass = other.GetComponentInParent<MassSegment>();
        //    if (projmass != null)
        //    { 
        //        projmass.CheckdoorMass();
        //        if (!hasbeenPressed)
        //        {
        //            if (buttonRender != null)
        //            {
        //                buttonRender.color = pressedColor;
        //            }
        //            if (messageText != null)
        //            {
        //                messageText.text = messageToShow;
        //            }
        //        }
        //    }
        //}


        //Add mass segment object to the list if it is not added.
        MassSegment mass = other.GetComponentInParent<MassSegment>();

        if (mass != null && !segmentsonButton.Contains(mass))
        {
            //If the object is a mass segment and it's not already on the button, add it.
            segmentsonButton.Add(mass);
        }

        //Add projectile object to the list if it is not added.
        Projectile proj = other.GetComponentInParent<Projectile>();
        if (proj != null && !projectilesOnButton.Contains(proj))
        {
            //If the object is a mass segment and it's not already on the button, add it.
            projectilesOnButton.Add(proj);
        }

        //Adding a rigidbody2d object.
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null && !objbutton.Contains(rb))
        {
            //To prevent double-counting if the object is already counted as mass segment or projectile.
            if (other.GetComponentInParent<MassSegment>() == null && other.GetComponentInParent<Projectile>() == null)
            { 
                //Add the RB to the list.
                objbutton.Add(rb);  
            }
        }



    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Remove massSegment when exit the trigger area.
        MassSegment mass = other.GetComponentInParent<MassSegment>();
        if (mass != null && segmentsonButton.Contains(mass))
        {
            segmentsonButton.Remove(mass); //Remove mass segment from the list when it leaves the button.

            //If no more objects (mass segments or projectiles) are on the button, reset the timer.
            if (segmentsonButton.Count == 0 && projectilesOnButton.Count == 0)
            {
                holdTimer = 0; //Reset timer.
            }
        }

        //Do the same for porojecitle and rb..
        Projectile proj = other.GetComponentInParent<Projectile>();
        if (proj != null && projectilesOnButton.Contains(proj))
        {
            projectilesOnButton.Remove(proj);
            if (segmentsonButton.Count == 0 && projectilesOnButton.Count == 0)
            {
                holdTimer = 0;
            }
        }
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null && objbutton.Contains(rb))
        {
            objbutton.Remove(rb);
           
        }
    }

    //Deactivate the button, reset state and the visual.
    public void DeactivateButton()
    {
        hasbeenPressed = false;

        if (buttonRender != null)
            buttonRender.color = unpressedColor;

        if (triggerDoor && connectedDoor != null)
            connectedDoor.LockDoor();

        if (triggerPlatform && platforms != null)
            platforms.Stopmiving();

        if (messageText != null)
            buttonText.text = buttonmessageToShow;

        if (triggerDoor && grabblebox != null)
        {
            connectedDoor.LockDoor();
        }
    }

}

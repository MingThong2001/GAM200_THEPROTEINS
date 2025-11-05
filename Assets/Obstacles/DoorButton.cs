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

    public float holdDuration = 5f;
    public float holdTimer = 0f;

    //To reactivate the door
    public float lockbacktime = 2f;
    public float locktimer = 0f;

    //Track whether the button has been pressed.
    public bool hasbeenPressed = false;
    public bool triggerDoor = false;
    public bool triggerPlatform = false;

    //Color Setting for visual feedback.
    private Color pressedColor = Color.green * 2f;
    private Color unpressedColor;
    private SpriteRenderer spriteRenderer;


    //Track Objects and Mass.
    private List<MassSegment> segmentsonButton = new List<MassSegment>();
    private List<Projectile> projectilesOnButton = new List<Projectile>();
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
        hasbeenPressed = false;
        if (buttonRender != null)
        {
            buttonRender.color = unpressedColor;
        }
        if (messageText != null)
        {
            messageText.text = "";
        }
        if (buttonText != null)
        {
            buttonText.text = "";
        }

    }
    private void Awake()
    {
        buttonRender = GetComponent<SpriteRenderer>();

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
            buttonRender.color = unpressedColor;
        }
        if (messageText != null)
        {
            messageText.text = "";
        }
        if (buttonText != null)
        {
            buttonText.text = "";
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
                totalMass += segmentsonButton[i].GetTotalMass();
            }
        }

        //Find the sum mass of projectiles on the buttons.
        for (int i = 0; i < projectilesOnButton.Count; i++)
        {
            Projectile proj = projectilesOnButton[i];
            if (proj != null)
            {
                Rigidbody2D[] childSegments = proj.GetComponentsInChildren<Rigidbody2D>();
                for (int j = 0; j < childSegments.Length; j++)
                {
                    Rigidbody2D rb = childSegments[j];
                    totalMass += rb.mass;
                }
            }
        }

        //Unlocking sequence. If sum mass is more than what is required, unlocked.
        if (totalMass >= requiredMass)
        {
            holdTimer += Time.deltaTime;
            locktimer = 0f;
            if (buttonRender != null) buttonRender.color = pressedColor;
            if (holdTimer >= holdDuration)
            {
                ActivateButton();
            }

        }
        else
        {
            holdTimer = 0f;

            if (hasbeenPressed)
            {
                locktimer += Time.deltaTime;
                if (locktimer >= lockbacktime)
                {
                    DeactivateButton();
                }

            }
            if (buttonRender != null)
            {
                buttonRender.color = unpressedColor;
            }
            if (messageText != null)
            {
                buttonText.text = buttonmessageToShow;
            }

        }
    }

    //Activation Logic.
    public void ActivateButton()
    {
        hasbeenPressed = true;

        if (buttonRender != null)
        {
            buttonRender.color = Color.white;
            buttonRender.color = pressedColor;
        }

        // Door button
        if (triggerDoor && connectedDoor != null)
        {
            connectedDoor.UnlockedDoor();
        }
        //Grabble Box
        if (grabblebox.CompareTag("Object"))
        {
            connectedDoor.UnlockedDoor();

        }
        //Player

        // Platform button
        if (triggerPlatform && platforms != null)
        {
            platforms.Startmoving();
        }

        if (messageText != null) messageText.text = messageToShow;



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


        //Check if the relevant compoenent and add it into the list if is not present.
        MassSegment mass = other.GetComponentInParent<MassSegment>();
        if (mass != null && !segmentsonButton.Contains(mass))
        {
            segmentsonButton.Add(mass);
        }
        Projectile proj = other.GetComponentInParent<Projectile>();
        if (proj != null && !projectilesOnButton.Contains(proj))
        {
            projectilesOnButton.Add(proj);
        }




    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MassSegment mass = other.GetComponentInParent<MassSegment>();
        if (mass != null && segmentsonButton.Contains(mass))
        {
            segmentsonButton.Remove(mass); //Remove object after the object is not on the button.
            if (segmentsonButton.Count == 0 && projectilesOnButton.Count == 0)
            {
                holdTimer = 0; //Reset timer.
            }
        }

        Projectile proj = other.GetComponentInParent<Projectile>();
        if (proj != null && projectilesOnButton.Contains(proj))
        {
            projectilesOnButton.Remove(proj);
            if (segmentsonButton.Count == 0 && projectilesOnButton.Count == 0)
            {
                holdTimer = 0;
            }
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

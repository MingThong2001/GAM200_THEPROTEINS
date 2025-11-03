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

    public float holdDuration = 5f;
    public float holdTimer = 0f;

    //To reactivate the door
    public float lockbacktime = 2f;
    public float locktimer = 0f;

    //Track whether the button has been pressed.
    public bool hasbeenPressed = false;

    //Color Setting for visual feedback.
    private Color pressedColor = Color.green * 2f;
    private Color unpressedColor;
    private SpriteRenderer spriteRenderer;


    //Track Objects and Mass
    private List<MassSegment> segmentsonButton = new List<MassSegment>();
    private List <Projectile> projectilesOnButton = new List<Projectile>();   
    //Visuals
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
            // Try to read the color from the SpriteRenderer
            unpressedColor = buttonRender.color;

            // If it's black or transparent (which can happen), use the material tint instead
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
        float totalMass = 0f;

        for (int i = 0; i < segmentsonButton.Count; i++)
        {
            if (segmentsonButton[i] != null)
            {
                totalMass += segmentsonButton[i].GetTotalMass();
            }
        }
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


    public void ActivateButton()
    { 
        hasbeenPressed = true;

        if (buttonRender != null)
        {
            buttonRender.color = Color.white;
            buttonRender.color = pressedColor;
        }
        if (connectedDoor != null)
        {
            connectedDoor.UnlockedDoor();
        }

        if (platforms != null)
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
            segmentsonButton.Remove(mass);
            if (segmentsonButton.Count == 0 && projectilesOnButton.Count == 0)
            {
                holdTimer = 0;
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
    public void DeactivateButton()
    {
        hasbeenPressed = false;

        if (buttonRender != null)
        {
            buttonRender.color = unpressedColor;
        }
        if (connectedDoor != null)
        {
            connectedDoor.LockDoor();
        }
        if (messageText != null) buttonText.text = buttonmessageToShow;

        if (platforms != null)
        { 
            platforms.Stopmiving();
        }
    }

}

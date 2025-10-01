using NUnit.Framework;
using TMPro; 
using UnityEngine;
using System.Collections.Generic;


public class DoorButton : MonoBehaviour
{
    //Reference.
    public Door connectedDoor;

    public float requiredMass = 0.2f;

    public float holdDuration = 5f;
    public float holdTimer = 0f;

    //To reactivate the door
    public float lockbacktime = 2f;
    public float locktimer = 0f;

    //Track whether the button has been pressed.
    public bool hasbeenPressed = false;

    //Color Setting for visual feedback.
    private Color pressedColor = Color.green;
    private Color unpressedColor = new Color(186f / 255f, 79f / 255f, 97f / 255f);
  
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
            buttonRender.color = pressedColor;
        }
        if (connectedDoor != null)
        {
            connectedDoor.UnlockedDoor();
        }
        if (messageText != null) messageText.text = messageToShow;

    }

    //Triggered when collider enter the trigger area.
    private void OnTriggerEnter2D(Collider2D other)
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
            holdTimer = 0f;
        }

        Projectile proj = other.GetComponentInParent<Projectile>();
        if (proj != null && projectilesOnButton.Contains(proj))
        {
            projectilesOnButton.Remove(proj);
            holdTimer = 0f;
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


    }

}

using System;
using UnityEngine;

public class InteractableObjects : MonoBehaviour
{
    public enum InteractionType
    {
        Switch,
        Plate,
        Jam,
    }
    public InteractionType type;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BodyParts"))
        {
            triggerInteraction();
        }
    }

    private void triggerInteraction()
    {
        switch (type)
        { 
            case InteractionType.Switch:
               // flipSwitch();
                break;
            case InteractionType.Plate:
             //  pressPlate();
                break;
            case InteractionType.Jam:
              //  jamMachinery();
                break;
       
        }
    }
}

using System.Collections.Generic;

using UnityEngine;

public class CheckPoints : MonoBehaviour
{
    //Might not need this if we just going to have 1 checkpoint. But keep it first.
    public static List<CheckPoints> allCheckPoints = new List<CheckPoints>();


    //Checkpoint Settings. 
    public bool isActivated = false;
    public Color cpActivated = Color.white;
    public Color cpDisabled = Color.red;
    public GameObject player;
    private SpriteRenderer spriteRenderer;

    //Saved player state when checkpoint is activated.
    public float savedHealth;
    public int savedSegment;
    public Vector2 savedPosition;
    
    //References
    private PlayerStats playerStats;    

    private void Awake()
    {

       spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        //If player reference is assigned, save intial stats.
        if (player != null)
        {
            if (playerStats != null)
            {
                savedHealth = playerStats.GetCurrentHealth();
                savedSegment = playerStats.currentSegments;
                savedPosition = player.transform.position;
            }
        }
        isActivated = false; //Checkpoints start as inactive first.
        UpdateColor(); 
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Only activate the checkpoints if player touches the checkpoint capsules.
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        if (!isActivated)
        {
            Activatethischeckpoint();
        }

    }

    public void Activatethischeckpoint()
    { 
        //Checkpoint is set as activated.
        isActivated = true;

        PlayerStats stats = player.GetComponent <PlayerStats >();
        if (stats != null)
        { 
            savedHealth = stats.GetCurrentHealth();
            savedSegment = stats.currentSegments;
            savedPosition = player.transform.position;  
        }
        UpdateColor(); //Update colour to show that the checkpoint has been activated.
    }

   
    public void Deactivate()
    { 
        isActivated = false;
        UpdateColor();
    }

    //Change checkpoint's colour depends on its activation state.
    private void UpdateColor()
    {
        if (spriteRenderer != null)
        { 
            spriteRenderer.color = isActivated ? cpActivated : cpDisabled;  
        }

    }

    //Move the player back to the checkpoint's saved position.
    public void RespawnatCheckPoint()
    {
        if (player != null)
        { 
            player.transform.position = savedPosition;
            if (playerStats != null)
            { 
                playerStats.GetCurrentHealth();
                playerStats.currentSegments = savedSegment;
            }

        }
    }
}
   
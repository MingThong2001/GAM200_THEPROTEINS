using Unity.Cinemachine;
using UnityEngine;

public class UpgradeCollectible : MonoBehaviour
{
    //References.
    private AudioManager audioManager;
    public AudioClip pickup;

    private void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

    }
    //Triggered when another collider enter this collider tirgger.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject playerroot = null;

        //Check collider or gameobject.
        if (collision.name == "SquishyShell")
        {
            playerroot = collision.transform.parent.gameObject;
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.pickup);

            }
        } 
        else if (collision.GetComponent<PlayerMovement>() != null) //SafetyNet 01.
        {

            playerroot = collision.gameObject;
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.pickup);

            }
        }
        else if (collision.transform.parent != null) //SafetyNet 02.
        { 
            PlayerMovement pm = collision.transform.parent.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                playerroot = collision.transform.parent.gameObject;
                if (audioManager != null)
                {
                    audioManager.PlaySFX(audioManager.pickup);

                }
            }
        }

        if (playerroot != null) //If the components exsit, find the tentacle script.
        { 
         TentacleAppendage tentacle = playerroot.GetComponentInChildren<TentacleAppendage>();
            if (tentacle != null)
            {
                tentacle.enabled = true; //Enable it.
            }
            else
            { }

            Destroy(gameObject);
        }
    }

}
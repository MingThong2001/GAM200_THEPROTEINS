using Unity.Cinemachine;
using UnityEngine;

public class UpgradeCollectible : MonoBehaviour
{
    private AudioManager audioManager;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject playerroot = null;

        //check collider or gameobject.
        if (collision.name == "SquishyShell")
        {
            playerroot = collision.transform.parent.gameObject;
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.pickup);

            }
        }
        else if (collision.GetComponent<PlayerMovement>() != null)
        {

            playerroot = collision.gameObject;
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.pickup);

            }
        }
        else if (collision.transform.parent != null)
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

        if (playerroot != null)
        { 
         TentacleAppendage tentacle = playerroot.GetComponentInChildren<TentacleAppendage>();
            if (tentacle != null)
            {
                tentacle.enabled = true;
            }
            else
            { }

            Destroy(gameObject);
        }
    }

}
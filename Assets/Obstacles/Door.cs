using UnityEngine;

public class Door : MonoBehaviour
{
    //Door state variables.
    public bool isOpening = false;
    public bool hasOpened = false;
    public bool isUnlocked = false;
    
    //References
    public GameObject whitecoverdoor;
    private GameManager gameManager;
    public bool Playerinreange = false;
    private GameObject player;
    public float massThreshold = 2f;

    //Unlock Range
    public float minMassThreshold = 0f;
    public float maxMassThreshold = 1f;
   
    //Reset the door to default closed and locked state.
    public void ResetDoor()
    {
        isOpening = false;
        hasOpened = false;
        isUnlocked = false;
        Playerinreange = false;
        
        //Find the whitecover door if its not assigned.
        if (whitecoverdoor == null)
        {
            whitecoverdoor = transform.Find("whitecoverdoor")?.gameObject;

        }

        //Reactivate the door cover if it exists.
        if (whitecoverdoor != null)
        {
            whitecoverdoor.SetActive(true);
        }
    }

    //Initialization.
    public void Start()
    {
        gameManager = GameManager.instance;
        if (gameManager != null && gameManager.player != null)
        {
            player = gameManager.player;
        }
        if (whitecoverdoor == null)
        {
            whitecoverdoor = transform.Find("whitecoverdoor").gameObject;
        }

        
    }

    public void Update()
    {
        //Check if the player is in range, has pressed W and when the door is unlocked.
        if (Playerinreange && Input.GetKeyDown(KeyCode.W) && isUnlocked)
        {
            Debug.Log("Player in range and W pressed. Opening door...");

            OpenDoor();
        }

        //If the door is opening, and haven't been marked as open yet.
        if (isOpening && !hasOpened)
        {
            isOpening = false;
            hasOpened = true; //Mark the door as opened.
            handleVictory();
        }
    }

    //Detect when something enters the door's trigger area.
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D triggered by: {other.name} with tag: {other.tag}");

        //Check against the player.
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is in the trigger zone.");

            //Confirm if its the correct player object.
            if (other.transform.IsChildOf(player.transform) || other.CompareTag("Player"))
            {

                Playerinreange = true;
                Debug.Log("Player entered range.");
            }

        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Playerinreange = false;
        }
    }
    public void OpenDoor()
    {
        isOpening = true;
    }

    //Called by the button to unlock the door.
    public void UnlockedDoor()
    {
        Debug.Log("UnlockedDoor() called!");

        isUnlocked = true;
        Debug.Log("Door unlocked!");

        //Remove the door cover to visually indicate that it is unlocked.
        if (whitecoverdoor != null)
        {
            Destroy(whitecoverdoor);
        }
        else
        {
            Debug.Log("whitecoverdoor is null.");

        }
    }

    //Handle victory by calling gameManager.
    public void handleVictory()
    {
        if (GameManager.instance != null)
        {
            gameManager.endGame(true);
        }
    }

  
}

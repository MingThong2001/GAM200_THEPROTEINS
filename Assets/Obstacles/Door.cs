using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpening = false;
    public bool hasOpened = false;
    public bool isUnlocked = false;

    public GameObject whitecoverdoor;
    private GameManager gameManager;
    public bool Playerinreange = false;
    private GameObject player;
   
    public void ResetDoor()
    {
        isOpening = false;
        hasOpened = false;
        isUnlocked = false;
        Playerinreange = false;

        if (whitecoverdoor == null)
        {
            whitecoverdoor = transform.Find("whitecoverdoor")?.gameObject;

        }
        if (whitecoverdoor != null)
        {
            whitecoverdoor.SetActive(true);
        }
    }
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

        if (Playerinreange && Input.GetKeyDown(KeyCode.W) && isUnlocked)
        {
            Debug.Log("Player in range and W pressed. Opening door...");

            OpenDoor();
        }

        if (isOpening && !hasOpened)
        {
            isOpening = false;
            hasOpened = true;
            handleVictory();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D triggered by: {other.name} with tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is in the trigger zone.");

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
    public void UnlockedDoor()
    {
        Debug.Log("UnlockedDoor() called!");  // Add this log to confirm the method is being called

        isUnlocked = true;
        Debug.Log("Door unlocked!");

        if (whitecoverdoor != null)
        {
            Destroy(whitecoverdoor);
        }
        else
        {
            Debug.Log("whitecoverdoor is null.");

        }
    }

    public void handleVictory()
    {
        if (GameManager.instance != null)
        {
            gameManager.endGame(true);
        }
    }

  
}

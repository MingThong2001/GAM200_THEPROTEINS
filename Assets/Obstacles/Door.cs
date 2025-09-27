using UnityEngine;

public class Door : MonoBehaviour
{
    private Vector3 initialPosition;
    public Vector3 openforOffset = new Vector3(0, 0.3f, 0);
    public float openSpeed = 5f;
    public bool isOpening = false;
    public bool hasOpened = false;

    //Victory UI Panel.
    public GameObject victoryPanel;


    public void Start()
    {
        initialPosition = transform.position;
    }

    public void Update()
    {
        if (isOpening)
        {
            Vector3 targetPosition = initialPosition + openforOffset;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.02f)
            {
                isOpening = false;
                hasOpened = true;

                if (victoryPanel != null)
                {
                    victoryPanel.SetActive(true);
                }
            }
        }
        else
        {
            return;
        }
    }

    public void OpenDoor()
    { 
        isOpening = true;
    }
}

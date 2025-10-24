using System.Collections;

using UnityEngine;

public class EMPShockWave : MonoBehaviour
{
    [SerializeField] private float expandSpeed = 3f;   // how fast it grows
    [SerializeField] private float shrinkSpeed = 3f;   // how fast it shrinks
    [SerializeField] private float maxScale = 5f;      // target max scale
    [SerializeField] private float minScale = 0.1f;    // starting scale
    [SerializeField] private float delayBeforeShrink = 0.1f; // optional pause before shrinking

    private bool expanding = true;
    private bool shrinking = false;
    public GameObject followtarget;
    private void Start()
    {
        transform.localScale = Vector3.one * minScale;
        Debug.Log("Start scale: " + transform.localScale);
    }

    private void Update()
    {
        if ( followtarget != null)
        {
                transform.position = followtarget.transform.position;   

        }
        if (expanding)
        {
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                Vector3.one * maxScale,
                expandSpeed * Time.deltaTime
            );

            if (Mathf.Approximately(transform.localScale.x, maxScale))
            {
                expanding = false;
                Invoke(nameof(StartShrink), delayBeforeShrink);
            }
        }
        else if (shrinking)
        {
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                Vector3.one * minScale,
                shrinkSpeed * Time.deltaTime
            );

            if (Mathf.Approximately(transform.localScale.x, minScale))
            {
                Debug.Log("EMP pulse complete — destroying object.");
                Destroy(gameObject);
            }
        }
    }

    private void StartShrink()
    {
        shrinking = true;
    }


private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
            //if (player != null)
            // //   player.ApplyEmpParalysis(paralysisDuration);
        }
    }
}
using UnityEngine;

public class Traps : MonoBehaviour
{

    public int spikedamage = 1;
    public float bounceForce = 20f;
    public bool isRectractable = false;
    public bool isActive = true;
    private Collider2D trapCollider;
    private Renderer spikeRenderer;
    //private Animator spikeAnimator;
    public void Start()
    {
        trapCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (isRectractable)
        {
            //Toggle every 2 seconds.
            if (Time.time % 4 < 2)
            {
                ActivateTrap();
            }
            else
            {
                DeactivateTrap();
            }
        }
    }

    //Posibly use animator.
    private void ActivateTrap()
    {
        isActive = true;
        trapCollider.enabled = true;
        spikeRenderer.enabled = true;

    }

    private void DeactivateTrap()
    {
        isActive = true;
        trapCollider.enabled = false;
        spikeRenderer.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        { 
            HandlePlayerBounce(collision.gameObject);
        }
    }

    private void HandlePlayerBounce(GameObject Player)
    { 
    
        Rigidbody2D rb = Player.GetComponent<Rigidbody2D>();

        if (rb)
        {
            //apply bounce force
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        }
    }
}

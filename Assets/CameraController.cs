using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3 targetPoint = Vector3.zero;
    public PlayerMovement playermovement;
    public float lookAheadDistance = 5f;
    public float lookAheadSpeed = 5f; // Reduced from 15 for slower flip
    public float cameraSmoothTime = 0.5f; // New: controls overall camera smoothness
    public float verticalSmoothTime = 0.3f;
    public float fallingSmoothTime = 0.15f;
    public Transform player;
    private float lookOffset;
    private float verticalVelocity;
    private float horizontalVelocity;

    private void Start()
    {
        targetPoint = new Vector3(playermovement.transform.position.x, playermovement.transform.position.y, transform.position.z);
        lookOffset = 0f;
    }

    private void LateUpdate()
    {

        // Auto-find player if not assigned
        if (player == null)
        {
               return; // no player yet
        
        }

        float currentVerticalSmoothTime = verticalSmoothTime;
        Rigidbody2D rb = player.GetComponentInParent<Rigidbody2D>();
        PlayerMovement pm = player.GetComponentInParent<PlayerMovement>();

        if (pm != null && !pm.isGrounded && rb.linearVelocity.y < -1f)
        {
            currentVerticalSmoothTime = fallingSmoothTime;
        }

        float movementDirection = pm != null ? pm.horizontalInput : 0f;

        if (Mathf.Abs(movementDirection) > 0.01f)
        {
            float targetOffset = Mathf.Sign(movementDirection) * lookAheadDistance;
            lookOffset = Mathf.MoveTowards(lookOffset, targetOffset, lookAheadSpeed * Time.deltaTime);
        }
        else
        {
            lookOffset = Mathf.MoveTowards(lookOffset, 0f, lookAheadSpeed * 0.3f * Time.deltaTime);
        }

        float targetX = player.position.x + lookOffset;
        float targetY = player.position.y;

        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref horizontalVelocity, cameraSmoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref verticalVelocity, currentVerticalSmoothTime);

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
    // Called by GameManager when player is spawned
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
}

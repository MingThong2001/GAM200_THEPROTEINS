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
        // Vertical smoothing
        float currentVerticalSmoothTime = verticalSmoothTime;
        if (!playermovement.isGrounded && playermovement.rb.linearVelocity.y < -1f)
        {
            currentVerticalSmoothTime = fallingSmoothTime;
        }

        // Get movement direction from player's horizontalInput
        float movementDirection = playermovement.horizontalInput;

        // Update look offset based on input direction (SLOWER now)
        if (Mathf.Abs(movementDirection) > 0.01f)
        {
            float targetOffset = Mathf.Sign(movementDirection) * lookAheadDistance;
            lookOffset = Mathf.MoveTowards(lookOffset, targetOffset, lookAheadSpeed * Time.deltaTime);
        }
        else
        {
            // Return to center even slower when idle
            lookOffset = Mathf.MoveTowards(lookOffset, 0f, lookAheadSpeed * 0.3f * Time.deltaTime);
        }

        // Calculate target position
        float targetX = playermovement.transform.position.x + lookOffset;
        float targetY = playermovement.transform.position.y;

        // Smooth camera movement (MUCH SMOOTHER now)
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref horizontalVelocity, cameraSmoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref verticalVelocity, currentVerticalSmoothTime);

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}

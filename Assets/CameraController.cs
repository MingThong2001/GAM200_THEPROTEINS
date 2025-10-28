using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    //References.
    public PlayerMovement playermovement;
    public Transform player;

    //Camera Settings.
    private Vector3 targetPoint = Vector3.zero; //The position that cameria is trying to move towards.
    public float lookAheadDistance = 5f; //How far ahead in terms of X axis, the camera should look based on the player's movement.
    public float lookAheadSpeed = 5f; //How quickly the offset transit when player change direction.

    //Camera's Smoothness Transistion.
    public float cameraSmoothTime = 0.5f; 
    public float verticalSmoothTime = 0.3f;
    public float fallingSmoothTime = 0.15f;

    //Range where vertical movement doesn't cause the camera to move.
    public float verticalDeadZone = 2f;

    //Offset Settings.
    private float lookOffset;
    private float verticalVelocity;
    private float horizontalVelocity;

    private void Start()
    {
        //Initialize the camer's target position to match with player's position.
        targetPoint = new Vector3(playermovement.transform.position.x, playermovement.transform.position.y, transform.position.z);
        lookOffset = 0f;
    }

    private void LateUpdate()
    {
        //Auto-find player if not assigned.
        if (player == null)
        {
            return; // no player yet
        }

        //Default vertical smoothing time.
        float currentVerticalSmoothTime = verticalSmoothTime;

        //Get player's references.
        Rigidbody2D rb = player.GetComponentInParent<Rigidbody2D>();
        PlayerMovement pm = player.GetComponentInParent<PlayerMovement>();

        //If the player exists, not grounded and is falling down.
        if (pm != null && !pm.isGrounded && rb.linearVelocity.y < -1f)
        {
            currentVerticalSmoothTime = fallingSmoothTime;
        }

        //Get the player's horizontal input.
        float movementDirection = pm != null ? pm.horizontalInput : 0f;

        //If the player is moving.
        if (Mathf.Abs(movementDirection) > 0.01f)
        {
            //Calculate the target horizontal offset based on the movement direction.
            float targetOffset = Mathf.Sign(movementDirection) * lookAheadDistance;
            lookOffset = Mathf.MoveTowards(lookOffset, targetOffset, lookAheadSpeed * Time.deltaTime); //Smoothly move the lookoffset toward the targetoffset.
        }
        else
        {
            //If the player stop moving, gradually reset the lookaheadoffset.
            lookOffset = Mathf.MoveTowards(lookOffset, 0f, lookAheadSpeed * 0.3f * Time.deltaTime);
        }

        //Calculate the target x position of the camera (player position + lookahead).
        float targetX = player.position.x + lookOffset;

        //Calculate the target y position of the camera (player position + lookahead). *With a set threshhold.
        float verticalDifference = player.position.y - transform.position.y;
        float targetY = transform.position.y; //Start with the current Y position (Will only change if it is outside the set threshold).

        //Move only when it is outside the threshhold.
        if (Mathf.Abs(verticalDifference) > verticalDeadZone)
        {
            //We move the player but keep the threshold.
            targetY = player.position.y - (Mathf.Sign(verticalDifference) * verticalDeadZone);
        }

        //For smooth interpolation to get into targetted X axis and Y axis.
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref horizontalVelocity, cameraSmoothTime);
        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref verticalVelocity, currentVerticalSmoothTime);

        //Apply the new camera position.
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    // Called by GameManager when player is spawned.
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
}

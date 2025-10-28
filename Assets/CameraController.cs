using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{

   //References.
    public Transform player;
    public PlayerMovement playermovement;

    //Camera Settings.
    public float lookAheadDistance = 5f;
    public float lookAheadSpeed = 5f;
    public float cameraSmoothTime = 0.5f;
    public float verticalSmoothTime = 0.3f;
    public float fallingSmoothTime = 0.15f;

   
    //Y-Axis Deadzone.
    public float verticalDeadZone = 2f;

    //Boundaries Settings.
    public string boundaryTag = "CameraBoundary";

    private GameObject[] boundaries;
    private float lookOffset;
    private float horizontalVelocity;
    private float verticalVelocity;
    private float camHalfWidth;
    private float camHalfHeight;

    private void Start()
    {
        //Find boundaries.
        boundaries = GameObject.FindGameObjectsWithTag(boundaryTag);
        lookOffset = 0f;

        //Set Camera.
        Camera cam = Camera.main;
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        //Get referneces.
        Rigidbody2D rb = player.GetComponentInParent<Rigidbody2D>();
        PlayerMovement pm = player.GetComponentInParent<PlayerMovement>();

        //Start with default vertical smooth time.
        float currentVerticalSmooth = verticalSmoothTime;

        //If player exists, in the air and is falling down, use faster vrtical smoothing.
        if (pm != null && !pm.isGrounded && rb.linearVelocity.y < -1f)
            currentVerticalSmooth = fallingSmoothTime;

        //Get horizontal movement input.
        float movementDirection = pm != null ? pm.horizontalInput : 0f;

        //If the player is moving horizontally.
        if (Mathf.Abs(movementDirection) > 0.01f)
        {
            //Determine the target offset for look-ahead.
            float targetOffset = Mathf.Sign(movementDirection) * lookAheadDistance;
            
            //Smoothly move the camera lookoffset toward the target offset.
            lookOffset = Mathf.MoveTowards(lookOffset, targetOffset, lookAheadSpeed * Time.deltaTime);
        }
        else
        {
            //If player stop moving, gradually reset to 0 for netural view.
            lookOffset = Mathf.MoveTowards(lookOffset, 0f, lookAheadSpeed * 0.3f * Time.deltaTime);
        }

        //Calculate the target x position of the camera based on the lookahead and playermovmetn.
        float targetX = player.position.x + lookOffset;

        //Calculate the vertical difference
        float verticalDifference = player.position.y - transform.position.y;

        //Set the targetY as current Y (camera only move sif player is outside verticaldeadzone).
        float targetY = transform.position.y;

        //If vertifcal difference exceeds deadzone, adjust targetY to follow player.
        if (Mathf.Abs(verticalDifference) > verticalDeadZone)
            targetY = player.position.y - Mathf.Sign(verticalDifference) * verticalDeadZone;

        //For smooth movement  (X and Y axis Y)
        float smoothX = Mathf.SmoothDamp(transform.position.x, targetX, ref horizontalVelocity, cameraSmoothTime);
        float smoothY = Mathf.SmoothDamp(transform.position.y, targetY, ref verticalVelocity, currentVerticalSmooth);

        //Caclulate the smooth X,Y.
        Vector3 targetPos = new Vector3(smoothX, smoothY, transform.position.z);

        //Clamp the camera position.
        Vector3 clampedPos = ClampToTaggedBoundaries(targetPos);

        //Apply the clamped camera position.
        transform.position = clampedPos;
    }

    //Boundaries Setting.
    private Vector3 ClampToTaggedBoundaries(Vector3 targetPos)
    {
        //Initialize the extreme min/max values for clamping.
        float minX = float.NegativeInfinity;
        float maxX = float.PositiveInfinity;
        float minY = float.NegativeInfinity;
        float maxY = float.PositiveInfinity;

        //Loop through all boundary objects with the designated tag.
        foreach (GameObject b in boundaries)
        {
            Collider2D col = b.GetComponent<Collider2D>();
            if (col == null) continue;

            Bounds bounds = col.bounds;

            //Determine which side this boundary represent the name.
            if (b.name.ToLower().Contains("left")) minX = Mathf.Max(minX, bounds.max.x + camHalfWidth);
            if (b.name.ToLower().Contains("right")) maxX = Mathf.Min(maxX, bounds.min.x - camHalfWidth);
            if (b.name.ToLower().Contains("bottom")) minY = Mathf.Max(minY, bounds.max.y + camHalfHeight);
            if (b.name.ToLower().Contains("top")) maxY = Mathf.Min(maxY, bounds.min.y - camHalfHeight);
        }

        //If the level is smaller than camera width, centre the camera horizontally.
        if (maxX < minX) { float centerX = (minX + maxX) / 2f; minX = maxX = centerX; }

        //If level is smaller than camera height, center camera vertically.
        if (maxY < minY) { float centerY = (minY + maxY) / 2f; minY = maxY = centerY; }

        //Clamp target of x-axis and y-axis to the min and max bounds.
        float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

        //Return the clamped position.
        return new Vector3(clampedX, clampedY, targetPos.z);
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
}
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //References.
    public Transform player; //To get player's position.
    public PlayerMovement playermovement; //To get player's movement and input.

    //Camera Settings.
    public float lookAheadDistance = 5f;
    public float lookAheadSpeed = 5f; //Transistion speed to lookahead.
    public float cameraSmoothTime = 0.5f; //The smoothing time for camera x-axis movement.
    public float verticalSmoothTime = 0.3f; //Transistion speed for vertical movement.
    public float fallingSmoothTime = 0.15f; //The smoothing time for camera y-axis movement.

   
    //Y-Axis Deadzone. This is a need so to prevent disorienting or any jitteriness.
    public float verticalDeadZone =3f;

    //Boundaries Settings.
    public string boundaryTag = "CameraBoundary"; //Tag used to identify camera boundary objects in the scene.

    private GameObject[] boundaries; //A collection of boundaries object.
    private float lookOffset; //To store the current horizontal offset of the camera baesd on the player movement.

    //To smooth horizontal/vertical camera movement.
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
        camHalfHeight = cam.orthographicSize; //Camera's height in world space.
        camHalfWidth = camHalfHeight * cam.aspect; //Camera's width in world space, calculated using the aspect ratio.
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
        float movementDirection = pm != null ? pm.horizontalInput : 0f; //If the player movement exist, use its horizontal input.

        //If the player is moving horizontally.
        if (Mathf.Abs(movementDirection) > 0.01f)
        {
            // Calculate the target offset for camera lookahead based on the movement direction.
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

        //For smooth movement  (X and Y axis Y) toward the target position.
        float smoothX = Mathf.SmoothDamp(transform.position.x, targetX, ref horizontalVelocity, cameraSmoothTime);
        float smoothY = Mathf.SmoothDamp(transform.position.y, targetY, ref verticalVelocity, currentVerticalSmooth);

        // Calculate the final smoothed target position for the camera.
        Vector3 targetPos = new Vector3(smoothX, smoothY, transform.position.z);

        //Clamp the camera position within the clamped boudnaries.
        Vector3 clampedPos = ClampToTaggedBoundaries(targetPos);

        //Aplying the clamped boundaries.
        transform.position = clampedPos;
    }

    //This is to calculate the boundaries.
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
            Collider2D col = b.GetComponent<Collider2D>(); //Get the collider2d component of the boundary tag.
            if (col == null) continue; //If no Collider2d, skip the boundary object.

            Bounds bounds = col.bounds; //Get the bounding box of the boundary.

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

        //Clamp taget of x-axis and y-axis to the min and max bounds.
        float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

        //Return back to tthe clamped position.
        return new Vector3(clampedX, clampedY, targetPos.z);
    }

    //Place the camera on the new player.
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer; //Set the camera's target player to the new player transform.
    }
}
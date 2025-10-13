using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using Unity.VisualScripting;
public enum slimeState
{ 
    Normal,
    Puddle,
    Segmented
}

public class PlayerMovement : MonoBehaviour
{
    //Movement speed for different states.
    public float normalmovementSpeed = 1f; //Normal state movement speed.
    public float puddlemovementSpeed = 2f; //Puddle state movement speed.
    public float segmentedmovementspeed = 3f; //Segmented state movement speed.

    //Jumping 
    public float childforcemultiplier = 0.25f;      
    private bool isJumping;
    private bool isGrounded;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundcheckRadius = 10f;

    //Squeeze Jump
    private bool isCharging = false;
    private bool isSqueezing = false;
    private float squeezeTimer = 0f;
    private float squeezeChargeduration = 2f;
    private float maxsqueezeboost = 1.5f;
    private float squeezeholdThreshold = 0.5f;

    private List<Transform> segmentTransforms = new List<Transform>();
    private List<Vector3> originalloclPos = new List<Vector3>();    

    //Normal Movement
    public float moveForce = 2f;
    public float maxSpeed = 4f;
    public float groundDrag = 5f;

    //Puddle Movmement
    public float puddleSpeed = 5f;
    public float puddleFriction = 1f;
    public float puddleGravity = 0.3f;


    //Spring Strength & Damping for Segmented movement.
    public float springStrength = 10f; 
    public float springDamp = 5f;


    //Reference to RigidBody2D
    private Rigidbody2D rb;

    
    //Input Variables
    public  float horizontalInput;
    private float verticalInput;    

    //Physics Components
    public SpriteShapeController spriteshapeController; //Used to control the slime's shape.
    private bool isControllingsegment = false; //Flag to control the segmented body.

    //Current state of the player
    public slimeState currentstate = slimeState.Normal; //Slime's initial state.

    //List to manage segmented parts (Slime's segmented state).
    private List<GameObject> segmentedobjects = new List<GameObject>();

   

    //Spline shape only for slime body (Used to update slime's shape and movement).
    [SerializeField] private SpriteShapeController spriteShape;
    [SerializeField] private List<Transform> slimePoints;
    [SerializeField] private MassSegment massSegment;

    //Audio
    private AudioManager audioManager;
    private float movementSFXCD = 0.2f;
    private float movementSFXTimer = 0f;


    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; //Prevent rotation of the player (WIP).
        rb.gravityScale = 5f;

        if (massSegment == null)
        {
            massSegment = GetComponent<MassSegment>();

        }
        else
        {
           
                massSegment.UpdateAllStats();

           
        }

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();


    }
    public void Update()
    {
       
        handleInput();
        CheckGrounded();

     
    }
    
    public void FixedUpdate()
    {
        movementSFXTimer -= Time.deltaTime; 


        handleJump();
        handleMovement();

    }

 

    //User input for movement and state changes.
    public void handleInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 1f; // Move Up
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -1f; // Move Down
        }

        //Handle Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCharging) 
        {
            squeezeTimer = 0f;
            isCharging = false;
            isJumping = true;       // always set for normal jump first
        }

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            squeezeTimer += Time.deltaTime;

            // Switch to squeeze mode if hold time exceeds threshold
            if (squeezeTimer >= squeezeholdThreshold && !isCharging)
            {
                isCharging = true;
                isJumping = false;  // cancel normal jump
                StartSqueeze();
            }

            if (isCharging)
            {
                float chargeTime = squeezeTimer - squeezeholdThreshold;
                float squeezeCharge = Mathf.Clamp01(chargeTime / squeezeChargeduration);
                UpdatesqueezeProgress(squeezeCharge);
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && isGrounded)
        {
            if (isCharging)
            {
                float chargeTime = squeezeTimer - squeezeholdThreshold;
                float squeezeCharge = Mathf.Clamp01(chargeTime / squeezeChargeduration);
                Applysqueezeboost(squeezeCharge);
                EndSqueeze();
            }
            squeezeTimer = 0f;
            isCharging = false;
        }
        //State Input
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            switchState(slimeState.Normal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            switchState(slimeState.Puddle);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            switchState(slimeState.Segmented);
        }


    }

    //Handle movement based on current state.
    public void handleMovement()
    {
        float speed = 0f;
        switch (currentstate)
        {
            case slimeState.Normal:
                speed = normalmovementSpeed;
                normalMovement(speed);
                break;
            case slimeState.Puddle:
                normalMovement(puddlemovementSpeed);
                break;
            case slimeState.Segmented:
                speed = segmentedmovementspeed;
                break;

        }
    }

    public void normalMovement(float speed)
    {
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f; // Move Left
        }
        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f; // Move Right
        }
        float adjustedSpeed = massSegment.currentMoveSpeed;

        Vector2 newPos = rb.position + new Vector2(horizontalInput * adjustedSpeed * Time.fixedDeltaTime, 0f);
        rb.MovePosition(newPos);

       Debug.Log($"[Normal Move] Input: {horizontalInput}, Speed: {adjustedSpeed}, Position: {rb.position}");

        if (horizontalInput != 0f && movementSFXTimer <= 0f)
        {
            audioManager.PlaySFX(AudioManager.movement);
            movementSFXTimer = movementSFXCD;
        }
    }

    #region Squeeze Jump Mechanics
    private List <SpringJoint2D> joints = new List<SpringJoint2D>();
    private List <float> originalDistance = new List<float>();
    private List <float> originalFrequency = new List <float>();
    private List <float> originalDamping = new List<float>();
    private void StartSqueeze()
    {
        isSqueezing = true;
        squeezeTimer = 0f;
        joints.Clear();
        originalDistance.Clear();
        segmentTransforms.Clear();
        originalloclPos.Clear();

        rb.constraints = RigidbodyConstraints2D.FreezePositionX;

        foreach (SpringJoint2D joint in GetComponentsInChildren<SpringJoint2D>())
        {
            if (joint == null) continue;
            Rigidbody2D rbsegment = joint.GetComponent<Rigidbody2D>();
            if (rbsegment == null || joint.transform.parent == null)
            {
                continue;   
            }

            joints.Add(joint);
            originalDistance.Add(joint.distance);
            originalFrequency.Add(joint.frequency);
            originalDamping.Add(joint.dampingRatio);
            segmentTransforms.Add(joint.transform);
            originalloclPos.Add(joint.transform.localPosition);
            

        }



    }

    private void UpdatesqueezeProgress(float squeezeCharge)
    {
        if (segmentTransforms.Count == 0) return;
        squeezeCharge = Mathf.Clamp01(squeezeCharge);

        // Find lowest and highest segments in world space
        float lowestY = float.MaxValue;
        float highestY = float.MinValue;
        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            Transform seg = segmentTransforms[i];
            if (seg == null) continue;
            lowestY = Mathf.Min(lowestY, seg.position.y);
            highestY = Mathf.Max(highestY, seg.position.y);
        }

        float totalHeight = Mathf.Max(highestY - lowestY, 0.01f); // avoid divide by zero
        float maxFraction = 0.75f; // max top-to-bottom squeeze

        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            Transform seg = segmentTransforms[i];
            if (seg == null) continue;

            Rigidbody2D rbSeg = seg.GetComponent<Rigidbody2D>();
            if (rbSeg == null) continue;

            // Calculate relative height: bottom=0, top=1
            float relativeHeight = (seg.position.y - lowestY) / totalHeight;
            relativeHeight = Mathf.Clamp01(relativeHeight);

            // Compute target Y based on squeezeCharge
            float dropAmount = totalHeight * squeezeCharge * maxFraction * relativeHeight;
            Vector3 targetPos = seg.position;
            targetPos.y -= dropAmount;

            // Move smoothly
            float moveSpeed = 20f; // tweak for consistency
            rbSeg.MovePosition(Vector3.Lerp(rbSeg.position, targetPos, moveSpeed * Time.fixedDeltaTime));
        }
    }
    
    private void EndSqueeze()
    {
        isSqueezing = false;
        isCharging = false;
        squeezeTimer = 0f;
        rb.constraints = RigidbodyConstraints2D.None;

        for (int i = 0; i < joints.Count; i++)
        {
            if (i < originalDistance.Count)
            {
                joints[i].distance = originalDistance[i];
                joints[i].frequency = originalFrequency[i];
                joints[i].dampingRatio = originalDamping[i];
            }

            if (i < segmentTransforms.Count && i < originalloclPos.Count)
            {
                Rigidbody2D segmentRb = segmentTransforms[i].GetComponent<Rigidbody2D>();
                if (segmentRb != null && segmentRb != rb)
                {
                    Vector3 targetWorldPos = segmentTransforms[i].parent.TransformPoint(originalloclPos[i]);
                    segmentRb.MovePosition(targetWorldPos);
                }
            }
        }

        joints.Clear();
        originalDistance.Clear();
        segmentTransforms.Clear();
        originalloclPos.Clear();
        originalDamping.Clear();
        originalFrequency.Clear();
    }

    public void Applysqueezeboost(float squeezeCharge)
    {
        if (squeezeCharge > 0.05f)
        {
            float boost = massSegment.currentJumpPower * squeezeCharge * maxsqueezeboost;
            rb.AddForce(Vector2.up * boost, ForceMode2D.Impulse);

            foreach (Rigidbody2D segmentsRB in GetComponentsInChildren<Rigidbody2D>())
            {
                if (segmentsRB != rb)
                {
                    segmentsRB.AddForce(Vector2.up * boost * childforcemultiplier, ForceMode2D.Impulse);
                }
            }
            Debug.Log($"[Squeeze Jump] squeezeCharge: {squeezeCharge:F2}, Jump Force Applied: {boost}");

            EndSqueeze();
            isCharging = false;
        }
    }
    #endregion
    #region Puddle State
    private bool isPuddled = false;
    private List<Transform> puddleSegments = new List<Transform>();
    private List<Vector3> originalLocalPositions = new List<Vector3>();
    private float maxPuddleFraction = 0.75f; // how much the top drops
    public void StartPuddle()
    {
        if (isPuddled) return;
        isPuddled = true;

        puddleSegments.Clear();
        originalLocalPositions.Clear();

        foreach (SpringJoint2D joint in GetComponentsInChildren<SpringJoint2D>())
        {
            if (joint == null) continue;
            puddleSegments.Add(joint.transform);
            originalLocalPositions.Add(joint.transform.localPosition);
        }

        ApplyPuddle(); // apply instantly
    }

    public void EndPuddle()
    {
        if (!isPuddled) return;
        isPuddled = false;

        // Restore all segment positions
        for (int i = 0; i < puddleSegments.Count; i++)
        {
            if (puddleSegments[i] != null)
            {
                puddleSegments[i].localPosition = originalLocalPositions[i];
            }
        }

        puddleSegments.Clear();
        originalLocalPositions.Clear();
    }

    private void ApplyPuddle()
    {
        // Find lowest and highest segments in world space
        float lowestY = float.MaxValue;
        float highestY = float.MinValue;

        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            Transform seg = segmentTransforms[i];
            if (seg == null) continue;
            lowestY = Mathf.Min(lowestY, seg.position.y);
            highestY = Mathf.Max(highestY, seg.position.y);
        }

        float totalHeight = Mathf.Max(highestY - lowestY, 0.01f); // avoid divide by zero
        float maxFraction = 0.75f; // max top-to-bottom squeeze

        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            Transform seg = segmentTransforms[i];
            if (seg == null) continue;

            Rigidbody2D rbSeg = seg.GetComponent<Rigidbody2D>();
            if (rbSeg == null) continue;

            // Calculate relative height: bottom=0, top=1
            float relativeHeight = (seg.position.y - lowestY) / totalHeight;
            relativeHeight = Mathf.Clamp01(relativeHeight);

            // Compute target Y based on squeezeCharge
            float dropAmount = totalHeight * maxFraction * relativeHeight;
            Vector3 targetPos = seg.position;
            targetPos.y -= dropAmount;

            // Move smoothly
            float moveSpeed = 20f; // tweak for consistency
            rbSeg.MovePosition(Vector3.Lerp(rbSeg.position, targetPos, moveSpeed * Time.fixedDeltaTime));
        }
    }

    #endregion



    #region Segmented State


    //Transform the player into segmented state and separate the segment. (WIP)
    /*  public void transformSegmented()
      {
          currentstate = slimeState.Segmented;

          //Get all the circle segments.
          Transform[] circlesegments = GetComponentsInChildren<Transform>();

          foreach (Transform circlesegment in circlesegments)
          {
              //Make each segment independent by removing its SpringJoint2D.
              if (circlesegment != this.transform)
              {
                  GameObject segmentObj = circlesegment.gameObject;

                  //Remove springjoint to break them apart.
                  SpringJoint2D circlejoint = segmentObj.GetComponent<SpringJoint2D>();
                  if (circlejoint != null)
                  {
                      Destroy(circlejoint); //Break the spring connection.
                  }

                  //Add projectle behaviour
                  segmentObj.AddComponent<SegmentProjectile>();

                  //Launch the segment toward the mouse target.
                  //  LaunchSegment(segmentObj);
              }
          }
      }*/

    #endregion

    public void CheckGrounded()
    {
        //Find the lowest point of the player.
        float lowestYpoint = transform.position.y;

        //Loop through all child springJoint2D components.
        foreach (SpringJoint2D joint in GetComponentsInChildren<SpringJoint2D>())
        { 
            //Update lowestY to the smallest Y-position found.
            lowestYpoint = Mathf.Min(lowestYpoint, joint.transform.position.y);
        }
        //Define the position where we check for ground and make it slightly below the lowest segment to avoid floating point error.
        Vector2 checkPos = new Vector2(transform.position.x, lowestYpoint - 0.1f);
        isGrounded = Physics2D.OverlapCircle(checkPos, 0.2f, groundLayer);
    }

  
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("groundLayer") || collision.CompareTag("Objects"))
        {
            isGrounded = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("groundLayer") || collision.CompareTag("Objects"))
        {
            isGrounded = false;
        }
    }
    public void handleJump()
    {
        //Handle jumping behaviour.
        if (isJumping && isGrounded && !isCharging) 
        {
            EndSqueeze();

            float adjustedJump = massSegment.currentJumpPower;
                
            rb.AddForce(Vector2.up * adjustedJump, ForceMode2D.Impulse);

            //Debug.Log($"[Jump] Segments: {massSegment.currentSegments}, Mass: {mass:F2}, Adjusted Jump: {adjustedJump:F2}");
            foreach (Rigidbody2D segmentsRB in GetComponentsInChildren<Rigidbody2D>())
            {
                if (segmentsRB != rb)
                {
                    segmentsRB.AddForce(Vector2.up * adjustedJump , ForceMode2D.Impulse);
                }

            }
            Debug.Log($"[Normal Jump] Jump Force Applied: {adjustedJump}, isGrounded: {isGrounded}");

            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.jump);

            }
            isJumping = false;


        }
    }

    //Switch between different slime states.
    public void switchState(slimeState slimeState)  
    {
        currentstate = slimeState;

        if (currentstate == slimeState.Segmented)
        {
            // transformSegmented();
        }
        else
        {
            resettonormalstate();
        }

        if (currentstate == slimeState.Puddle)
        {
            StartPuddle();
        }
        else
        { 
            EndPuddle();
            resettonormalstate();
        }
    }

    public void resettonormalstate()
    { 
        transform.localScale = Vector3.one; //Reset scale
    }


    


   
}
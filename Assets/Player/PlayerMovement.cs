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
    public Transform targetPosition;
    public float normalmovementSpeed = 1f; //Normal state movement speed.
    public float puddlemovementSpeed = 2f; //Puddle state movement speed.
    public float segmentedmovementspeed = 3f; //Segmented state movement speed.

    //Jumping 
    public float jumpforce = 10f; //Force applied for jumping.
    public float childforcemultiplier = 0.25f;      
    private bool isJumping;
    private bool isGrounded;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundcheckRadius = 1f;


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




    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; //Prevent rotation of the player (WIP).
    }
    public void Update()
    {
       
        handleInput();
        CheckGrounded();
    }
    
    public void FixedUpdate()
    {
        handleJump();
        handleMovement();

    }

    //Used to adjust the shape of the slime's body (WIP).
    /* public void LateUpdate()
     {
         Spline spline = spriteShape.spline;
         for (int i = 0; i < slimePoints.Count; i++)
         {
             Vector3 localPos = spriteShape.transform.InverseTransformPoint(slimePoints[i].position);
             spline.SetPosition( i, localPos );
         }

         spriteShape.BakeMesh();
     }*/




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
        if (Input.GetKeyDown(KeyCode.Space)) 
        { 
            isJumping = true;
            Debug.Log("Jump input detected and grounded!");
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

        //More controls
        if (Input.GetKeyDown(KeyCode.J))
        {
            switchState(slimeState.Normal);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            switchState(slimeState.Puddle);
        }
        else if (Input.GetKeyDown(KeyCode.L))
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
                speed = puddlemovementSpeed;
                puddleMovement();
                break;
            case slimeState.Segmented:
                speed = segmentedmovementspeed;
                break;

        }
    }

    public void normalMovement(float speed)
    {

        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f; // Move Left
        }
        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f; // Move Right
        }
        Vector2 newPos = rb.position + new Vector2(horizontalInput * normalmovementSpeed *Time.fixedDeltaTime, 0f);
        rb.MovePosition(newPos);
    }


    #region Puddle State
    //Puddle State movement. (WIP)
    public void puddleMovement()
    {

        // Calculate current horizontal speed
        Vector2 velocityAtPoint = rb.GetPointVelocity(rb.position);
        float horizontalSpeed = velocityAtPoint.x;

        // Apply force only if under max speed
        if (Mathf.Abs(horizontalSpeed) < maxSpeed)
        {
            rb.AddForce(new Vector2(horizontalInput * moveForce, 0), ForceMode2D.Force);
        }

        // Apply drag when no input
        rb.linearDamping = (horizontalInput == 0 && isGrounded) ? groundDrag : 0f;
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
    }

    #endregion */

    //Check if the player is grounded by checking the vertical velocity of each Rigidbody2D component.
    public void CheckGrounded()
    {
        Vector2 checkPos = (Vector2)transform.position + Vector2.down * 0.5f;
        isGrounded = Physics2D.OverlapCircle(checkPos, groundcheckRadius, groundLayer);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("groundLayer"))
        {
            isGrounded = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("groundLayer"))
        {
            isGrounded = false;
        }
    }
    public void handleJump()
    {
        //Handle jumping behaviour.
        if (isJumping && isGrounded)
        {
            rb.AddForce(rb.position + Vector2.up * jumpforce * Time.fixedDeltaTime);

            foreach (Rigidbody2D segmentsRB in GetComponentsInChildren<Rigidbody2D>())
            {
                if (segmentsRB != rb)
                {
                    segmentsRB.AddForce(Vector2.up * jumpforce * childforcemultiplier, ForceMode2D.Impulse);
                }
            
            }

            foreach (SpringJoint2D spring in GetComponentsInChildren<SpringJoint2D>())
            {
                spring.dampingRatio = 0.5f;
                spring.frequency = 4f;
            }
        }
        isJumping = false;


    }


    //Switch between different slime states.
    public void switchState(slimeState slimeState)  
    {
        currentstate = slimeState;

        if (currentstate == slimeState.Segmented)
        {
           // transformSegmented();
        }
        else if (currentstate == slimeState.Puddle)
        {
         //   transformPuddle();
        }
        else
        { 
            resettonormalstate();
        }
    }

    public void resettonormalstate()
    { 
        transform.localScale = Vector3.one; //Reset scale
        rb.linearDamping = 0f;
    }


    


   
}
#endregion  
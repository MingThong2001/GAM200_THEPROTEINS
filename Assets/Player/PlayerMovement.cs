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
    public float normalmovementSpeed = 5f; //Normal state movement speed.
    public float puddlemovementSpeed = 2f; //Puddle state movement speed.
    public float segmentedmovementspeed = 3f; //Segmented state movement speed.

    public float squishfactor = 0.5f; //Factor for squishing effect (Used in puddle state).
    public float jumpforce = 0.5f; //Force applied for jumping.

    //Spring Strength & Damping for Segmented movement.
    public float springStrength = 10f; 
    public float springDamp = 5f;

    //Reference to RigidBody2D
    private Rigidbody2D rb;


    //Input Variables
    private float horizontalInput;
    private float verticalInput;    
    private bool isJumping;
    private bool isGrounded;

    //Physics Components
    public SpriteShapeController spriteshapeController; //Used to control the slime's shape.
    private bool isControllingsegment = false; //Flag to control the segmented body.

    //Current state of the player
    public slimeState currentstate = slimeState.Normal; //Slime's initial state.

    //List to manage segmented parts (Slime's segmented state).
    private List<GameObject> segmentedobjects = new List<GameObject>();


    
   // private int currentcontrolsegmentedIndex = -1;


    //Layers
    public LayerMask groundLayer;

    //Spline shape only for slime body (Used to update slime's shape and movement).
    [SerializeField] private SpriteShapeController spriteShape;
    [SerializeField] private List<Transform> slimePoints;
  
    public void Update()
    {
        CheckGrounded();
        handleInput();
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

  
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; //Prevent rotation of the player (WIP).
    }

    //User input for movement and state changes.
    public void handleInput()
    {
        horizontalInput = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 4f; // Move Up
        }
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -4f; // Move Left
        }
        if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -4f; // Move Down
        }
        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 4f; // Move Right
        }

        //Handle Jump input
        if (Input.GetButtonDown("Jump") && isGrounded)
        { 
            isJumping = true;
            Debug.Log("Jump input detected and grounded!");
        }
        else if (Input.GetButtonDown("Jump") && !isGrounded)
        {

            Debug.Log("Jump input detected and not grounded!");

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
        Debug.Log($"isGrounded: {isGrounded}, linearVelocity.y: {rb.linearVelocity.y}"); // Add this

        float speed = 0f;

        //Determine movement speed based on current slime state.
        switch (currentstate)
        { 
            case slimeState.Normal:
                speed = normalmovementSpeed;
                break;
            case slimeState.Puddle:
                speed = puddlemovementSpeed;
                break;
            case slimeState.Segmented:
                speed = segmentedmovementspeed;
                break;
        }

        //Move the player horizontally based on their input and state speed.
        Vector2 targetPosition = new Vector2(rb.position.x + horizontalInput * speed * Time.deltaTime, rb.position.y);
        rb.MovePosition(targetPosition);

        //Handle jumping behaviour.
        if (isJumping && isGrounded)
        {
            // Apply force to EVERY rigidbody, not just the main one
            Rigidbody2D[] allRbs = GetComponentsInChildren<Rigidbody2D>();
            Debug.Log($"Found {allRbs.Length} rigidbodies");

            foreach (Rigidbody2D rb2d in allRbs)
            {
                rb2d.AddForce(Vector2.up * 0.25f, ForceMode2D.Impulse); // Smaller force per segment
                Debug.Log($"Applied force to {rb2d.name}");
            }

            isJumping = false; //Reset the jump flag after applying force.
        }
    }

    //Check if the player is grounded by checking the vertical velocity of each Rigidbody2D component.
    public void CheckGrounded()
    {
        Rigidbody2D[] allRigidBodies = GetComponentsInChildren<Rigidbody2D>();
        isGrounded = false;
        foreach (Rigidbody2D rb in allRigidBodies)
        {
            if (Mathf.Abs(rb.linearVelocity.y) < 1f)
            {
                isGrounded = true;

            }
        }
    }

    
    
    //Switch between different slime states.
    public void switchState(slimeState slimeState)  
    {
        currentstate = slimeState;

        if (currentstate == slimeState.Segmented)
        {
            transformSegmented();
        }
        else if (currentstate == slimeState.Puddle)
        {
            transformPuddle();
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


    #region Puddle State
    //Puddle State movement. (WIP)
    public void puddleMovement()
    {

        Vector2 targetPosition = rb.position + new Vector2(horizontalInput * puddlemovementSpeed * Time.deltaTime, rb.position.y);
        rb.MovePosition(targetPosition);

        //Adding sliding physics.
        rb.AddForce(Vector2.right * horizontalInput * puddlemovementSpeed, ForceMode2D.Impulse);
        //Puddle will have no jump movement
    }

    //Transfrom the player into Puddle state. (WIP)
    public void transformPuddle()
    {
        currentstate = slimeState.Puddle;
        rb.linearDamping = 2f; //Increase damping for slower movement.

    }
    #endregion


    #region Segmented State

    //Transform the player into segmented state and separate the segment. (WIP)
    public void transformSegmented()
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
                LaunchSegment(segmentObj);
            }
        }
    }
    public void LaunchSegment(GameObject segmentObj)
    {
        //Launch Projectile Logic.
    }


    public void AddSegment()
    {


    }

    #endregion
}


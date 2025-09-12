using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class TentacleAppendage : MonoBehaviour
{
    /* TLDR
     * 
     * Extending : Tentacle extends toward a target position.
     * Interaction : if Tentacle hits object (tagged "Object"), it can grab the object.
     * Retracting : Tentacle retracts back to the starting position when the an assigned key is pressed.
     */


    //Store Body
    //Tentacles Properties
    public float maxtentacleLength = 5f;
    public float extensinSpeed = 10f;
    public float retractSpeed = 8f;
    public float pullStrength = 10f;
    public float pushStrength = 5f;

    public SpriteShapeController spriteShapeController;
    public float softDamping = 0.5f;


    private Vector2 targetPosition;
    private bool isExtending = false;
    private bool isRetracting = false;
    private bool isInteracting = false;
    private Rigidbody2D rb;
    private SpringJoint2D springJoint;


    //Object
    private Rigidbody2D grabbedObject = null;

    public void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();

        //Set up SpringJoint2D
        springJoint = gameObject.AddComponent<SpringJoint2D>();
        springJoint.enabled = false;
        
    }

    public void Update()
    {
        HandleInput();
        UpdateTentaclePosition();
     
    }

    private void HandleInput()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);    //Get Mouse Input.

        //Mouse click to pick up /drop items.

        if (Input.GetMouseButtonDown(0))
        {
            if (isInteracting && grabbedObject != null)
            {
                DropObject();
            }
        }
        else
        {
            if (!isExtending && !isRetracting)
            {
                //Extend the tentacle towards the mose position and try to grab it.
                isExtending = true;
                SetTargetPosition(mousePosition);
                TryGrabObject(mousePosition);
            }
        }
        //Update tentecle position.
        if (isExtending && !isRetracting)
        {
            UpdateTentaclePosition();
        }
       
    }
    public void UpdateTentaclePosition()
    {
        //When extending, move the tentacle toward the mouse
        if (isExtending && !isRetracting)
        {
            ExtendTentacle();
        }
        else if (isRetracting && !isExtending)
        {
            RetractTentacle();
        }
    }


    public void ExtendTentacle()
    { 
        //Calculate the direction from player to the target position
        Vector2 extendDirection = (targetPosition - (Vector2)transform.position).normalized;

        //Calculate the tentacle length
        float currentLength = Vector2.Distance(transform.position, targetPosition);

        if (currentLength < maxtentacleLength)
        {
            //Extend the tentacle toward the target
            transform.position = Vector2.MoveTowards((Vector2)transform.position, targetPosition, extensinSpeed * Time.deltaTime);
        }
        else
        {
            //Once the tentalce reaches its target, intreacts with the object.

            isInteracting = true;
            isExtending = false; //Stop extending.
            springJoint.distance = currentLength;

        }
    }

    public void RetractTentacle()
    {
        //Calculate the direction back to the starting point.
        Vector2 retractDirection = ((Vector2)transform.parent.position - (Vector2)transform.position).normalized;

        transform.position = Vector2.MoveTowards((Vector2)transform.position, (Vector2)transform.parent.position, retractSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, transform.parent.position) < 0.1f)
        { 
            isRetracting = false;
            springJoint.enabled = false;
        }
    }

    public void SetTargetPosition(Vector2 position)
    { 
        targetPosition = position; 
    }
    private void TryGrabObject(Vector2 mousePosition)
    {

        //Cast a ray from the tentacle to the target position.
        RaycastHit2D raycastHit2 = Physics2D.Raycast(transform.position, (mousePosition - (Vector2)transform.position).normalized, maxtentacleLength);

        //If the ray hits something.
        if (raycastHit2.collider != null)
        { 
            //Get the rigidbody of the object hit by the ray.
            Rigidbody2D raycastHitRB = raycastHit2.collider.GetComponent<Rigidbody2D>();
            if (raycastHitRB != null && raycastHitRB.CompareTag("Object"))
            {
                grabbedObject = raycastHitRB; //Set this object as grabbed.
                springJoint.connectedBody = grabbedObject;  //Attached to the body
                springJoint.enableCollision = true;

               
                //Start interacting.
                isInteracting = true;
            }
        }
    }

    public void DropObject()
    {
        if (grabbedObject != null)
        { 
            //Disconnect the object from the tentacle
            springJoint.connectedBody = null;   
            grabbedObject = null; //Clear the references
            isInteracting = false;
        }
    }

    private void pushObject()
    {
        
    }
}

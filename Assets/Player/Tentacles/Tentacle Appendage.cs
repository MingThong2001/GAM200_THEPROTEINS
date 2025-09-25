using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class TentacleAppendage : MonoBehaviour
{

    //Store Body
    //Tentacles Properties
    public Transform tentacleAnchor; 
    public Transform tentacleTip;

    public float maxtentacleLength = 5f;
    public float extensinSpeed = 10f;
    public float retractSpeed = 8f;
    public float maxLiftHeight = 2f;
    

    //Spring Joint Settings
    public float springFrequency = 8f;
    public float springDamping = 1f;


    //Object Interaction
    private Vector2 targetPosition;
    private bool isExtending = false;
    private bool isRetracting = false;


    //References
    private Rigidbody2D grabbedObject = null;
    private SpringJoint2D springJoint;
    private Vector2 originalGrabPos;

    public void Start()
    {
        
        if (tentacleAnchor == null)
        {
            enabled = false;
            return;
        }

        //Create and configue the sprint joint dynamically.
        springJoint = gameObject.AddComponent<SpringJoint2D>();
        springJoint.enabled = false;
        springJoint.autoConfigureDistance = false;
        springJoint.frequency = springFrequency;
        springJoint.dampingRatio = springDamping;

        springJoint.connectedAnchor = tentacleAnchor.position; //Connect it to anchor.
        transform.position = tentacleAnchor.position; //Start tentacle at anchor.

    }

    public void Update()
    {
        HandleInput();
        moveTentacle();

       
    }

   
    private void HandleInput()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Left click to extend toward target.
        if (Input.GetMouseButtonDown(1) && grabbedObject == null)
        {
            StartExtend(mousePosition);
        }

        

        // Release
        if (Input.GetMouseButtonUp(1))
        {
            if (grabbedObject != null)
            {
                DropObject();
            }
            StartRetract(); //Begin to retract tentacle.
        }


        // While Holding left click with object.
        if (Input.GetMouseButton(1) && grabbedObject != null)
        {
            dragObject();
           
        }
    }
    private Vector2 constraintsTarget(Vector2 desiredPos)
    {
        Vector2 constraintDirection = desiredPos - (Vector2)tentacleAnchor.position; //Direction from the anchor.
        float distance = Mathf.Min(constraintDirection.magnitude, maxtentacleLength); //Limit by max length.
        Vector2 constrainedPos = (Vector2)tentacleAnchor.position + constraintDirection.normalized * distance; //Constraint position.

       /* //Limit maximum height
        if (grabbedObject != null)
        { 
            float maxY = originalGrabPos.y + maxLiftHeight;
            constrainedPos.y = Mathf.Min(constrainedPos.y, maxY);   
        
        }*/
        return constrainedPos;
    }
    public void StartExtend(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)tentacleAnchor.position).normalized; //Direction to target.
        float distance = Mathf.Min(Vector2.Distance(tentacleAnchor.position, target), maxtentacleLength); //Clamp the distance.
        targetPosition = (Vector2)tentacleAnchor.position + direction * distance; //Calculate final taret position.

        // Raycast to grab object
        Collider2D[] hit = Physics2D.OverlapCircleAll(targetPosition, 0.1f, LayerMask.GetMask("Jumpable"));

        //   RaycastHit2D hit = Physics2D.Raycast(tentacleAnchor.position, direction, distance, LayerMask.GetMask("groundLayer"));

        foreach (Collider2D hit2 in hit)
        {
            if (hit2.CompareTag("Object"))
            { 
                Rigidbody2D rb = hit2.attachedRigidbody ?? hit2.GetComponentInParent<Rigidbody2D>();
                if (rb != null)
                { 
                    grabbedObject = rb;
                    grabbedObject.gravityScale = 0f;
                    originalGrabPos = grabbedObject.position;

                    transform.position = grabbedObject.position;
                    springJoint.connectedBody = grabbedObject;
                    springJoint.frequency = 20f;
                    springJoint.distance = 0.1f;
                    springJoint.autoConfigureDistance = false;
                    springJoint.enabled = true;
                    break;
                }
            }
        
        }

        //if (hit != null && hit.rigidbody != null)
        //{
        //    grabbedObject = hit.rigidbody;

        //    grabbedObject.gravityScale = 0f; //Disable gravity for smooth dragging.
        //    originalGrabPos = grabbedObject.position; //Store original position.
        //    springJoint.connectedBody = grabbedObject;
        //    springJoint.frequency = 20f;
        //    springJoint.distance = 0.1f;
        //    springJoint.autoConfigureDistance = false;
        //    springJoint.enabled = true;

        //}

        isExtending = true;
        isRetracting = false;
    }
    public void StartRetract()
    {
        isExtending = false;
        isRetracting = true;
    }
    public void moveTentacle()
    {
        if (isExtending)
        {
            //Move tentacle toward target.
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, extensinSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
            {
                isExtending = false; //Reach target, stop extending.

                if (grabbedObject == null) //No object to grab? Start retracting.
                {
                    StartRetract();
                }


            }
        }

        if (isRetracting)
        {
            //Move tentacle back toward anchor.
            transform.position = Vector2.MoveTowards(transform.position, tentacleAnchor.position, retractSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, tentacleAnchor.position) < 0.05f)
            {
                isRetracting = false;
                transform.position = tentacleAnchor.position;

               
            }
        }
    }
   
  

    public void SetTargetPosition(Vector2 position)
    { 
        targetPosition = position; //External method to set tentacle target. Might not need, but safety net.
    }
   

    public void DropObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.gravityScale = 1f; //Renable gravity.
           
            springJoint.connectedBody = null;  //Disconnect the object from the tentacle
            springJoint.enabled = false;
            grabbedObject = null; //Clear the references
        }
    }

    public void dragObject()
    {
        if (grabbedObject != null)
        {

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 constainedPos = constraintsTarget(mousePos); //Apply constraints.

            //Move tentacle smoothly toward constrained position.
            transform.position = Vector2.Lerp(transform.position, constainedPos, Time.deltaTime * springFrequency);

            //Calculate spring like force to pull the object.
            Vector2 displacement = (Vector2)transform.position - (Vector2)grabbedObject.transform.position;
            Vector2 damping = -grabbedObject.linearVelocity * springDamping; //Damp the movement to avoid any sudden forces.
            Vector2 force = displacement * springFrequency + damping;
            grabbedObject.AddForce(force, ForceMode2D.Force); //Apply force.
            isExtending = false;
            isRetracting = false;
        }

    }
}

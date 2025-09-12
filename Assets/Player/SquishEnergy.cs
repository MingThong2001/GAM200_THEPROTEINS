using System.Collections.Generic;
using UnityEngine;

public class SquishEnergy : MonoBehaviour
{
  

    //Charge Settings
    public float currentCharge = 0f;
    public float maxCharge = 5f;
    public float chargingspeed = 1f; //How fast the charge increase.
    private bool isCharging = false;

    //Visual Squish
    public float squishAmount = 0.5f;
    public bool usesplineDistortion = true;

    //Launch
    public float maxlaunchForce = 20f;
    public bool isLaunched = false;

    //References
    private Rigidbody2D rb;
    private List<Rigidbody2D> segmentBodies = new List<Rigidbody2D>();
    private SoftBodyPhyiscs softBodyPhyiscs;

    //Recoil
    public float knockbackForce;
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        softBodyPhyiscs = GetComponent<SoftBodyPhyiscs>();

        if (softBodyPhyiscs != null)
        {
           /*Transform[] segments = softBodyPhyiscs.GetSegments();
            for (int i = 0; i < segments.Length; i++)
            { 
                Rigidbody2D rb2d = segments[i].GetComponent<Rigidbody2D>();
                if (rb2d != null)
                { 
                    segmentBodies.Add(rb2d);    
                }
            }*/

        }

    }

    public void Update()
    {
        handleEnergyCharging();
       
    }

    public void handleEnergyCharging()
    {
        if (Input.GetKey(KeyCode.Space) && currentCharge < maxCharge)
        {
                
                isCharging = true;
                currentCharge += chargingspeed * Time.deltaTime;
                currentCharge = Mathf.Min(currentCharge, maxCharge); //Clamp the charge to maxcharge.

                //Applying squish effect
                float squishfactor = Mathf.Lerp(1f, squishAmount, currentCharge/ maxCharge);

            if (usesplineDistortion && softBodyPhyiscs != null)
            { 
                float tangentStretch = Mathf.Lerp(0.1f, 1.5f, currentCharge / maxCharge);   
                softBodyPhyiscs.SetSquishFactor(tangentStretch);
            
            }

            
        }
        else if (Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            launch();
            isCharging = false;
        }
    }


    private void ApplySquishtoSegments(float scale)
    {
        for (int i = 0; i < segmentBodies.Count; i++ )
        { 
            Rigidbody2D segment = segmentBodies[i];
            segment.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
    public void launch()
    { 
        //Calculate the launch force based on the charge.
        float launchForce = Mathf.Lerp(0, maxlaunchForce, currentCharge/ maxCharge);
        
        Vector3 mouseworldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 launchDirection = (mouseworldPos - transform.position).normalized;



        //Apply the force to launch Gloo
        rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);

        for (int i = 0; i < segmentBodies.Count; i++)
        {
            Rigidbody2D segment = segmentBodies[i];
            segment.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
            segment.transform.localScale = Vector3.one;
        }

        //Reset  after launch.

        if (softBodyPhyiscs != null)
        {
            softBodyPhyiscs.SetSquishFactor(0.3f);
        }
        currentCharge = 0; 
        
    }
}

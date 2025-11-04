using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/*This script is not in use. It was for experimental purposes*/
public class SqusishJump : MonoBehaviour
{
    //Squeeze Charge Settings.
    public float chargeTime = 0f;
    public float maxChargeTime = 3f;
    public float chargeSpeed = 1f;
    private bool isCharging = false;

    //Squeeze Visual
    public float currentsquezeAmount = 0f;
    public float maxsqueezeAmount = 0.3f;
    public float maxLaunchForce = 5f;
    public float launchMultiplier = 0.5f;

    //Spring Settings
    public float normalspringFrequency = 1f; 
    public float squeezespringFrequency = 3f;

    //Jump Settingss
    public float jumpForce = 2f;

    //References and Collections.
    private Rigidbody2D rb;
    private List<Rigidbody2D> segmentedBodies = new List<Rigidbody2D>(); 
    private List<Vector3> originalScales = new List<Vector3>();
    private List<SpringJoint2D> springJoints = new List<SpringJoint2D>();
    private List<float> originalSpringFrequencies = new List<float>();
    private SoftBodyPhyiscs softBodyPhysics;


    //Jump Target
    private Vector2 launchTarget;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        softBodyPhysics = GetComponent<SoftBodyPhyiscs>();

        //Get all segment bodies
        Rigidbody2D[] allSegments = GetComponentsInChildren<Rigidbody2D>();

        foreach (Rigidbody2D segment in allSegments)
        {
            if (segment != null && segment != rb) //Skip the main Rigidbody (CentrePoint).
            {
                segmentedBodies.Add(segment);
                originalScales.Add(segment.transform.localScale); //Stores its original scale.
            }
        }

        //Get all spring joints
        SpringJoint2D[] allSprings = GetComponentsInChildren<SpringJoint2D>();
        foreach (SpringJoint2D spr in allSprings)
        {
            if (spr != null)
            {
                springJoints.Add(spr); //Add spring joints to the list - to prevent breakage.
                originalSpringFrequencies.Add(spr.frequency);

            }
        }
    }

    public void LateUpdate()
    {
        HandleCharging(); //Handle input and visuals for charging jump.

        if (Input.GetKeyDown(KeyCode.B))
        {
           // jump();
        }
    }
    public void HandleCharging()
    {
        if (!Input.GetKeyDown(KeyCode.B)) //Begin Charging with B.
        {
            StartCharging();
        }

        if (Input.GetKeyDown(KeyCode.B) && isCharging) //Continue Charging with B.
        {
            ContinueCharging();
        }
        if (Input.GetKeyUp(KeyCode.B) && isCharging) //Launch when B is released.
        {
            Launch();
        }

        UpdateSqueezeVisuals();
    }

    public void StartCharging()
    {
        isCharging = true;
        chargeTime = 0;
        Vector3 launchmosPos = Input.mousePosition; //Get current mouse position.
        launchmosPos.z = -Camera.main.transform.position.z;
        launchTarget = Camera.main.ScreenToWorldPoint(launchmosPos); //Convert mouse position to world position.
    }

    public void ContinueCharging()
    {
        chargeTime += Time.deltaTime * chargeSpeed; //Increase charge time based on frame and speed.
        chargeTime = Mathf.Min(chargeTime, maxChargeTime); //Clamp to maxchargetime.
        float chargePercentage = chargeTime / maxChargeTime;
    }

    public void UpdateSqueezeVisuals()
    {
        if (isCharging)
        {
            float chargePercentage = chargeTime / maxChargeTime; //Based on the calculated charge percentage, update current squish amount.
            currentsquezeAmount = chargePercentage;
        }
        else
        {
            currentsquezeAmount = Mathf.MoveTowards(currentsquezeAmount, 0f, 2f * Time.deltaTime); //Gradually return to normal.
        }
        applysqueezeEffects(); //Update visual and spring effects.
    }
    public void applysqueezeEffects()
    { 
        float currentScale = Mathf.Lerp(1f, maxsqueezeAmount, currentsquezeAmount); //Calculate squish scale.
        applysquishtoSegments(currentScale);

        float currentspringFreq = Mathf.Lerp(normalspringFrequency, squeezespringFrequency, currentsquezeAmount); //Calculate spring frequency

        applySpringFrequency(currentspringFreq);

        if (softBodyPhysics != null)
        {
            float tangentStretch = Mathf.Lerp(0.3f, 1.5f, currentsquezeAmount);
            softBodyPhysics.SetSquishFactor(tangentStretch); //Apply stretch to soft body physics.
        }
    }

    public void applysquishtoSegments (float scale)
    {

        for (int i = 0; i < segmentedBodies.Count; i++)
        {
            if (segmentedBodies[i] != null)
            {
                segmentedBodies[i].transform.localScale = originalScales[i] * scale;     //Apply scale to each segment.
            }
        }
    }
    public void applySpringFrequency(float frequency)
    {
        for (int i = 0; i < springJoints.Count; i++)
        {
            if (springJoints[i] != null)
            {
                springJoints[i].frequency = frequency; //Update spring frequency to each joint.
            }
        }
    }

    public void Launch()
    {
      
        float chargePercentage = chargeTime / maxChargeTime; //Calculate the charge jump.
        float launchForce = chargePercentage * maxLaunchForce * launchMultiplier; //Calculate the launch force.
        Vector2 launchDirection = (launchTarget - (Vector2)transform.position).normalized; //And the direction

        //Clamp force to prevent excessive launch (Controlled launch).
        launchForce = Mathf.Clamp(launchForce, 1f, 4f);

        
        //Apply launch force
        if (rb != null)
        {
            float originalGravity = rb.gravityScale; //Store current gravity.
            rb.gravityScale = 1f;

            //Apply launch force
            rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse); //Apply directional launch.
            rb.AddForce(Vector2.up * launchForce * 0.3f, ForceMode2D.Impulse); //Apply upward force to simulate jump.

            //Restore gravity after delay
            StartCoroutine(RestoreGravity(originalGravity));
        }

        //Segments force.
        foreach (Rigidbody2D segment in segmentedBodies) //Disable gravity for segments.
        {
            segment.gravityScale = 0f;
            segment.AddForce(launchDirection * (launchForce * 0.3f), ForceMode2D.Impulse);
            segment.AddForce(Vector2.up * (launchForce * 0.1f), ForceMode2D.Impulse);
        }
        //ResetCharging
        isCharging = false;
        chargeTime = 0f;

        StartCoroutine(ReEnableEverything()); //Renable springs after delay.
    }

        IEnumerator RestoreGravity (float originalGravity)
        { 
            
            yield return new WaitForSeconds(0.5f); //Wait before restoring gravity to simulate slim.


            if (rb != null)
            { 
                rb.gravityScale = originalGravity; //Restore gravity.
            }

            foreach (Rigidbody2D segment in segmentedBodies)
            {
                if (segment != null)
                {
                    segment.gravityScale = 1f;
                }
            }
        }

    IEnumerator ReEnableEverything()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (SpringJoint2D spring in springJoints)
        {
            if (spring != null)
            {
                spring.enabled = true;
            }
        }

        for (int i = 0; i < springJoints.Count; i++)
        {
        
            if (springJoints[i] != null && i < originalSpringFrequencies.Count)
            {

                springJoints[i].frequency = originalSpringFrequencies[i];
            }
        }
    }
    

}


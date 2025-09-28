using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishEnergy : MonoBehaviour
{
    [Header("Dynamic Charge Settings")]
    public float chargeTime = 0f; // How long B has been held
    public float maxChargeTime = 3f; // Maximum charge time for max force
    public float chargeSpeed = 1f; // How fast it charges (1 = 1 second = 1 charge)
    private bool isCharging = false;

    [Header("Visual Squeeze")]
    public float maxSqueezeAmount = 0.3f; // How much to squeeze when fully charged
    public float currentSqueezeAmount = 0f; // Current squeeze level (0-1)

    [Header("Launch Settings")]
    public float maxLaunchForce = 100f;
    public float launchMultiplier = 5f;
    public bool debugMode = true;

    [Header("Spring Settings")]
    public float normalSpringFrequency = 1f;
    public float squeezedSpringFrequency = 3f;

    [Header("Test Settings")]
    public KeyCode testJumpKey = KeyCode.T;
    public float testForce = 50f;

    // References
    private Rigidbody2D rb;
    private List<Rigidbody2D> segmentBodies = new List<Rigidbody2D>();
    private List<Vector3> originalScales = new List<Vector3>();
    private List<SpringJoint2D> springJoints = new List<SpringJoint2D>();
    private List<float> originalSpringFrequencies = new List<float>();
    private SoftBodyPhyiscs softBodyPhysics;

    private Vector2 launchTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        softBodyPhysics = GetComponent<SoftBodyPhyiscs>();

        Debug.Log($"=== DYNAMIC CHARGE SYSTEM START ===");
        Debug.Log($"Main Rigidbody found: {rb != null}");

        if (rb != null)
        {
        }

        // Get all segment bodies
        Rigidbody2D[] allSegments = GetComponentsInChildren<Rigidbody2D>();
        Debug.Log($"Total Rigidbodies found: {allSegments.Length}");

        foreach (Rigidbody2D segment in allSegments)
        {
            if (segment != null && segment != rb)
            {
                segmentBodies.Add(segment);
                originalScales.Add(segment.transform.localScale);
            }
        }

        // Get all spring joints
        SpringJoint2D[] allSprings = GetComponentsInChildren<SpringJoint2D>();
        Debug.Log($"Total SpringJoints found: {allSprings.Length}");

        foreach (SpringJoint2D spring in allSprings)
        {
            if (spring != null)
            {
                springJoints.Add(spring);
                originalSpringFrequencies.Add(spring.frequency);
            }
        }

        Debug.Log($"=== SETUP COMPLETE - Segments: {segmentBodies.Count}, Springs: {springJoints.Count} ===");
    }

    void Update()
    {
        HandleDynamicCharging();

        // Test jump
        if (Input.GetKeyDown(testJumpKey))
        {
            TestJumpUp();
        }
    }

    void HandleDynamicCharging()
    {
        // Start charging when B is pressed
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCharging();
        }

        // Continue charging while B is held
        if (Input.GetKey(KeyCode.B) && isCharging)
        {
            ContinueCharging();
        }

        // Launch when B is released
        if (Input.GetKeyUp(KeyCode.B) && isCharging)
        {
            Launch();
        }

        // Update visual squeeze based on charge time
        UpdateSqueezeVisuals();
    }

    void StartCharging()
    {
        isCharging = true;
        chargeTime = 0f;

        // Get cursor position for launch target
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        launchTarget = Camera.main.ScreenToWorldPoint(mousePos);

        Debug.Log($"Started charging towards: {launchTarget}");
    }

    void ContinueCharging()
    {
        // Increase charge time
        chargeTime += Time.deltaTime * chargeSpeed;
        chargeTime = Mathf.Min(chargeTime, maxChargeTime); // Cap at max charge time

        // Calculate current charge percentage (0 to 1)
        float chargePercentage = chargeTime / maxChargeTime;

        if (debugMode && Time.frameCount % 30 == 0) // Debug every 30 frames
        {
            Debug.Log($"Charging: {chargeTime:F2}s/{maxChargeTime}s ({chargePercentage:P0})");
        }
    }

    void UpdateSqueezeVisuals()
    {
        if (isCharging)
        {
            // Squeeze more as charge time increases
            float chargePercentage = chargeTime / maxChargeTime;
            currentSqueezeAmount = chargePercentage;
        }
        else
        {
            // Return to normal when not charging
            currentSqueezeAmount = Mathf.MoveTowards(currentSqueezeAmount, 0f, 2f * Time.deltaTime);
        }

        // Apply visual effects based on current squeeze
        ApplySqueezeEffects();
    }

    void ApplySqueezeEffects()
    {
        // Calculate current scale based on squeeze amount
        float currentScale = Mathf.Lerp(1f, maxSqueezeAmount, currentSqueezeAmount);

        // Apply scale to segments
        ApplySquishToSegments(currentScale);

        // Apply spring frequency changes
        float currentSpringFreq = Mathf.Lerp(normalSpringFrequency, squeezedSpringFrequency, currentSqueezeAmount);
        ApplySpringFrequency(currentSpringFreq);

        // Apply soft body effects if available
        if (softBodyPhysics != null)
        {
            float tangentStretch = Mathf.Lerp(0.3f, 1.5f, currentSqueezeAmount);
            softBodyPhysics.SetSquishFactor(tangentStretch);
        }
    }

    void ApplySquishToSegments(float scale)
    {
        for (int i = 0; i < segmentBodies.Count; i++)
        {
            if (segmentBodies[i] != null)
            {
                segmentBodies[i].transform.localScale = originalScales[i] * scale;
            }
        }
    }

    void ApplySpringFrequency(float frequency)
    {
        for (int i = 0; i < springJoints.Count; i++)
        {
            if (springJoints[i] != null)
            {
                springJoints[i].frequency = frequency;
            }
        }
    }

    void Launch()
    {
        // Calculate launch force based on charge TIME
        float chargePercentage = chargeTime / maxChargeTime;
        float launchForce = chargePercentage * maxLaunchForce * launchMultiplier;
        Vector2 launchDirection = (launchTarget - (Vector2)transform.position).normalized;

        Debug.Log($"=== LAUNCHING DYNAMICALLY ===");
        Debug.Log($"Charge Time: {chargeTime:F2}s / {maxChargeTime}s");
        Debug.Log($"Charge Percentage: {chargePercentage:P1}");
        Debug.Log($"Launch Force: {launchForce}");
        Debug.Log($"Direction: {launchDirection}");

        // Use minimum force if charge is too low
        if (launchForce < 20f)
        {
            launchForce = 50f; // Minimum force for short taps
            Debug.Log($"Using minimum force: {launchForce}");
        }

        // Disable springs temporarily for clean launch
        foreach (SpringJoint2D spring in springJoints)
        {
            if (spring != null)
            {
                spring.enabled = false;
            }
        }

        // Apply launch force
        if (rb != null)
        {
            Debug.Log($"Main RB velocity before: {rb.linearVelocity}");

            // Temporarily reduce gravity
            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;

            // Apply force
            rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
            rb.AddForce(Vector2.up * launchForce * 0.3f, ForceMode2D.Impulse); // Extra upward boost

            Debug.Log($"Main RB velocity after: {rb.linearVelocity}");

            // Restore gravity after delay
            StartCoroutine(RestoreGravity(originalGravity));
        }

        // Apply to segments
        foreach (Rigidbody2D segment in segmentBodies)
        {
            if (segment != null)
            {
                segment.gravityScale = 0f; // Reduce gravity
                segment.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
                segment.AddForce(Vector2.up * launchForce * 0.3f, ForceMode2D.Impulse);
            }
        }

        // Reset charging state
        isCharging = false;
        chargeTime = 0f;

        // Re-enable springs after delay
        StartCoroutine(ReEnableEverything());
    }

    IEnumerator RestoreGravity(float originalGravity)
    {
        yield return new WaitForSeconds(0.5f);

        if (rb != null)
        {
            rb.gravityScale = originalGravity;
        }

        foreach (Rigidbody2D segment in segmentBodies)
        {
            if (segment != null)
            {
                segment.gravityScale = 1f;
            }
        }

        Debug.Log("Gravity restored");
    }

    IEnumerator ReEnableEverything()
    {
        yield return new WaitForSeconds(0.3f);

        foreach (SpringJoint2D spring in springJoints)
        {
            if (spring != null)
            {
                spring.enabled = true;
            }
        }

        // Reset spring frequencies
        for (int i = 0; i < springJoints.Count; i++)
        {
            if (springJoints[i] != null && i < originalSpringFrequencies.Count)
            {
                springJoints[i].frequency = originalSpringFrequencies[i];
            }
        }

        Debug.Log("Springs re-enabled");
    }

    void TestJumpUp()
    {
        Debug.Log($"=== TEST JUMP ===");

        Vector2 upForce = Vector2.up * testForce;

        if (rb != null)
        {
            rb.AddForce(upForce, ForceMode2D.Impulse);
        }

        foreach (Rigidbody2D segment in segmentBodies)
        {
            if (segment != null)
            {
                segment.AddForce(upForce, ForceMode2D.Impulse);
            }
        }

        Debug.Log($"Test jump applied");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 30), $"Hold B longer = more force. T for test jump");

        if (isCharging)
        {
            float chargePercentage = chargeTime / maxChargeTime;
            float previewForce = chargePercentage * maxLaunchForce * launchMultiplier;

            GUI.Label(new Rect(10, 40, 500, 30), $"CHARGING: {chargeTime:F1}s / {maxChargeTime}s ({chargePercentage:P0})");
            GUI.Label(new Rect(10, 70, 500, 30), $"Force: {previewForce:F0} | Squeeze: {currentSqueezeAmount:F2}");
            GUI.Label(new Rect(10, 100, 500, 30), $"Target: {launchTarget}");

            // Visual charge bar
            GUI.Box(new Rect(10, 130, 300, 20), "");
            GUI.Box(new Rect(10, 130, 300 * chargePercentage, 20), "CHARGE");
        }
        else
        {
            GUI.Label(new Rect(10, 40, 500, 30), $"Not charging. Squeeze returning: {currentSqueezeAmount:F2}");
        }

        if (rb != null)
        {
            GUI.Label(new Rect(10, 160, 500, 30), $"Velocity: {rb.linearVelocity} | BodyType: {rb.bodyType}");
        }
    }
}
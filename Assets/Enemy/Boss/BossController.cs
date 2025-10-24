using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("EMP Settings")]
    public float detectionRange = 5f;    // How close player must be to trigger EMP
    public float empDelay = 2f;        // Delay between each pulse
    public int totalPulses = 6;          // How many shockwave pulses to fire
    public float slowDuration = 2f;      // How long the paralysis lasts
    
    [Header("References")]
    public GameObject shockwaveEffect;   // Assign your EMPShockWave prefab
    public Transform playerTarget;       // Drag the Player here in Inspector
    private bool isFiring = false;
    
    private void Update()
    {
        //  Don’t do anything if there’s no player
        if (playerTarget == null)
            return;

        // Measure distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        Debug.Log("Distance to player: " + distanceToPlayer);

        //  Trigger EMP when player is within range
        if (distanceToPlayer <= detectionRange && !isFiring)
        {
            Debug.Log("Starting EMP sequence");

            StartCoroutine(EmpSequence());
        }
    }

    // Optional manual trigger (e.g., for testing)
    public void FireEMP()
    {
        if (!isFiring)
            StartCoroutine(EmpSequence());
    }

    private IEnumerator EmpSequence()
    {    
    
        isFiring = true;
        Debug.Log("EmpSequence started");
        for (int i = 0; i < totalPulses; i++)
        {
            FirePulse();
            yield return new WaitForSeconds(empDelay);
        }

        isFiring = false;
    }

    private void FirePulse()
    {
        Debug.Log("Trying to fire EMP!");
        if (shockwaveEffect != null)
        {
            // Spawn the expanding shockwave at boss position
            GameObject wave = Instantiate(shockwaveEffect, transform.position, Quaternion.identity);
            Debug.Log("Shockwave spawned: " + wave.name + ", active? " + wave.activeSelf);
            if (wave == null)
                Debug.LogError("Instantiation failed!");
            else
                Debug.Log("Shockwave spawned successfully!");

            wave.transform.SetParent(null); 
            wave.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            wave.transform.position = transform.position;

           
            // Pass paralysis duration to the wave script
            EMPShockWave emp = wave.GetComponent<EMPShockWave>();
            if (emp != null)
            {
               // emp.paralysisDuration = slowDuration;

                emp.followtarget = this.gameObject;
            }
            Debug.Log("EMP instantiated and component found? " + (emp != null));

        }
    }

    // Draw detection radius in Scene view for easy tuning
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

}


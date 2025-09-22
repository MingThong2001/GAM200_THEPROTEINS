using System.ComponentModel;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class MassSegment : MonoBehaviour
{
    [SerializeField] public MassStats gloomassStats = new MassStats();

    //Reference to components
    public PlayerMovement playermovement;
    public PlayerStats playerstats;
    private Rigidbody2D rb;
    private Rigidbody2D[] allrigidbodies;

    public HealthBarUI HealthbarUI;
    public int savedSegment;

    public void Awake()
    {
        if (playerstats == null)
        {
            playerstats = GetComponent<PlayerStats>();
        }
        if (playermovement == null)
        {
            playermovement = GetComponent<PlayerMovement>();
        }
        rb = GetComponent<Rigidbody2D>();
        allrigidbodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in allrigidbodies)
        {
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

        }
        UpdateAllStats();
    }
    public void handlepickupCollision(Collider2D coll)
    {
        Debug.Log("handlepickupCollision triggered!");

        gloopickup pickup = coll.GetComponentInParent<gloopickup>();
        Debug.Log("Pickup is: " + pickup);
        Debug.Log("Is Pickup Being Processed? " + pickup?.isBeingProcessed);

        if (pickup == null || pickup.isBeingProcessed) return;
        pickup.isBeingProcessed = true;
        bool processed = false;



       
        
            Debug.Log("Pickup is not being processed, continuing.");

            Debug.Log("Can add segment: " + canaddSegment());
            Debug.Log("Can remove segment: " + canremoveSegment());
            if (pickup.isAdditive)
            {
                if (canaddSegment())
                {
                    Debug.Log("Add Segments.");
                    AddSegment(pickup.segmentAmount);
                    processed = true;   
                    pickup.CollectPickup();

                }
              
            }
            else
            {
                if (canremoveSegment())
                {
                    Debug.Log("Remove Segments.");

                    RemoveSegment(pickup.segmentAmount);
                    pickup.CollectPickup();
                }
             

            }

        if (processed)
        {
            pickup.CollectPickup();
        }
        else
        {
            pickup.isBeingProcessed = false;
        }
        
    }
    #region Segment Count

    public bool canaddSegment()
    {
        if (gloomassStats.currentSegments < gloomassStats.maxSegments)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool canremoveSegment()
    {
        if (gloomassStats.currentSegments > gloomassStats.minSegments)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddSegment(int amount = 1)
    {
        int oldSegment = gloomassStats.currentSegments;
        int newsegmentCount = Mathf.Min(gloomassStats.maxSegments, oldSegment + amount);
        if (newsegmentCount != oldSegment)
        {
            gloomassStats.currentSegments = newsegmentCount;
            playerstats.currentSegments = newsegmentCount;

            savedSegment = gloomassStats.currentSegments;
            Debug.Log($"Segment Added! New segment count: {gloomassStats.currentSegments}");

            UpdateAllStats();
            LogStats("AddSegment");
        }
    }
    public void RemoveSegment(int amount = 1)
    {
        int oldSegment = gloomassStats.currentSegments;
        int newsegmentCount = Mathf.Max(gloomassStats.minSegments, oldSegment - amount);
        if (newsegmentCount != oldSegment)
        {
            gloomassStats.currentSegments = newsegmentCount;
            playerstats.currentSegments = newsegmentCount;

            savedSegment = gloomassStats.currentSegments ;
            Debug.Log($"Segment Added! New segment count: {gloomassStats.currentSegments}");
            UpdateAllStats();
            LogStats("RemoveSegment");
        }
    }
    public void SetSegmentCount(int count)
    {
        int newsegmentCount = Mathf.Clamp(count, gloomassStats.minSegments, gloomassStats.maxSegments);
        if (newsegmentCount != gloomassStats.currentSegments)
        {
            gloomassStats.currentSegments = newsegmentCount;
            UpdateAllStats();
        }
    }
    #endregion



    #region Stats Update
    public void UpdateAllStats()
    {
        gloomassStats.Updatemaxhealth();
        gloomassStats.Updatejumppower();
        gloomassStats.Updatemovespeed();

        UpdatemovementStats();
        UpdatehealthStats();
        UpdatephysicsMass();
        UpdateUi();
    }

    private void UpdatemovementStats()
    {
        if (playermovement != null && gloomassStats != null)
        {
            playermovement.normalmovementSpeed = gloomassStats.currentMoveSpeed;
            playermovement.puddlemovementSpeed = gloomassStats.currentMoveSpeed * 0.4f;
            playermovement.jumpforce = gloomassStats.currentJumpPower;
        }
    }

    private void UpdatehealthStats()
    {
        if (playerstats != null)
        {
            float healthPercentage = playerstats.GetMaxHealth() > 0 ? playerstats.GetCurrentHealth() / playerstats.GetMaxHealth() : 1f;
            playerstats.SetHealth(gloomassStats.currentMaxHealth, healthPercentage);

            if (HealthbarUI != null)
            {
                HealthbarUI.SetHealth(playerstats.GetCurrentHealth(), playerstats.GetMaxHealth());
            }

        }

    }
  
    public void UpdatephysicsMass()
    {
        foreach (Rigidbody2D rb in allrigidbodies)
        {
            if (rb != null)
            {
                rb.mass = gloomassStats.massPerSegment;

            }
        }
    }

    public void UpdateUi()
    {
        if (HealthbarUI != null && playerstats != null)
        {
            HealthbarUI.SetHealth(playerstats.GetCurrentHealth(), playerstats.GetMaxHealth());
        }

    }

    //private void UpdateCheckpoint(int newsegmentCount)
    //{
    //    CheckPoints checkpoint = GameObject.FindGameObjectWithTag("CheckPoints").GetComponent<CheckPoints>();
    //    if (checkpoint != null && checkpoint.isActivated)
    //    {
    //        checkpoint.UpdateCheckPointSegments(newsegmentCount);

    //    }

    //}


        #endregion


        //    #region Pickup
        //    private void OnTriggerEnter2D(Collider2D slimepickup)
        //    {
        //        Debug.Log("Trigger Enter this: " + slimepickup.gameObject.name);
        //        if (slimepickup.CompareTag("GlooSegment"))
        //        {
        //            gloopickup pickup = slimepickup.GetComponent<gloopickup>();
        //            if (pickup != null)
        //            { 
        //                pickupProcess(pickup);
        //            }
        //        }
        //    }

        //    private void pickupProcess(gloopickup pickup)
        //    {
        //        if (pickup.isAdditive && canaddSegment())
        //        {
        //            AddSegment(pickup.segmentAmount);
        //            pickup.CollectPickup();
        //        }
        //        else if (!pickup.isAdditive && canremoveSegment())
        //        {
        //            RemoveSegment(pickup.segmentAmount );
        //            pickup.CollectPickup(); 

        //        }
        //        Destroy(gameObject);

        //    }



        //#endregion

        //}
    private void LogStats(string action)
    {
        Debug.Log($"[{ action}] Segments: { gloomassStats.currentSegments}, " + $"MaxHealth: {gloomassStats.currentMaxHealth}, " + 
            $"PlayerHealth: {playerstats.GetCurrentHealth()}, " + $"MoveSpeed: {playermovement.normalmovementSpeed}, " + $"JumpForce: {playermovement.jumpforce}");
    
    }

    private void UpdateCheckpoint(int newSegmentCount)
    {
        // Find the active checkpoint and update its segment count
        CheckPoints checkpoint = GameObject.FindGameObjectWithTag("CheckPoints")?.GetComponent<CheckPoints>();
        if (checkpoint != null && checkpoint.isActivated)
        {
            checkpoint.UpdateCheckPointSegments(newSegmentCount);
            Debug.Log($"Checkpoint segment count updated to {newSegmentCount}");
        }
        else
        {
            Debug.LogWarning("No active checkpoint found or checkpoint is not activated.");
        }
    }

}

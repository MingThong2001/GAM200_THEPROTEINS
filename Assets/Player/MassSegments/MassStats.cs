using UnityEngine;

public class MassStats
{
    //Mass Configuration
    public int currentSegments = 13;
    public int minSegments = 12;
    public int maxSegments = 30;
    public float massPerSegment = 0.1f;

    //Base Stats
    public float basemoveSpeed = 5f;
    public float baseJump = 0.5f;
    public float baseHealth = 100f;

    //Mass Modifiers
    [Range(0f, 2f)]
    public float speedmodifierRange = 0.8f;

    [Range(0f, 2f)]
    public float jumpmodifierRange = 0.8f;

    [Range(0f, 2f)]
    public float healthmodifierRange = 0.8f;

    //Stats Update
    public float TotalMass;
    public float currentMoveSpeed;
    public float currentJumpPower;
    public float currentMaxHealth;

    //Calculated Properties
    public MassStats()
    {
        //MassSegment.UpdateAllStats();
    }
    public float MassRatio()
    { 
        return Mathf.Clamp01((float)(currentSegments - minSegments)/(maxSegments - minSegments));
    
    }

    //Modified Stats
    public void Updatemovespeed()
    {
        currentMoveSpeed = basemoveSpeed * (1f + MassRatio() * speedmodifierRange);
    }

    public void Updatejumppower()
    {
        currentJumpPower = baseJump * (1f + MassRatio() * jumpmodifierRange);
    }

    public void Updatemaxhealth()
    {
        currentMaxHealth = baseHealth * (1f + MassRatio() * healthmodifierRange);
    }
}

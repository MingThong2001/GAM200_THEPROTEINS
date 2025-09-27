using UnityEngine;
using UnityEngine.UI;

//Controls enemy's health bar UI above each of the enemy's head.
//It will updates the health bar fill then follows the enemy on screen.
public class EnemyHPUI : MonoBehaviour
{
    public Image fillbar; //Image component that shows visually to represent the enemy's HP.
    private Transform HptoenemyHP; //Target enemy transform so the healthbar will follow.
    private Vector3 Hptoenemyoffset; //The position offset between the enemy and the HP bar (To show it above the enemy head and for visual appeal).

    public void Initialize(Transform targettofollow, Vector3 folllowoffset) //Initializes the HP UI to follow a specific enemy and apply the offset.
    {
        HptoenemyHP = targettofollow; //Set the enemy transform to follow.
        Hptoenemyoffset = folllowoffset; //Set the offset for positioning the HP bar.

    }

    void LateUpdate() //Called after all update functions have been called. This is for the HPUI to follow.
    {
        if (HptoenemyHP != null)
        {
            transform.position = HptoenemyHP.position + Hptoenemyoffset; //Set the UI position to follow the enemy with the given offset.
            transform.rotation = Quaternion.identity;    //Keep the UI rotation fix so that it doesnt orientate or tilt with the enemy.
        }
    }

    public void DestroyenemyHPUI() //To destory the enemy hp ui when the game is reseted or restarted.
    {
        Destroy(gameObject);
        Debug.Log("HP bar destroyed");

    }
    public void SetHealth(int current, int max) //Update the fill amount of the HP bar based on the current health.
    {
        fillbar.fillAmount = (float)current / max;  //Calculate the HP and apply the amount.
        Debug.Log($"SetHealth called with current={current}, max={max}");


    }



}



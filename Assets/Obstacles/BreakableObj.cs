using UnityEngine;

public class BreakableObj : MonoBehaviour
{

    public float Objhealth = 10f;

    public void TakeDamage(float damage)
    { 
        Objhealth -= damage;
        if (Objhealth <= 0)
        {
            Break();
        }
    }
    private void Break()
    {
        //Insert break animation or what not here.

        Destroy(gameObject);
    }
}

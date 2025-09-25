using UnityEngine;
public class TestTrigger : MonoBehaviour
{
    private void OnCollisionEnter2D (Collision2D other)
    {
        //Debug.Log("Triggered by: " + other.name);
    }
}

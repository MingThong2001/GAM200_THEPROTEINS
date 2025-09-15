using UnityEngine;

public class TurnConstantly : MonoBehaviour
{
    Rigidbody rb;
    float rate = 5f;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        float rotation = rb.rotation.eulerAngles.z;
        rb.rotation = Quaternion.Euler(0,0,rotation + rate);
    }
}

using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, height;
    Vector2 startpos;
    public GameObject cam;
    public float parallaxEffectX, parallaxEffectY;
    void Start()
    {
        startpos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffectX));
        float distX = (cam.transform.position.x * parallaxEffectX);
        float distY = (cam.transform.position.y  * parallaxEffectY);
        transform.position = new Vector3(startpos.x + distX, startpos.y + distY, transform.position.z);
        if (temp > startpos.x + length)
        {
            startpos.x += length;
        }
        else if (temp < startpos.x - length)
        {
            startpos.x -= length;
        }
    }
}

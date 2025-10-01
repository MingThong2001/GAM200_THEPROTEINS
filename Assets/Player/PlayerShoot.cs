using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    //Projectile Settings
    public Projectile projectilePrefab;
    public Transform firePoint;
    public MassSegment massSegment;

    public float cooldown = 0.5f;
    public float maxDistance = 5f;
    public float speed = 10f;
    private float lastFiretime = 0f;
    public float damage = 2f;

    //References
    private AudioManager audioManager;

    private void Update()
    {
       
        if (Input.GetMouseButtonDown(0) && Time.time >= lastFiretime + cooldown && massSegment.GetCurrentSegments() > massSegment.GetMinSegments())
        {
            lastFiretime = Time.time;
            Shoot();
        }

    }
    private void Shoot()
    {
        //Remove segment before shooting
        //massSegment.RemoveSegment(massSegment.GetMaxSegments());
        massSegment.RemoveSegment(1);
        //Direction toward mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; 
        Vector3 direction = (mouseWorldPos - firePoint.position).normalized;

        //Spawn a projectile
        Projectile projrb = Instantiate(projectilePrefab);
        projrb.projectilemass = massSegment;
        //projrb.GetComponent<Projectile>().enabled = false;
        //if (projrb == null)
        //{
        //    Debug.LogError("Projectile was null! Make sure your pool is correctly initialized.");

        //    projrb = Instantiate(projectilePrefab).GetComponent<Projectile>();
        //}

        projrb.transform.position = firePoint.position;
        projrb.SetDirection(direction);
        projrb.maxDistance = 10f;

        projrb.damage = (int)damage;
        projrb.gameObject.tag = "Player";

     
        Debug.Log("Fired projectile at time: " + Time.time);

        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.shootOut);
        }

    }



}



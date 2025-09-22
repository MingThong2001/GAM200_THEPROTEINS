using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    //Projectile Settings
    public GameObject projectilePrefab;
    public Transform firePoint;

    public float cooldown = 0.5f;
    public float maxDistance = 5f;
    public float speed = 10f;
    private float lastFiretime = 0f;
    public float damage = 2f;



    private void Update()
    {

        if (Input.GetMouseButtonDown(0) && Time.time >= lastFiretime + cooldown)
        {
            Shoot();
            lastFiretime = Time.time;
        }
    }

   

    private void Shoot()
    {
        //Direction toward mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; 
        Vector3 direction = (mouseWorldPos - firePoint.position).normalized;

        //Spawn a projectile
        Projectile projrb = ProjectilePool.Instance.GetProjectile();
        if (projrb == null)
        {
            Debug.LogError("Projectile was null! Make sure your pool is correctly initialized.");

            projrb = Instantiate(projectilePrefab).GetComponent<Projectile>();
        }

        projrb.transform.position = firePoint.position;
        projrb.SetDirection(direction);
        projrb.maxDistance = 10f;

        projrb.damage = (int)damage;

        Collider2D[] playerColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] bulletColliders = projrb.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D playerCols in playerColliders)
        {
            foreach (Collider2D bulletCols in bulletColliders)
            {
                Physics2D.IgnoreCollision(bulletCols, playerCols);

            }


        }
        Debug.Log("Fired projectile at time: " + Time.time);


    }



}



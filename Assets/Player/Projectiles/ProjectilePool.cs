using UnityEngine;
using System.Collections.Generic;
public class ProjectilePool : MonoBehaviour
{

    //Singleton.
    public static ProjectilePool Instance;

    //Projectile Settings.
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectilepoolSize = 20;

    //List to store projectile in pool.
    private Queue<Projectile> pool = new Queue<Projectile> ();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        FillPool();
    }

    private void FillPool()
    {
        //Calculate how many projectiles we need to add.
        int projectCount = projectilepoolSize - pool.Count;
        for (int i = 0; i < projectCount; i++)
        { 
            //Create a new projectile.
            GameObject obj = Instantiate (projectilePrefab);

            //Disable if not in the scene.
            obj.SetActive(false);

            //Get the projectile component references.
            Projectile proj = obj.GetComponent<Projectile>();   

            //Queue Object.
            pool.Enqueue (proj);
        }
    }

    public Projectile GetProjectile()
    {
        //If no pool in the pool then refills the pool.
        if (pool.Count == 0) 
        {
            FillPool();
        }
        Projectile proj = pool.Dequeue (); //Take the next available projectile.
        proj.gameObject.SetActive(true);   //Activate the projectile so we can use in the scene.
        return proj;    
    }

    public void ReturnProjectile(Projectile proj)
    {
        proj.ResetProjectile(); //Reset projectile state.

        proj.gameObject.SetActive(false);
        pool.Enqueue (proj); //Put the projectile back to the pool.
    }
}


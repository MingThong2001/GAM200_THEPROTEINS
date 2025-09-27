using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class Enemy : MonoBehaviour
{
    //Detection Setting
    public float lineofsightdistance = 10f;
    public LayerMask obstaclelayer;
    public float maxchasedistance = 15f;

    //Enemy Stats
    public float movespeed = 2f;
    public float attackrange = 1.5f;
    public float attackCD = 2f;
    public float damageAmt = 10f;
    public float maxHealth = 20f;

    //Patrol Settings
    public float patroldistance = 3f;
    private Vector3 leftpatrolpoint;
    private Vector3 rightpatrolpoint;
    private bool movingright = true;

    //Health Settings
    public GameObject healthbarPrefab;
    public Image fillBar;
    private GameObject healthbarinstance;
    private Vector3 healthbarOffset = new Vector3(0, 1f, 0);

    //Transfrom
    private Transform player;
    private float currentHealth;
    private float lastattacktime;
    private bool isplayerrange;
    private Vector3 spawnPos;
    private bool returningtospawn = false;

    public void Start()
    {
        currentHealth = maxHealth;
        spawnPos = transform.position;

        //Patrol points
        leftpatrolpoint = spawnPos - new Vector3(patroldistance, 0f, 0f);
        rightpatrolpoint = spawnPos + new Vector3(patroldistance, 0f, 0f);

        player = GameObject.FindWithTag("Player")?.transform;

        CreateHealthBar();
        UpdateHealthBar();
    }

    private void Update()

    {   if (player == null)
        {
            Patrol();
            return;
        }
        
        checkplayerinsight();

        float distancefromspawn = Mathf.Abs(transform.position.x - spawnPos.x);
        
            if (distancefromspawn > maxchasedistance)
            {
                returningtospawn = true;
                isplayerrange = false;
            }
       

        if (returningtospawn)
        {
            returntospawnarea();
        }
        else if (isplayerrange)
        {
                ChasePlayer();
            
                attack();

        }
        else
        {
            Patrol();
        }
    }

    private void checkplayerinsight()
    {
        if (player == null)
        {
            isplayerrange = false;
            return;
        } 

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);


        if (distance > lineofsightdistance)
        {
            isplayerrange = false;
            return;
        }

        RaycastHit2D obstaclehit = Physics2D.Raycast(transform.position, direction, distance, obstaclelayer);
        if (obstaclehit.collider != null)
        {
            isplayerrange = false;
        }
        else
        {
            isplayerrange = true;
        }
    }

    private void ChasePlayer()
    { 
        float targetX = player.position.x;
        movehorizontally(targetX);
        fliptofaceplayer();
    }

    private void returntospawnarea()
    {
        float distancetospawn = Mathf.Abs(transform.position.x - spawnPos.x);
        if (distancetospawn > 0.5f)
        {
            movehorizontally(spawnPos.x);

            if (spawnPos.x > transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            { transform.localScale = new Vector3(-1, 1, 1); }
        }
        else
        {
            returningtospawn = false;
        }
    }


    private void movehorizontally(float x)
    { 
        float newx = Mathf.MoveTowards(transform.position.x, x ,movespeed * Time.deltaTime);
        transform.position = new Vector3(newx, transform.position.y, transform.position.z);
    }
    private void Patrol()
    { 
    
        float targetx = movingright ? rightpatrolpoint.x : leftpatrolpoint.x;   
        movehorizontally(targetx);

        //check if reached patrol point.
        if (Mathf.Abs(transform.position.x - targetx) < 0.05f)
        {
            movingright = !movingright;
        }

        //Flip to face patrol direction.
        transform.localScale = movingright ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
    }

    private void fliptofaceplayer()
    {
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);

        }
    }

    private void attack()
    {
        if (Time.time - lastattacktime >= attackCD)
        {
            float distancetoplayer = Vector2.Distance(transform.position, player.position);
            if (distancetoplayer <= attackrange)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                { stats.TakeDamage(damageAmt);}
                lastattacktime = Time.time;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Enemy collided with: {collision.collider.name}");

        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Player collision detected!");
            // Add these debug lines:
            Debug.Log($"Current time: {Time.time}, Last attack time: {lastattacktime}, Attack CD: {attackCD}");
            Debug.Log($"Time since last attack: {Time.time - lastattacktime}");
            Debug.Log($"Can attack? {Time.time - lastattacktime >= attackCD}");
            if (Time.time - lastattacktime >= attackCD)
            {

                PlayerStats stats = collision.collider.GetComponent<PlayerStats>();
                if (stats == null)
                {
                    stats = collision.collider.GetComponentInParent<PlayerStats>();
                    Debug.Log("Checking parent for PlayerStats...");


                }

                if (stats != null)
                {
                    Debug.Log($"Dealing {damageAmt} collision damage to player");

                    stats.TakeDamage(damageAmt);
                    lastattacktime = Time.time;
                }
            }
        }
    }

    private void Die()
    {
        if (healthbarinstance != null)
            Destroy(healthbarinstance);

        Destroy(gameObject, 0.5f);
    }

    #region HealthBar
    private void CreateHealthBar()
    {
        Vector3 spawnPos = transform.position + healthbarOffset;
        healthbarinstance = Instantiate(healthbarPrefab, spawnPos, Quaternion.identity);
        fillBar = healthbarinstance.transform.Find("Fill").GetComponent<Image>();
    }

    private void UpdateHealthBar()
    {
        if (fillBar != null)
            fillBar.fillAmount = currentHealth / maxHealth;
    }

    private void LateUpdate()
    {
        if (healthbarinstance != null)
            healthbarinstance.transform.position = transform.position + healthbarOffset;
    }
    #endregion
}

using NUnit.Framework;
using System.Diagnostics.Contracts;
using UnityEngine;
using System.Collections.Generic;

public enum GameState
{
    Play,
    Paused,
    GameOver,
    Restart,
 
}
public class GameManager : MonoBehaviour
{
    #region UI Elements
    private GameState currentState;
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject restartMenu;
    [SerializeField] private GameObject victoryMenu;
    #endregion

    public static GameManager instance;
    private CheckPoints checkPoints;    

   // public PlayerStats playerStats;
    public GameObject player;
    private GameObject[] collectibles;


    //Game State Flag
    public bool gamePaused = false;
    public bool gameOver = false;

    //CheckPoint
    private Vector2 checkpointPosition;
    private CheckPoints activecheckPoints;

    //Player stats
    private int checkpointHealth;
    private int checkpointSegmentcount;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyonLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //Initialize state and menus.
        currentState = GameState.Play;
        playMenu.SetActive(true);
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        restartMenu.SetActive(false);
        //Player body (list of body segments) For collectibles.



    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gamePaused && currentState == GameState.Play)
        {
            TogglePaused();
        }

        if (Input.GetKeyDown(KeyCode.R))
        { 
            RespawnPlayer();
        }

    }
    public void StartGame()
    {
        if (player != null)
        {
            Destroy(player);
        }

        //Instantiate Player
        //player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
       // playerStats = player.GetComponent<PlayerStats>();

        //Reset Game State
        currentState = GameState.Play;
        gamePaused = false;
        gameOver = false;

        playMenu.SetActive(false);
    }


    
    public void TogglePaused()
    {
        gamePaused = !gamePaused;

        if (gamePaused)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
        }
    }

    public void resumeGame()
    {
        gamePaused = false;
        currentState = GameState.Play;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void endGame(bool isVictory)
    {
        gameOver = true;
        currentState = GameState.GameOver;
        gamePaused = true;
        Time.timeScale = 0f;

        if (isVictory)
        {
            victoryMenu.SetActive(true);
        }
        else
        {
            gameOverMenu.SetActive(true);
        }
    }

    public void restartGame()
    {
        gameOver = false;
        gamePaused = false;
        Time.timeScale = 1f;

        cleanupScene();
    
        StartGame();
    }

    public void resetgameStats()
    {
        gamePaused = false;
        gameOver = false;

    }

  
    public void cleanupScene()
    {
        //Destroy all enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemies");
        foreach (GameObject enemy in enemies)
        { 
            Destroy(enemy);
        
        }

        GameObject[] objects = GameObject.FindGameObjectsWithTag("Objects");
        foreach (GameObject obj in objects)
        {
            Destroy(obj);

        }

        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectiles");
        foreach (GameObject proj in projectiles)
        {
            Destroy(proj);

        }

        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectibles");
        foreach (GameObject coll in collectibles)
        {
            Destroy(coll);

        }

        if (player != null)
        { 
            SoftBodyPhyiscs softbody = player.GetComponent<SoftBodyPhyiscs>();
            if (softbody != null)
            {
              //  softbody.Reset();
            }
        }



    }

    #region CHECKPOINTS

    public void RegisterPlayer(GameManager playerObj)
    {
     //   GameManager.RegisterPlayer(playerObj.gameObject);
    }
    public void SetCheckpoint(CheckPoints newCheckpoint, GameObject playerObj)
    {
        //Deactivate previous checkpoint.
        if (activecheckPoints != null && activecheckPoints != newCheckpoint)
        {
            activecheckPoints.Deactivate();
        }

        activecheckPoints = newCheckpoint;
        activecheckPoints.Activatethischeckpoint();
        checkpointPosition = activecheckPoints.transform.position;


        //Player Stats
        PlayerStats stats = playerObj.GetComponent<PlayerStats>();
        if (stats != null)
        {
            //  checkpointHealth = stats.health;



            //SoftBody Physics here

        }

    }

    public void RespawnPlayer()
    {
        if (player == null || activecheckPoints == null)
        {
            return;
        }

        //Move player to checkpoint
        player.transform.position = checkpointPosition;

        //Restore health.
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            //  stats.health = checkpointHealth;
        }

        //Restore Segments.
        SoftBodyPhyiscs softbody = player.GetComponent<SoftBodyPhyiscs>();
        if (softbody != null)
        {
            //  softbody.ResetSegments(checkpointSegmentcount);
        }
    }
    #endregion  

        #region Segmentation
    public void collectSegment(Transform newSegment)
    {
        SoftBodyPhyiscs softbodyphysic = GetComponent<SoftBodyPhyiscs>();
        if (softbodyphysic != null)
        { 
            softbodyphysic.AddNewSegment(newSegment);
            softbodyphysic.UpdateCollider();
        }


        

    }
    #endregion
}


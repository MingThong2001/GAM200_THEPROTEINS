using NUnit.Framework;
using System.Diagnostics.Contracts;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Define game states.
public enum GameState
{
    Play,
    Paused,
    GameOver,
    Restart,
    Showobjectivedelay,
    showobjective
 
}
public class GameManager : MonoBehaviour
{
    #region UI Elements
    private GameState currentState;
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject pauseMenu;
    //[SerializeField] private GameObject gameOverMenu;
    //[SerializeField] private GameObject restartMenu;
    [SerializeField] private GameObject victoryMenu;
    [SerializeField] private GameObject objectivePanel;
    #endregion

    //Players and checkpoints.
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

    //Reset Door
    public Door[] doors;
    public DoorButton[] doorsButton;

    //References
    public PlayerShoot playershoot;
    public PlayerMovement playermovement;
    private Rigidbody2D playerRigid;

 
    public void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Duplicate GameManager found! Destroying: " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        Debug.Log("GameManager Awake on object: " + gameObject.name);

        //Initialize state and menus.
        currentState = GameState.Play;
        playMenu.SetActive(true);
        pauseMenu.SetActive(false);
        victoryMenu.SetActive(false);

        //gameOverMenu.SetActive(false);
        //restartMenu.SetActive(false);
        objectivePanel.SetActive(false);

    }

    //Get player controller.
    public void Start()
    {
        if (player != null)
        { 
            playermovement = player.GetComponentInParent<PlayerMovement>();
            playershoot = player.GetComponentInParent<PlayerShoot>();


        }

    }
    public void Update()
    {
        //Toggle Pause on esc key.
        Debug.Log("Pause");
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Play || currentState == GameState.Paused)
            {
                TogglePaused();
            }
        }

        //Disable playermovement and shoot when UI panel appears.
        if (playMenu.activeSelf || victoryMenu.activeSelf || objectivePanel.activeSelf)
        {
            if (playermovement != null)
            {
                playermovement.enabled = false;
            }
            if (playershoot != null)
            {
                playershoot.enabled = false;
            }

        }
        else //Enable them back if its in the playstate.
        {
            if (currentState == GameState.Play)
            {
                if (playermovement != null && !playermovement.enabled)
                {
                    playermovement.enabled = true;
                }
                if (playershoot != null && !playershoot.enabled)
                {
                    playershoot.enabled = true;
                }
            }
        }


    }

    #region Start
    public void StartGame()
    {
       
        playMenu.SetActive(false);
        objectivePanel.SetActive(true);
        currentState = GameState.showobjective;

        gamePaused = true;
        gameOver = false;

        Time.timeScale = 0f;


    }
   

    public void afterObjective()
    {
        objectivePanel.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;
        currentState = GameState.Play;

    }
    #endregion



    #region Pause & Resume
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
        if (currentState == GameState.Play)
        {
            if (playermovement != null && !playermovement.enabled)
            {
                playermovement.enabled = true;
            }
            if (playershoot != null && !playershoot.enabled)
            {
                playershoot.enabled = true;
            }
        }
    }

    #endregion

    #region End Cycle
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
        else if (!isVictory)
        {
            victoryMenu.SetActive(false);
        }
        else
        {
            resetVictory();
        }
    }
    public void resetVictory()
    {
        gameOver = false;
        currentState = GameState.Play;
        victoryMenu.SetActive(false);
        ResetDoorandbutton();
        }

    public void BackToStartMenu()
    {
        cleanupScene();

        victoryMenu.SetActive(false);
        pauseMenu.SetActive(false);
        objectivePanel.SetActive(false);

        
        Time.timeScale = 0f;
        gameOver = false;
        gamePaused = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        currentState = GameState.Play;
    }

    public void restartGame()
    {
        Debug.Log("StartGame() CALLED");

        ResetDoorandbutton();

        victoryMenu.SetActive(false);
        objectivePanel.SetActive(false);
        pauseMenu.SetActive(false);

        currentState = GameState.Play;

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        cleanupScene();
    
        resetVictory();
    }

    public void ResetDoorandbutton()
    {
        foreach (Door door in doors)
        {
            door.ResetDoor();
            door.Playerinreange = false;

        }

        foreach (DoorButton button in doorsButton)
        {
            button.ResetButton();
        }
    }
    public void resetgameStats()
    {
        gamePaused = false;
        gameOver = false;

    }

  
    public void cleanupScene()
    {
        ////Destroy all enemies
        //GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemies");
        //foreach (GameObject enemy in enemies)
        //{ 
        //    Destroy(enemy);
        
        //}

        //GameObject[] objects = GameObject.FindGameObjectsWithTag("Objects");
        //foreach (GameObject obj in objects)
        //{
        //    Destroy(obj);

        //}

        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject proj in projectiles)
        {
            if (proj.name.Contains("projectileV3(Clone)"))
            {

                Destroy(proj);
            }
          

        }

        //GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectibles");
        //foreach (GameObject coll in collectibles)
        //{
        //    Destroy(coll);

        //}

        //if (player != null)
        //{ 
        //    SoftBodyPhyiscs softbody = player.GetComponent<SoftBodyPhyiscs>();
        //    if (softbody != null)
        //    {
        //      //  softbody.Reset();
        //    }
        //}



    }

    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion
  
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


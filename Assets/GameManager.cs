using NUnit.Framework;
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
    [SerializeField] private GameObject gameOverMenu;
    //[SerializeField] private GameObject restartMenu;
    [SerializeField] private GameObject victoryMenu;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject SettingsPanel;

    [SerializeField] private GameObject[] inGameText;
    #endregion

    //Settings
    private GameObject previousMenu;

    //Players and checkpoints.
    public static GameManager instance;
    [SerializeField] private CheckPoints checkPoints;

    // public PlayerStats playerStats;
    public GameObject player;
    [SerializeField] private  Transform playerspawnPos;


    //Game State Flag
    public bool gamePaused = false;
    public bool gameOver = false;

    //CheckPoint
    private Vector2 checkpointPosition;
    private CheckPoints activecheckPoints;

    //Enemies
    [SerializeField] EnemyPatrol enemyPatrol;

    [SerializeField] private Drone drone;

    [SerializeField] public HelperWelper helperWelper;

    [SerializeField] private FailedSubjects failedSubjects;
    [SerializeField] ChargePatrol chargePatrol;




    //CheckPoints
    private CheckPoints checkpoint;
    private int checkpointHealth;
    private int checkpointSegmentcount;

    //Reset Door
    public Door[] doors;
    public DoorButton[] doorsButton;

    //References
    public PlayerShoot playershoot;
    public PlayerMovement playermovement;
    public SceneController sceneController;
    private Rigidbody2D playerRigid;
      


    public void Awake()
    {
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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

        foreach (GameObject text in inGameText)
        {
            text.SetActive(true);
        }

    
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


       bool uiActive = (playMenu.activeSelf || victoryMenu.activeSelf || objectivePanel.activeSelf);
       playermovement.enabled = !uiActive;
       playershoot.enabled = !uiActive;

        foreach (GameObject text in inGameText)
        {
            text.SetActive(!uiActive);
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
        SpawnPlayer();

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

            // Disable movement and shooting when paused
            if (playermovement != null)
                playermovement.enabled = false;

            if (playershoot != null)
                playershoot.enabled = false;
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
            currentState = GameState.Play;

            // Re-enable movement and shooting when unpaused
            if (playermovement != null)
                playermovement.enabled = true;

            if (playershoot != null)
                playershoot.enabled = true;
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


    #region Player/Enemies
    private void SpawnPlayer()
    {
        if (player != null && playerspawnPos != null) 
        {
            player.transform.position = playerspawnPos.position;

        }
    }

    public void spawnHelperWelper()
    {
        if (helperWelper != null && helperWelper.enemyPatrol != null)
        {
            helperWelper.enemyPatrol.SpawnAtPointHelperWelper();
        }
    }


    public void spawnDrone()
    {
        if (drone != null && drone.enemypatrol != null)
        {
            drone.enemypatrol.SpawnAtPointDrone();
        }
    }


    public void spawnFailedSubjects()
    {
        if (failedSubjects != null && failedSubjects.chargePatrol != null)
        {
            failedSubjects.chargePatrol.SpawnAtPointFailedSubject();
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
            // Win Condition
            victoryMenu.SetActive(true);
            gameOverMenu.SetActive(false);
        }
        else
        {
            // Lose Condition
            victoryMenu.SetActive(false);
            gameOverMenu.SetActive(true);
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
        Debug.Log("BackToStartMenu called");

    // Reset time scale
    Time.timeScale = 0f;

    // Reset game state
    gamePaused = true;
    gameOver = false;
    currentState = GameState.Play;

    // Clean up any scene elements like enemies, projectiles, etc.
    cleanupScene();

    // Reset checkpoints
    activecheckPoints = null;
    checkpointPosition = Vector2.zero;
    CheckPoints.allCheckPoints.Clear();

    // Disable all gameplay UI
    if (pauseMenu != null) pauseMenu.SetActive(false);
    if (victoryMenu != null) victoryMenu.SetActive(false);
    if (gameOverMenu != null) gameOverMenu.SetActive(false);
    if (objectivePanel != null) objectivePanel.SetActive(false);
    if (SettingsPanel != null) SettingsPanel.SetActive(false);

    // Show main menu UI
    if (playMenu != null) playMenu.SetActive(true);

    // Disable player controls
    if (playermovement != null) playermovement.enabled = false;
    if (playershoot != null) playershoot.enabled = false;

    // Hide in-game text
    foreach (GameObject text in inGameText)
    {
        text.SetActive(false);
    }
        Debug.Log("Returned to in-scene Main Menu.");
        spawnHelperWelper();
        spawnDrone();
        spawnFailedSubjects();
    }

    public void restartGame()
    {
        // Reset time scale
        gamePaused = false;
        gameOver = false;
        currentState = GameState.Play;

        // 2. Resume time
        Time.timeScale = 1f;

        // 3. Hide all menus
        if (playMenu != null) playMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (victoryMenu != null) victoryMenu.SetActive(false);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (objectivePanel != null) objectivePanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);

        // 4. Reset doors and buttons
        ResetDoorandbutton();

        // 5. Clean up scene (destroy projectiles, reset checkpoints, etc.)
        cleanupScene();

        // 6. Reset player
        if (player != null)
        {
            // Move to spawn or last checkpoint
            if (activecheckPoints != null)
            {
                player.transform.position = checkpointPosition;
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.restorefromCheckpoint(activecheckPoints);
                }
            }
            else if (playerspawnPos != null)
            {
                player.transform.position = playerspawnPos.position;
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.SetHealth(stats.GetMaxHealth(), 1f);
                }
            }

            // Enable movement and shooting
            if (playermovement != null) playermovement.enabled = true;
            if (playershoot != null) playershoot.enabled = true;
        }

        spawnHelperWelper();
        spawnDrone();
        spawnFailedSubjects();


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
        Debug.Log("Cleanup found");
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoints");
        foreach (GameObject obj in checkpointObjects)
        {
            CheckPoints cp = obj.GetComponent<CheckPoints>();
            if (cp != null)
            {
                cp.ResetCheckpoint();
            }
        }
        activecheckPoints = null;
        checkpointPosition = Vector2.zero;

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


    #region Settings Menu
    public void OpenSettings(GameObject fromMenu)
    {
        // Store the reference to the menu that opened settings
        previousMenu = fromMenu;

        // Hide that menu
        previousMenu.SetActive(false);

        // Show settings panel
        SettingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        // Hide settings
        SettingsPanel.SetActive(false);

        // Return to previous menu if available
        if (previousMenu != null)
        {
            previousMenu.SetActive(true);
            previousMenu = null;
        }
    }

    public void OpenSettingsFromPlayMenu()
    {
        OpenSettings(playMenu);
    }

    public void OpenSettingsFromPauseMenu()
    {
        OpenSettings(pauseMenu);
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


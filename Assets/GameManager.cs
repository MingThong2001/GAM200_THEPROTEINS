using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
    public static GameManager instance;

    #region UI Elements
    [Header("UI SETTINGS")]
    private GameState currentState;
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    //[SerializeField] private GameObject restartMenu;
    [SerializeField] private GameObject victoryMenu;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject SettingsPanel;
    //[SerializeField] private GameObject[] inGameText;
    private GameObject previousMenu;

    #endregion


    //Players
    [Header("Player SETTINGS")]
    public PlayerStats playerStats;
    public GameObject player;
    [SerializeField] public  Transform playerspawnPos;
    public static Transform PlayerTransform { get; private set; }


    //Game State Flag
    [Header("GameState SETTINGS")]
    public bool gamePaused = false;
    public bool gameOver = false;
    private static bool isRestarting = false;


    //CheckPoint
    [Header("CheckPoints SETTINGS")]
    [SerializeField] private CheckPoints checkPoints;
    private Vector2 checkpointPosition;
    private CheckPoints activecheckPoints;

    //CheckPoints
    private CheckPoints checkpoint;
    private int checkpointHealth;
    private int checkpointSegmentcount;

    [Header("Enemies SETTINGS")]
    [SerializeField] EnemyPatrol enemyPatrol;
    [SerializeField] private Drone drone;
    [SerializeField] EnemyPatrolDrone enemyPatrolDrone;

    [SerializeField] private HelperWelper helperWelper; // prefab in inspector
    [SerializeField] private EnemyPatrol helperWelperEnemyPatrol; // the EnemyPatrol script
    [SerializeField] private Transform helperWelperSpawnPos; // Add this to inspector



    [SerializeField] private FailedSubjects failedSubjects;
    [SerializeField] ChargePatrol chargePatrol;


   [Header("Obstacles SETTINGS")]
    //Reset Door
    public Door[] doors;
    public DoorButton[] doorsButton;

    [Header("References")]

    //References
    public PlayerShoot playershoot;
    public PlayerMovement playermovement;
    public SceneController sceneController;
    public PuddleController puddleController;
    [SerializeField] private GameObject playerPrefab; 

    public void Awake()
    {

        if (instance == null)
        {
            instance = this; // assign this instance
            player = null;
        }
        else
        {
            // optional: destroy duplicates in the same scene
            Destroy(gameObject);
        }
        Debug.Log("GameManager Awake on object: " + gameObject.name);
        if (playerPrefab == null) Debug.LogError("Player prefab NOT assigned!");
        if (playerspawnPos == null) Debug.LogError("Player spawn position NOT assigned!");
        currentState = GameState.Play;

        if (!isRestarting)
        {
            // Normal start, show main menu
            playMenu.SetActive(true);
            pauseMenu.SetActive(false);
            victoryMenu.SetActive(false);
            objectivePanel.SetActive(false);

            //foreach (GameObject text in inGameText)
            //    text.SetActive(true);
        }
        else
        {
            // Restarting gameplay, hide main menu
            playMenu.SetActive(false);
            pauseMenu.SetActive(false);
            victoryMenu.SetActive(false);
            objectivePanel.SetActive(false);

            //foreach (GameObject text in inGameText)
            //    text.SetActive(false);

            isRestarting = false; // reset flag
        }
    }

    //Get player controller.
    public void Start()
    {
        if (player == null)
        {
            SpawnPlayer();
        }
        else
        {
            // If the player already exists in the scene (from scene setup), assign references
            playermovement = player.GetComponent<PlayerMovement>();
            playershoot = player.GetComponent<PlayerShoot>();
            playerStats = player.GetComponent<PlayerStats>();
            puddleController = player.GetComponent<PuddleController>();
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


        bool uiActive = (playMenu.activeSelf || pauseMenu.activeSelf || victoryMenu.activeSelf || objectivePanel.activeSelf) || gamePaused;
        if (playermovement != null)
            playermovement.enabled = !uiActive;

        if (playershoot != null)
            playershoot.enabled = !uiActive;

        if (puddleController != null)
        { 
            puddleController.enabled = !uiActive;   
        }

        //foreach (GameObject text in inGameText)
        //{
        //    text.SetActive(!uiActive);
        //}
    }

    #region Start
    public void StartGame()
    {
        Debug.Log("[GameManager] StartGame() called");

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

            if (puddleController != null)
                puddleController.enabled = false;
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

            if (puddleController != null)
                puddleController.enabled = true;
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
            if (puddleController != null && !puddleController.enabled)
            {
                puddleController.enabled = true;
            }

        }
    }

    #endregion


    #region Player/Enemies
    public void SpawnPlayer()
    {
        Debug.Log("[GameManager] SpawnPlayer() called");


        if (playerPrefab == null)
        {
            Debug.LogError("[GameManager] playerPrefab is NULL!");
            return;
        }

        if (playerspawnPos == null)
        {
            Debug.LogError("[GameManager] playerspawnPos is NULL!");
            return;
        }
        // Reset stale reference
        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
        }

        if (player == null)
        {

            // Instantiate new player prefab at spawn position
            player = Instantiate(playerPrefab, playerspawnPos.position, Quaternion.identity);
            SetPlayer(player.transform);
            player.name = "Player";
            Debug.Log("[GameManager] Player spawned at: " + player.transform.position);

            // Assign references
            playermovement = player.GetComponentInParent<PlayerMovement>();
            playershoot = player.GetComponentInParent<PlayerShoot>();
            playerStats = player.GetComponentInParent<PlayerStats>();
            playerStats.healthBarUI = FindObjectOfType<HealthBarUI>();
            puddleController = player.GetComponentInParent<PuddleController>();

            // Initialize the player
            playerStats.InitializePlayer();
            // Reassign camera reference
            // Assign camera
            CameraController cam = Camera.main.GetComponent<CameraController>();
            if (cam != null)
            {
                cam.SetPlayer(player.transform);
            }
        }
        else
        { 
            player.transform.position = playerspawnPos.position;
            playerStats.InitializePlayer();
        }
    }

    public void spawnHelperWelper()
    {
        if (helperWelper != null && helperWelper.enemyPatrol != null)
        {
            helperWelper.enemyPatrol.SpawnAtPointDrone();
        }
    }


    public void spawnDrone()
    {
        if (drone != null && drone.enemyPatrolDrone != null)
        {
            drone.enemyPatrolDrone.SpawnAtPointDrone();
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
            Debug.Log("[GameManager] gameOverMenu active: " + gameOverMenu.activeSelf);

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

        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void restartGame()
    {
        isRestarting = true;       // Tell GameManager we are restarting gameplay
        Time.timeScale = 1f;
        gamePaused = false;
        gameOver = false;
        currentState = GameState.Play;

        // Get current scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Reload it cleanly
        SceneManager.LoadScene(currentScene.name);

    }

    #region Reset

    //private void DestroyAllEnemies()
    //{
    //    Debug.Log("[GameManager] Destroying all enemies...");

    //    // Find all HelperWelper scripts
    //    HelperWelper[] helpers = FindObjectsOfType<HelperWelper>(true);
    //    foreach (var helper in helpers)
    //    {
    //        // Destroy the ROOT parent (EnemyPatrol), not just the child
    //        Transform root = helper.transform.root;
    //        Debug.Log($"[GameManager] Destroying {root.name}");
    //        Destroy(root.gameObject);
    //    }
    //}


    //private void RespawnAllEnemies()
    //{
    //    Debug.Log("[GameManager] Respawning all enemies...");

    //    // Spawn new HelperWelper
    //    if (helperWelperPrefab != null)
    //    {
    //        spawnHelperWelper();
    //    }



    //}
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


    public void resetGameState()
    {
        gamePaused = false;
        gameOver = false;

        playerStats.InitializePlayer();
        spawnHelperWelper();


    }
    #endregion
    #endregion


    //public void cleanupScene()
    //{
    //    DestroyAllEnemies();

    //    Debug.Log("Cleanup found");
    //    GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoints");
    //    foreach (GameObject obj in checkpointObjects)
    //    {
    //        CheckPoints cp = obj.GetComponent<CheckPoints>();
    //        if (cp != null)
    //        {
    //            cp.ResetCheckpoint();
    //        }
    //    }
    //    activecheckPoints = null;

    //} 
    public void QuitGame()
    {
        ///Danielle said we need to have a confirmation panel for quiting.
        Application.Quit();
    }



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

    public void SetPlayer(Transform player)
    {
        PlayerTransform = player;
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


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
    Cutscene,
    Paused,
    GameOver,
    Restart,
    Showobjectivedelay,
    showobjective
 
}

//This is a static class to store persistent flags between scenes.
public static class GameFlags
{
    public static bool hasPlayedCutscene = false; //Track if this cutscene has been played.
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
    [SerializeField] private GameObject victoryMenu;
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject cutscenePanel;

    //[SerializeField] private GameObject[] inGameText;
    private GameObject previousMenu; //This store the menu that open settings either from pause menu or main menu.

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
    //private static bool hasCutscene = false;


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

    [SerializeField] private HelperWelper helperWelper; 
    [SerializeField] private EnemyPatrol helperWelperEnemyPatrol; 
    [SerializeField] private Transform helperWelperSpawnPos; 

    [SerializeField] private FailedSubjects failedSubjects;
    [SerializeField] ChargePatrol chargePatrol;


   [Header("Obstacles SETTINGS")]
    public Door[] doors;
    public DoorButton[] doorsButton;

    [Header("References")]
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

        //Default State.
        currentState = GameState.Play;

        //UI setup for normal start or restart.
        if (!isRestarting)
        {
            //If start normally, show main menu.
            playMenu.SetActive(true);
            pauseMenu.SetActive(false);
            victoryMenu.SetActive(false);
            objectivePanel.SetActive(false);
            if (cutscenePanel != null) cutscenePanel.SetActive(false);

            //foreach (GameObject text in inGameText)
            //    text.SetActive(true);
        }
        else
        {
            //If restart, hide main menu.
            playMenu.SetActive(false);
            pauseMenu.SetActive(false);
            victoryMenu.SetActive(false);
            objectivePanel.SetActive(false);
            if (cutscenePanel != null) cutscenePanel.SetActive(false);

            //foreach (GameObject text in inGameText)
            //    text.SetActive(false);

            isRestarting = false; // reset flag
        }
    }

    public void Start()
    {
        if (player == null)
        {
            SpawnPlayer();
        }
        else
        {
            //If the player already exists in the scene (from scene setup), assign references.
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

        //Disable player controls when any UI is active.
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
        #region old code
        //if (cutscenePanel != null)
        //{
        //    cutscenePanel.SetActive(true);
        //    objectivePanel.SetActive(false);
        //    currentState = GameState.Paused;
        //    gamePaused = true;
        //    Time.timeScale = 0f;
        //}
        //else
        //{
        //    //Scenes without cutscene go straight to objective
        //    objectivePanel.SetActive(true);
        //    currentState = GameState.showobjective;
        //    gamePaused = true;
        //    Time.timeScale = 0f;
        //}

        //gameOver = false;
        //SpawnPlayer();
        #endregion

        //Hide menu.
        playMenu.SetActive(false);

        // Check if the scene has a cutscene panel and the cutscene has not been played yet.
        if (cutscenePanel != null && !GameFlags.hasPlayedCutscene)
        {
            //If first time showing, showcut scene.
            cutscenePanel.SetActive(true);
            objectivePanel.SetActive(false);
            currentState = GameState.Cutscene;
            gamePaused = true;
            Time.timeScale = 0f;
            GameFlags.hasPlayedCutscene = true; //We then marked it as flagged.
        }
        else
        {
            //If restart or scene without any cutscene, start from objective panel.
            objectivePanel.SetActive(true);
            currentState = GameState.showobjective;
            gamePaused = true;
            Time.timeScale = 0f;
        }

        gameOver = false;
        SpawnPlayer();
    }

    //To transit it to play state.
    public void afterObjective()
    {
        objectivePanel.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;
        currentState = GameState.Play;

    }

    //When cutscene is complete.
    public void OnCutsceneComplete()
    {
        //Hide cutscene panel.
        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        //Then show obejctive panel
        objectivePanel.SetActive(true);

        currentState = GameState.showobjective;
        gamePaused = true;
        Time.timeScale = 0f;
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

            //We disable any interaction here.
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

            // And reEnable it afterward.
            if (playermovement != null)
                playermovement.enabled = true;

            if (playershoot != null)
                playershoot.enabled = true;

            if (puddleController != null)
                puddleController.enabled = true;
        }
    }


    //Resume game from pause menu.
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

        //Reset stale reference. (Safety-net).
        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
        }

        if (player == null)
        {

            //Instantiate new player prefab at spawn position.
            player = Instantiate(playerPrefab, playerspawnPos.position, Quaternion.identity);
            SetPlayer(player.transform);
            player.name = "Player";
            Debug.Log("[GameManager] Player spawned at: " + player.transform.position);

            //Assign key references.
            playermovement = player.GetComponentInParent<PlayerMovement>();
            playershoot = player.GetComponentInParent<PlayerShoot>();
            playerStats = player.GetComponentInParent<PlayerStats>();
            playerStats.healthBarUI = FindObjectOfType<HealthBarUI>();
            puddleController = player.GetComponentInParent<PuddleController>();

            //Initialize the player.
            playerStats.InitializePlayer();

            // Assign camera
            CameraController cam = Camera.main.GetComponent<CameraController>();
            if (cam != null)
            {
                cam.SetPlayer(player.transform);
            }
        }
        else
        { 
            //Reset position for existing player.
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
           //Win condition.
            victoryMenu.SetActive(true);
            gameOverMenu.SetActive(false);
        }
        else
        {
            Debug.Log("[GameManager] gameOverMenu active: " + gameOverMenu.activeSelf);

         //Lose condition.
            victoryMenu.SetActive(false);
            gameOverMenu.SetActive(true);
        }

    }


    //Reset vicotry menu and doors (not sure if this needed - tbc).
    public void resetVictory()
    {
        gameOver = false;
        currentState = GameState.Play;
        victoryMenu.SetActive(false);
        ResetDoorandbutton();
    }


    //Back to main menu (reload the current scene)
    public void BackToStartMenu()
    {
        Debug.Log("BackToStartMenu called");

        Time.timeScale = 1f;

        // Reload current scene
        GameFlags.hasPlayedCutscene = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    //Restart the current level.
    public void restartGame()
    {
        isRestarting = true;       
        Time.timeScale = 1f;
        gamePaused = false;
        gameOver = false;
        currentState = GameState.Play;

        //Get current scene
        Scene currentScene = SceneManager.GetActiveScene();

        //Reload it cleanly
        SceneManager.LoadScene(currentScene.name);

    }

    #region Reset

    #region oldcode
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

    #endregion 


    //Reset doorbutton state.
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
        }
        previousMenu = null;
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


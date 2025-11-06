using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.WSA;

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
    public static bool hasPlayedCutscene = false;
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
    private GameObject previousMenu;
    #endregion

    //Players
    [Header("Player SETTINGS")]
    public PlayerStats playerStats;
    public GameObject player;
    [SerializeField] public Transform playerspawnPos;
    public static Transform PlayerTransform { get; private set; }

    //Game State Flag
    [Header("GameState SETTINGS")]
    public bool gamePaused = false;
    public bool gameOver = false;
    private static bool isRestarting = false;
    public float delay = 2f;

    //CheckPoint
    [Header("CheckPoints SETTINGS")]
    [SerializeField] private CheckPoints checkPoints;
    private Vector2 checkpointPosition;
    private CheckPoints activecheckPoints;
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
    public SpriteShapeController spriteShapeController;
    public Playerline playerline;
    [SerializeField] private GameObject playerPrefab;

    [Header("Audio Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        player = null;

        Debug.Log("GameManager Awake on object: " + gameObject.name);

        if (playerPrefab == null) Debug.LogError("Player prefab NOT assigned!");
        if (playerspawnPos == null) Debug.LogError("Player spawn position NOT assigned!");

        currentState = GameState.Play;

        //If not restarting, toggle these panels.
        if (!isRestarting)
        {
            if (playMenu != null) playMenu.SetActive(true);
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (victoryMenu != null) victoryMenu.SetActive(false);
            if (objectivePanel != null) objectivePanel.SetActive(false);
            if (cutscenePanel != null) cutscenePanel.SetActive(false);
        }
        else
        {
            if (playMenu != null) playMenu.SetActive(false);
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (victoryMenu != null) victoryMenu.SetActive(false);
            if (objectivePanel != null) objectivePanel.SetActive(false);
            if (cutscenePanel != null) cutscenePanel.SetActive(false);
            isRestarting = false;
        }
    }

    public void Start()
    {
        //To find and autobind the audio sliders.
        if (AudioManager.instance != null)
        {
            if (musicSlider == null)
            {
                GameObject obj = GameObject.FindWithTag("MusicSlider");
                if (obj != null) musicSlider = obj.GetComponent<Slider>();
            }

            if (sfxSlider == null)
            {
                GameObject obj = GameObject.FindWithTag("SFXSlider");
                if (obj != null) sfxSlider = obj.GetComponent<Slider>();
            }

            AudioManager.instance.BindSliders(musicSlider, sfxSlider);
            Debug.Log("[GameManager] Audio sliders bound to AudioManager");
        }

        //Player initialization logic.
        if (player == null)
        {
            SpawnPlayer();
        }
        else
        {
            playermovement = player.GetComponentInParent<PlayerMovement>();
            playershoot = player.GetComponentInParent<PlayerShoot>();
            playerStats = player.GetComponentInParent<PlayerStats>();
            puddleController = player.GetComponentInParent<PuddleController>();
            playerline = player.GetComponentInParent<Playerline>();
            spriteShapeController = player.GetComponentInParent<SpriteShapeController>();
            CameraController cam = Camera.main?.GetComponent<CameraController>();
            if (cam != null && player != null)
            {
                cam.SetPlayer(player.transform);
                Debug.Log("[GameManager] Camera reconnected to player");
            }
        }
    }

    public void Update()
    {
        //Pause logic (Toggle E for pause panel).
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Play || currentState == GameState.Paused)
            {
                TogglePaused();
            }
        }

        //If these UI panels are active, movmeent, shoot or puddle are disabled.
        bool uiActive =
            (playMenu != null && playMenu.activeSelf) ||
            (pauseMenu != null && pauseMenu.activeSelf) ||
            (victoryMenu != null && victoryMenu.activeSelf) ||
            (objectivePanel != null && objectivePanel.activeSelf) ||
            (SettingsPanel != null && SettingsPanel.activeSelf) ||
            gamePaused;

        if (playermovement != null) playermovement.enabled = !uiActive;
        if (playershoot != null) playershoot.enabled = !uiActive;
        if (puddleController != null) puddleController.enabled = !uiActive;
    }

    #region Start
    public void StartGame()
    {
        Debug.Log("[GameManager] StartGame() called");

        if (playMenu != null) playMenu.SetActive(false);

        if (cutscenePanel != null && !GameFlags.hasPlayedCutscene)
        {
            cutscenePanel.SetActive(true);
            if (objectivePanel != null) objectivePanel.SetActive(false);
            currentState = GameState.Cutscene;
            gamePaused = true;
            Time.timeScale = 0f;
            GameFlags.hasPlayedCutscene = true;
        }
        else
        {
            if (objectivePanel != null) objectivePanel.SetActive(true);
            currentState = GameState.showobjective;
            gamePaused = true;
            Time.timeScale = 0f;
        }

        gameOver = false;
        //SpawnPlayer();
    }

    public void afterObjective()
    {
        if (objectivePanel != null) objectivePanel.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;
        currentState = GameState.Play;
    }

    public void OnCutsceneComplete()
    {
        if (cutscenePanel != null) cutscenePanel.SetActive(false);
        if (objectivePanel != null) objectivePanel.SetActive(true);
        currentState = GameState.showobjective;
        gamePaused = true;
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
            if (pauseMenu != null) pauseMenu.SetActive(true);

            if (playermovement != null) playermovement.enabled = false;
            if (playershoot != null) playershoot.enabled = false;
            if (puddleController != null) puddleController.enabled = false;
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenu != null) pauseMenu.SetActive(false);
            currentState = GameState.Play;

            if (playermovement != null) playermovement.enabled = true;
            if (playershoot != null) playershoot.enabled = true;
            if (puddleController != null) puddleController.enabled = true;
        }
    }

    public void resumeGame()
    {
        gamePaused = false;
        currentState = GameState.Play;
        Time.timeScale = 1f;
        if (pauseMenu != null) pauseMenu.SetActive(false);

        if (currentState == GameState.Play)
        {
            if (playermovement != null && !playermovement.enabled) playermovement.enabled = true;
            if (playershoot != null && !playershoot.enabled) playershoot.enabled = true;
            if (puddleController != null && !puddleController.enabled) puddleController.enabled = true;
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

        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
        }

        //If player is null, we create player and put its relevant component in.
        if (player == null)
        {
            player = Instantiate(playerPrefab, playerspawnPos.position, Quaternion.identity);
            SetPlayer(player.transform);
            player.name = "Player";
            Debug.Log("[GameManager] Player spawned at: " + player.transform.position);

            playermovement = player.GetComponentInParent<PlayerMovement>();
            playershoot = player.GetComponentInParent<PlayerShoot>();
            playerStats = player.GetComponentInParent<PlayerStats>();
            if (playerStats != null) playerStats.healthBarUI = FindObjectOfType<HealthBarUI>();
            puddleController = player.GetComponentInParent<PuddleController>();
            playerline = player.GetComponentInParent<Playerline>();
            spriteShapeController = player.GetComponentInParent<SpriteShapeController>();

            if (playerStats != null) playerStats.InitializePlayer();
            CameraController cam = Camera.main?.GetComponent<CameraController>();
           
            //If camera is null, we set camera to the player.
            if (cam != null)
            {
                cam.SetPlayer(player.transform);
                Debug.Log("[GameManager] Camera set to follow player at: " + player.transform.position);
            }
            else
            {
                Debug.LogError("[GameManager] CameraController NOT FOUND on Main Camera!");
            }
        }
        else
        {   
            player.transform.position = playerspawnPos.position; //Reset position if exsit.
            if (playerStats != null) playerStats.InitializePlayer(); //Reinitialize.

            CameraController cam = Camera.main?.GetComponent<CameraController>();
            if (cam != null) cam.SetPlayer(player.transform); //Reconnect the camera.
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

        if (isVictory)
        {
            Debug.Log("[GameManager] Victory branch entered");

            if (playMenu != null) playMenu.SetActive(false);
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (gameOverMenu != null) gameOverMenu.SetActive(false);
            if (victoryMenu != null) victoryMenu.SetActive(false);
            if (objectivePanel != null) objectivePanel.SetActive(false);

            bool finallevel = isFinalLevel();
            Debug.Log($"[GameManager] isFinalLevel returned: {finallevel}");

            if (finallevel)
            {
                Debug.Log("[GameManager] Entering final level branch - activating victory menu");

                if (victoryMenu != null)
                {
                    Debug.Log($"[GameManager] Victory menu is NOT null, calling SetActive(true)");
                    victoryMenu.SetActive(true);
                    Debug.Log($"[GameManager] Victory menu active state: {victoryMenu.activeSelf}");
                }
                else
                {
                    Debug.LogError("[GameManager] Victory menu IS NULL!");
                }
            }
            else
            {
                Debug.Log("[GameManager] Not final level - loading next scene");
                if (victoryMenu != null) victoryMenu.SetActive(false);
                LoadNextScene();
            }
        }
        else
        {
            StartCoroutine(AfterDelay(0f));
        }

            Debug.Log("[GameManager] === ENDGAME END ===");
    }

    //To delay game over menu.
    private IEnumerator AfterDelay(float delay)
    {

        yield return new WaitForSeconds(0f);

        if (gameOverMenu !=null) gameOverMenu.SetActive(true);
    }
    private bool isFinalLevel()
    {
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int totalScenes = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        Debug.Log($"[GameManager] Current scene index: {currentSceneIndex}, Total scenes: {totalScenes}");

        // Return true if this is the last level (scene before going back to menu)
        return currentSceneIndex == totalScenes - 1;
    }

    //IEnumerator Afterdelay(float delay)
    //{
    //    yield return new WaitForSeconds(delay);

    //    if (gameOverMenu != null) gameOverMenu.SetActive(true); //Delay set active time 

    //}
    public void resetVictory()
    {
        gameOver = false;
        currentState = GameState.Play;
        if (victoryMenu != null) victoryMenu.SetActive(false);
        ResetDoorandbutton();
    }

    public void BackToStartMenu()
    {
        if (player != null)
        {
            Destroy(player);
            player = null;
        }

        // Clean up old health bar if it exists
        HealthBarUI oldHealthBar = FindObjectOfType<HealthBarUI>();
        if (oldHealthBar != null)
        {
            Destroy(oldHealthBar.gameObject);
        }
        Debug.Log("BackToStartMenu called");
        Time.timeScale = 1f;
        isRestarting = false;
        GameFlags.hasPlayedCutscene = false;
        SceneManager.LoadScene(0);
    }

    public void restartGame()
    {
        if (player != null)
        {
            Destroy(player);
            player = null;
        }

        // Clean up old health bar if it exists
        HealthBarUI oldHealthBar = FindObjectOfType<HealthBarUI>();
        if (oldHealthBar != null)
        {
            Destroy(oldHealthBar.gameObject);
        }
        Debug.Log("[GameManager] restartGame() called");
        isRestarting = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetDoorandbutton()
    {
        if (doors != null)
        {
            foreach (Door door in doors)
            {
                if (door != null)
                {
                    //door.ResetDoor(); //door fix
                    door.playerInRange = false;
                }
            }
        }

        if (doorsButton != null)
        {
            foreach (DoorButton button in doorsButton)
            {
                if (button != null) button.ResetButton();
            }
        }
    }

    public void resetGameState()
    {
        gamePaused = false;
        gameOver = false;
        if (playerStats != null) playerStats.InitializePlayer();
        spawnHelperWelper();
    }
    #endregion

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
    }

    #region Settings Menu
    public void OpenSettings(GameObject fromMenu)
    {
        previousMenu = fromMenu;
        if (previousMenu != null) previousMenu.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(true);
       

        // Rebind sliders when opening settings
        if (AudioManager.instance != null)
        {
            if (musicSlider == null)
            {
                GameObject obj = GameObject.FindWithTag("MusicSlider");
                if (obj != null) musicSlider = obj.GetComponent<Slider>();

            }

            if (sfxSlider == null)
            {
                GameObject obj = GameObject.FindWithTag("SFXSlider");
                if (obj != null) sfxSlider = obj.GetComponent<Slider>();
            }

            AudioManager.instance.BindSliders(musicSlider, sfxSlider);
        }

        gamePaused = true;
        currentState = GameState.Paused;
    }

    public void CloseSettings()
    {
        if (SettingsPanel != null) SettingsPanel.SetActive(false);

        if (previousMenu != null)
        {
            previousMenu.SetActive(true);

            if (previousMenu == playMenu)
            {
                gamePaused = false;
                currentState = GameState.Play;
            }
            else if (previousMenu == pauseMenu)
            {
                gamePaused = true;
                currentState = GameState.Paused;
            }
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

    #region CHECKPOINTS/Scene
    public void LoadNextScene()
    {
        Time.timeScale = 1f;
        gamePaused = false;
        gameOver = false;
        isRestarting = true;
        GameFlags.hasPlayedCutscene = false;
        if (player != null)
        { 
            Destroy(player);
            player = null;
        }

        //HealthBarUI oldHealthBar = FindObjectOfType<HealthBarUI>();
        //if (oldHealthBar != null)
        //{
        //    Destroy(oldHealthBar.gameObject);
        //}
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }


public void RegisterPlayer(GameManager playerObj)
    {
    }

    public void SetCheckpoint(CheckPoints newCheckpoint, GameObject playerObj)
    {
        if (activecheckPoints != null && activecheckPoints != newCheckpoint)
        {
            activecheckPoints.Deactivate();
        }

        activecheckPoints = newCheckpoint;
        activecheckPoints.Activatethischeckpoint();
        checkpointPosition = activecheckPoints.transform.position;

        PlayerStats stats = playerObj.GetComponent<PlayerStats>();
        if (stats != null)
        {
            //  checkpointHealth = stats.health;
        }
    }

    public void RespawnPlayer()
    {
        if (player == null || activecheckPoints == null) return;

        player.transform.position = checkpointPosition;

        PlayerStats stats = player.GetComponentInParent<PlayerStats>();
        if (stats != null)
        {
            //  stats.health = checkpointHealth;
        }

        //SoftBodyPhyiscs softbody = player.GetComponent<SoftBodyPhyiscs>();
        //if (softbody != null)
        //{
        //    //  softbody.ResetSegments(checkpointSegmentcount);
        //}
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
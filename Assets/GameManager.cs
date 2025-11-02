using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
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
    public static bool hasPlayedCutscene = false;
}

public class GameManager : MonoBehaviour
{
    // SIMPLE FUCKING SINGLETON
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
        // Singleton setup
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

        // UI setup
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
        // Find and bind audio sliders to AudioManager
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
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Play || currentState == GameState.Paused)
            {
                TogglePaused();
            }
        }

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
        SpawnPlayer();
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
            if (cam != null) cam.SetPlayer(player.transform);


        }
        else
        {
            player.transform.position = playerspawnPos.position;
            if (playerStats != null) playerStats.InitializePlayer();
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
            if (victoryMenu != null) victoryMenu.SetActive(true);
            if (gameOverMenu != null) gameOverMenu.SetActive(false);
        }
        else
        {
            Debug.Log("[GameManager] gameOverMenu active: " + (gameOverMenu != null ? gameOverMenu.activeSelf.ToString() : "NULL"));
            if (victoryMenu != null) victoryMenu.SetActive(false);
            if (gameOverMenu != null) gameOverMenu.SetActive(true);
        }
    }

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
        GameFlags.hasPlayedCutscene = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
                    door.ResetDoor();
                    door.Playerinreange = false;
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
        Application.Quit();
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

    #region CHECKPOINTS
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
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] AudioSource BGM;
    [SerializeField] AudioSource SFX;

    [Header("Mixer")]
    [SerializeField] private AudioMixer myMixer;

    [Header("UI Sliders (optional, auto-bind by tag)")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Clips")]
    public AudioClip BGMmusic;
    public AudioClip uiclick;
    public AudioClip Victory;
    public AudioClip Endgame;
    public AudioClip pickup;
    public AudioClip shootOut;
    public AudioClip jump;
    public AudioClip grab;
    public static AudioClip movement;
    public AudioClip restorePuddle;
    public AudioClip applyPuddle;

    private bool sfxSliderInteracting = false;
    private bool sfxSliderFirstLoaded = false;

    //Singleton.
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    //To load scene.
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Autobind.
        if (musicSlider == null) musicSlider = GameObject.FindWithTag("MusicSlider")?.GetComponent<Slider>();
        if (sfxSlider == null) sfxSlider = GameObject.FindWithTag("SFXSlider")?.GetComponent<Slider>();

     
        ApplySavedVolume();
    }

    //
    private void Start()
    {
        if (BGM != null && BGMmusic != null)
        {
            BGM.clip = BGMmusic;
            BGM.Play();
        }

        // Apply saved volume at start if sliders exist
        ApplySavedVolume();
    }

    //To bind the sliders.
    public void BindSliders(Slider music, Slider sfx)
    {
        musicSlider = music;
        sfxSlider = sfx;


        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 1f); // set before binding
            musicSlider.onValueChanged.AddListener(value => SetMusicVolume());
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f); // set before binding
            sfxSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
            sfxSliderFirstLoaded = false;
        }
    }

    //Update.
    private void Update()
    {
        if (sfxSlider != null && !Input.GetMouseButton(0) && sfxSliderInteracting)
            sfxSliderInteracting = false;
    }

    //SFX Only.
    public void OnSFXSliderValueChanged(float value)
    {
        if (!sfxSliderFirstLoaded)
        {
            sfxSliderFirstLoaded = true;
            return; // ignore first trigger on load
        }

        if (!sfxSliderInteracting)
        {
            PlayUI();
            sfxSliderInteracting = true;
        }

        SetSFXVolume();
    }

    //Play music and sfx here.
    public void PlaySFX(AudioClip clip)
    {
        if (SFX != null && clip != null)
            SFX.PlayOneShot(clip);
    }

    public void PlayUI()
    {
        if (SFX != null && uiclick != null)
            SFX.PlayOneShot(uiclick);
    }

    //Set music and sfx here.
    public void SetMusicVolume()
    {
        if (myMixer == null || musicSlider == null) return;

        float volume = musicSlider.value;
        myMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        if (myMixer == null || sfxSlider == null) return;

        float volume = sfxSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    //Apply Saved Volume (Music Volume doenst reflect the value when the game loaded - Gameplay.scene).
    private void ApplySavedVolume()
    {
        float musicVol = PlayerPrefs.GetFloat("musicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (musicSlider != null)
        {
            musicSlider.value = musicVol;
            SetMusicVolume();
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVol;
            SetSFXVolume();
            sfxSliderFirstLoaded = false;
        }
    }
}
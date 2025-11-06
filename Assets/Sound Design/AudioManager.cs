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

    [Header("UI Sliders")]
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

    //Audio State Flags
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

    //To subscribe/unscribe to scene load events (Experimental).
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

   
    //
    private void Start()
    {
        ApplyMixerVolume();

        if (BGM != null && BGMmusic != null)
        {
            BGM.clip = BGMmusic;
            BGM.Play();
        }

        //Apply saved volume at start if sliders exist.
        ApplySavedVolume();
    }

    //To bind the sliders.
    public void BindSliders(Slider music, Slider sfx)
    {
        musicSlider = music;
        sfxSlider = sfx;

        if (musicSlider != music && music != null)
        {
            musicSlider = music;
        }

        if (sfxSlider != sfx && sfx != null)
        {
            sfxSlider = sfx;
        }


        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 1f); // set before binding
            musicSlider.onValueChanged.AddListener(value => SetMusicVolume());
        }

        if (sfxSlider != null)
        {
            sfxSliderFirstLoaded = false;

            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f); // set before binding
            sfxSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
            sfxSliderFirstLoaded = false;

        }
        SetMusicVolume();
        SetSFXVolume();
    }

    //Update (This is for SFX because when drag the UI it might have tons of sounds spamming).
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


    //Force applied mixervolume incase setting does not work instantly.
    private void ApplyMixerVolume()
    {
        if (myMixer == null) return;

        float musicVol = PlayerPrefs.GetFloat("musicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        myMixer.SetFloat("Music", Mathf.Log10(musicVol) * 20);
        myMixer.SetFloat("SFX", Mathf.Log10(sfxVol) * 20);

        Debug.Log($"[AudioManager] Mixer volumes applied - Music: {musicVol}, SFX: {sfxVol}");
    }


    //Apply Saved Volume.
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
            sfxSliderFirstLoaded = false;
            sfxSlider.value = sfxVol;
            SetSFXVolume();
            sfxSliderFirstLoaded = false;
        }
    }

    //Scene Load Handler. - to bind the buttons and the sliders.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    { 
         Slider newMusicSlider = GameObject.FindWithTag("MusicSlider")?.GetComponent<Slider>();
        Slider newSFXslider = GameObject.FindWithTag("SFXSlider")?.GetComponent<Slider>();

        if (newMusicSlider != null || newSFXslider != null)
        { 
            BindSliders(newMusicSlider, newSFXslider);
           
        }
        BindButtons();
        ApplySavedVolume();  
    
    }

    //Find all the buttons in the scene including the inactive ones.
    private void BindButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button btn in buttons)
        {
            //Remove previous listener just in case.
            btn.onClick.RemoveListener(PlayUI);

            //Add your PlayUI method to this button.
            btn.onClick.AddListener(PlayUI);
        }
    }
}

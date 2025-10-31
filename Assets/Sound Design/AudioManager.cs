using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource BGM;
   // [SerializeField] AudioSource BossBGM;

    [SerializeField] AudioSource SFX;

    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    //public static AudioManager instance;

    //References

    public AudioClip BGMmusic;

    #region GameState
    //public AudioClip death;
    //public AudioClip checkpoint;
    public AudioClip Victory;
    public AudioClip Endgame;

    #endregion

    #region Obstacles
    //public AudioClip walltouch;
    //public AudioClip doorIn;
    //public AudioClip doorOut;
    //public AudioClip leverTriggered;
    //public AudioClip levelUntriggered;
    //public AudioClip spikesTriggered;
    //public AudioClip spikesUnTriggered;

    #endregion

    #region Player 
    public AudioClip pickup;
    public  AudioClip shootOut;

    public  AudioClip jump;
    public AudioClip grab;
    public static AudioClip movement;

    #endregion

    //#region Drone SFX
    //public AudioClip Dmovemement;
    //public AudioClip Dattack;

    //#endregion

    //#region HelperWelper SFX
    //public AudioClip Hmovemement;
    //public AudioClip Hattack;

    //#endregion

    //#region FailedSubject SFX
    //public AudioClip Fmovemement;
    //public AudioClip Fattack;

    //#endregion

    public void Awake()
    {
        //audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        //if (instance == null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);

        //}
        //else
        //{ 
        //    Destroy(gameObject);
        //}
    }

    public void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        { 
            SetMusicVolume();
            SetSFXVolume();
        }

            BGM.clip = BGMmusic;
        BGM.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFX.PlayOneShot(clip);
    }

    public void SetMusicVolume()
    {
        if (myMixer == null || musicSlider == null)
            return;

        float volume = musicSlider.value;
        myMixer.SetFloat("Music", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("musicVolume", volume);
        PlayerPrefs.Save();

    }

    public void SetSFXVolume()
    {
        if (myMixer == null || sfxSlider == null)
            return;
        float volume = sfxSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();

    }


    //Store Music Setting.
    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");

        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 1f);
            SetMusicVolume();
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            SetSFXVolume();
        }
    }
}


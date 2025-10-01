using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource BGM;
    [SerializeField] AudioSource SFX;

    //References

    public AudioClip background;
    //public AudioClip death;
    //public AudioClip checkpoint;
    //public AudioClip walltouch;
    //public AudioClip doorIn;
    //public AudioClip doorOut;
    public  AudioClip pickup;
    public  AudioClip shootOut;
    public  AudioClip jump;
    public static AudioClip movement;
    //public AudioClip enemy;

    public void Awake()
    {
        // audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void Start()
    {
        BGM.clip = background;
        BGM.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFX.PlayOneShot(clip);
    }
}


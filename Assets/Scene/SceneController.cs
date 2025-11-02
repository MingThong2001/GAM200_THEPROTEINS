using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{

    //Singleton
    public static SceneController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        { 
            Destroy(gameObject);
        }
    }

 
    public static void LoadnextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            SceneManager.LoadScene("Victory");
        }
    }

    public  void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
   
}


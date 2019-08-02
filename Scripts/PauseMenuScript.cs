using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuScript : MonoBehaviour
{

    public static bool GameIsPaused = false;
    public static PauseMenuScript instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Resume();
    }

    
    public void TogglePauseMenu()
    {
        if (GameIsPaused)
            Resume();
        else
            Pause();

    }

   public void Resume()
    {

        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        //TODO why doesn't the game pause?
        Time.timeScale = 0f;
        GameIsPaused = true;
    }


    public void LoadMenu()
    {
        Debug.Log("Loading main menu...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }


    public void QuitGame()
    {
        Debug.Log("Quiting game...");
        Application.Quit();
    }

}

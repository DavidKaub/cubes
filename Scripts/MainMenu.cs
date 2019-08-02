using UnityEngine;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject inputIpAdress;

    [SerializeField]
    private GameObject inputUsername;

    [SerializeField]
    private GameObject loadMenu;

    [SerializeField]
    private GameObject settingsMenu;

    [SerializeField]
    private GameObject mainMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenMainMenu();
        }
    }

    public void StartMultiPlayer()
    {
        Scenes.Drop();
        //SSTools.ShowMessage("Start multiPlayer Clicked", SSTools.Position.bottom, SSTools.Time.oneSecond);
        string ip = PlayerPrefs.GetString("ip");
        if (ip.Equals(""))
        {
            SSTools.ShowMessage("Please insert valid ip in settings menu before launching multiplayer", SSTools.Position.bottom, SSTools.Time.threeSecond);
            return;
        }
        string localPort = PlayerPrefs.GetString("localPort");
        if (localPort.Equals(""))
        {
            SSTools.ShowMessage("Please insert valid local port in settings menu before launching multiplayer", SSTools.Position.bottom, SSTools.Time.threeSecond);
            return;
        }

        string remotePort = PlayerPrefs.GetString("remotePort");
        if (remotePort.Equals(""))
        {
            SSTools.ShowMessage("Please insert valid remote Port in settings menu before launching multiplayer", SSTools.Position.bottom, SSTools.Time.threeSecond);
            return;
        }

        string userName = PlayerPrefs.GetString("userName");
        if (userName.Equals(""))
        {
            SSTools.ShowMessage("Please insert valid username in settings menu before launching multiplayer", SSTools.Position.bottom, SSTools.Time.threeSecond);
            return;
        }
        Scenes.setParam("multiplayer", "true");
        Scenes.setParam("ip", ip);
        Scenes.setParam("remotePort", remotePort);
        Scenes.setParam("localPort", localPort);
        Scenes.setParam("userName", userName);
        Scenes.MyLoad("Cubes");
        //StartSinglePlayer();
    }

    public void OpenLoadMenu()
    {
        Debug.Log("open load menu!");
        mainMenu.SetActive(false);
        loadMenu.SetActive(true);
        Debug.Log("opened load menu!");
    }

    public void StartSinglePlayer()
    {
        Scenes.Drop();
        SSTools.ShowMessage("Start singlePalyer Clicked", SSTools.Position.bottom, SSTools.Time.oneSecond);
        Scenes.setParam("multiplayer", "false");
        Scenes.MyLoad("Cubes");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettingsMenu()
    {
        if (PlayerPrefs.HasKey("ip"))
        {
            Debug.Log("ip found = " + PlayerPrefs.GetString("ip"));
            inputIpAdress.GetComponent<InputField>().text = PlayerPrefs.GetString("ip");
        }
        else
            Debug.Log("ip not found");


        if (PlayerPrefs.HasKey("userName"))
        {
            Debug.Log("playerName found = " + PlayerPrefs.GetString("userName"));
            inputUsername.GetComponent<InputField>().text = PlayerPrefs.GetString("userName");
        }
        else
            Debug.Log("userName not found");

    
        Debug.Log("open settings menu!");
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        Debug.Log("opened settings menu!");
    }

    public void OpenMainMenu()
    {
        settingsMenu.SetActive(false);
        loadMenu.SetActive(false);
        mainMenu.SetActive(true);
    }



    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }


    public void ApplySettings()
    {
        if (inputUsername.GetComponent<InputField>().text.Equals(""))
        {
            SSTools.ShowMessage("Please insert a valid Username", SSTools.Position.bottom, SSTools.Time.threeSecond);
            Debug.Log("Please insert valid username");
            return;
        }else
            PlayerPrefs.SetString("userName", inputUsername.GetComponent<InputField>().text);
        if (inputIpAdress.GetComponent<InputField>().text.Equals(""))
        {
            SSTools.ShowMessage("Please insert a valid Ip-Adress", SSTools.Position.bottom, SSTools.Time.threeSecond);
            Debug.Log("Please insert valid ip");
            return;
        }else
            PlayerPrefs.SetString("ip", inputIpAdress.GetComponent<InputField>().text);

      
            PlayerPrefs.SetString("localPort", "4321");


       
            PlayerPrefs.SetString("remotePort", "4321");



        PlayerPrefs.Save();
        SSTools.ShowMessage("Settings saved to disk", SSTools.Position.bottom, SSTools.Time.threeSecond);
    }

}

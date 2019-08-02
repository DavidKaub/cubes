using UnityEngine;

public class UiManagementScript : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private GameObject chatMenu;

    [SerializeField]
    private GameObject oSD;

    [SerializeField]
    private GameObject inventoryMenu;

    private BuildingSystem buildingSystemScript;
    private Inventory inventoryScript;
    private PauseMenuScript pauseMenuScript;
    private ChatClient chatClientScript;

    private bool pauseMenuActive = false;
    private bool chatMenuActive = false;

    private bool isMultiplayer;

    void Awake()
    {
        isMultiplayer = Scenes.getParam("multiplayer") == "true";
        buildingSystemScript = BuildingSystem.instance;
        inventoryScript = Inventory.instance;
        pauseMenuScript = PauseMenuScript.instance;
        chatClientScript = ChatClient.instance;
    }

       
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuActive)
                ClosePauseMenu();
            else
                OpenPauseMenu();

            if (pauseMenuScript == null)
            {
                pauseMenuScript = PauseMenuScript.instance;
                if (pauseMenuScript == null)
                    Debug.LogWarning("couldn't get pauseMenuScript instance!");
            }
            pauseMenuScript.TogglePauseMenu();
        }
        if (pauseMenuActive)
            return;
     
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (chatMenuActive)
                {
                    CloseChatMenu();
                }
                else
                {
                    OpenChatMenu();
                }
                if (chatClientScript == null)
                {
                    chatClientScript = ChatClient.instance;
                    if (chatClientScript == null)
                        Debug.LogWarning("couldn't get chatClientScript instance!");
                }
                chatClientScript.ToggleChatMenu();
                return;
            }
        
      

        if (oSD.activeSelf)
        {
            if (Input.GetKeyDown("t"))
            {
                if (buildingSystemScript == null)
                {
                    buildingSystemScript = BuildingSystem.instance;
                    if (buildingSystemScript == null)
                        Debug.LogWarning("couldn't get buildingSystemScript instance!");
                }                    
                buildingSystemScript.ToggleEditMode();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (inventoryScript == null)
                {
                    inventoryScript = Inventory.instance;
                    if (inventoryScript == null)
                        Debug.LogWarning("couldn't get inventory instance!");
                }
                inventoryScript.ToggleInventory();
                return;
            }
        }
    }

    private void DissableAllMenus()
    {
        inventoryMenu.SetActive(false);
        chatMenu.SetActive(false);
        pauseMenu.SetActive(false);
        oSD.SetActive(false);
    }


    public void OpenPauseMenu()
    {
        DissableAllMenus();
        pauseMenu.SetActive(true);
        oSD.SetActive(false);
    }

    public void OpenChatMenu()
    {
        DissableAllMenus();
        chatMenu.SetActive(true);
        chatMenuActive = true;
    }

    public void OpenInventoryMenu()
    {
        DissableAllMenus();
        inventoryMenu.SetActive(true);
    }

       
    public void ClosePauseMenu()
    {
        DissableAllMenus();
        pauseMenuActive = false;
        oSD.SetActive(true);
    }

    public void CloseChatMenu()
    {
        DissableAllMenus();
        pauseMenu.SetActive(false);
        oSD.SetActive(true);
    }

    public void CloseInventoryMenu()
    {
        DissableAllMenus();
        oSD.SetActive(true);
    }
}

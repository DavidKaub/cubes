using UnityEngine;
using UnityEngine.UI;

public class BuildingMode : MonoBehaviour
{

    public Image icon;
    private Button button;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
    }



    void enableBuildingMode()
    {
        icon.color = new Color(255, 255, 255, 255);
    } 

    void disableBuildingMode()
    {
        icon.color = new Color(80, 80, 80, 120);
    }
}


using UnityEngine;
using UnityEngine.UI;

public class MutliplayerObject : ScriptableObject
{

    public GameObject goInstance;
    private string playersName;
    private Text goText;

    public void Init(GameObject playerPrefab, Vector3 position, GameObject parent, string playersName)
    {
        this.playersName = playersName;
        goInstance = Instantiate(playerPrefab, position, Quaternion.identity);
        goInstance.transform.parent = parent.transform;
        goText = goInstance.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        goText.text = playersName;

    }

    public void SetNewPosition(Vector3 newPosition)
    {
        goInstance.transform.position = newPosition;
    }
    public (Vector3, string) GetPlayer()
    {
        return (goInstance.transform.position, playersName);
    }
}

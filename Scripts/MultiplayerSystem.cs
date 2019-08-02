using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;



    [SerializeField]
    private GameObject player;


    public static MultiplayerSystem instance;
    public Dictionary<int, MutliplayerObject> players = new Dictionary<int, MutliplayerObject>();
    public List<int> playersWithNewPosition = new List<int>();
    private int idCounter = 1;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("MultiplayerSystem: starting mp sys!");
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            DummyUpdatePosition();
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            AddPlayer();
        }
        UpdatePlayerPositions();


    }

    void DummyUpdatePosition()
    {
        Debug.Log("Dummy update position");
        foreach (var pair in players)
        {
            int key = pair.Key;
            //string value = pair.Value;
            playersWithNewPosition.Add(key);

        }
    }



        void UpdatePlayerPositions()
    {

        foreach (int playerId in playersWithNewPosition)
        {
            //TODO change postion to actual new postion
            players[playerId].SetNewPosition(player.transform.position);
        }
        playersWithNewPosition.Clear();

    }


    public void AddPlayer()
    {

        MutliplayerObject newObject = ScriptableObject.CreateInstance<MutliplayerObject>();
        newObject.Init(playerPrefab, player.transform.position, this.gameObject, "PlayerName");
        players.Add(idCounter, newObject);
        idCounter++;
        //newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
        Debug.Log("MultiplayerSystem: Adding my own Player");
        //take some variables like the position and instaciate a playerPrefab into the multiplayerWOrlds object
    }

    public MutliplayerObject AddPlayer(string playername, Vector3 playerPosition)
    {
        // Funktion wird genutzt um neue Spieler hinzuzufügen
        MutliplayerObject newObject = ScriptableObject.CreateInstance<MutliplayerObject>();
        newObject.Init(playerPrefab, playerPosition, this.gameObject, playername);
        players.Add(idCounter, newObject);
        idCounter++;
        Debug.Log("MultiplayerSystem: Added new Player: " + playername +" Position: " + playerPosition);
        return newObject;
    }
}

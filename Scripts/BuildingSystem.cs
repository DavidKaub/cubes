using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BuildingModeName { Observation, Construction, Demolition, Teleport};

public class BuildingSystem : MonoBehaviour
{

    [SerializeField]
    private Camera playerCamera;

    private bool editModeOn = false;
    private bool canExecute = false;

    public BuildingModeName buildingMode = BuildingModeName.Observation;

    private BlockSystem blockSystem;

    [SerializeField]
    private LayerMask buildableSurfacesLayer;

    [SerializeField]
    private LayerMask placedBlockLayer;

    private Vector3 buildPos;

    private GameObject currentTemplateBlock;


    private GameObject currentDemolitionObject;
    private Material currentDemolitionObjectMaterial;

    [SerializeField]
    private GameObject blockTemplatePrefab;
    [SerializeField]
    private GameObject blockPrefab;

    [SerializeField]
    private GameObject multiplayerWorlds;

    private PeerToPeerManager peerToPeerManager;

    [SerializeField]
    private Material templateMaterial;


    private Dictionary<Vector3, GameObject> blockGameObjects = new Dictionary<Vector3, GameObject>(); // <position, Gameobject>

    [SerializeField]
    private Material demolitionMaterial;

    public static int blockSelectCounter = 0;

    public static BuildingSystem instance;

    private bool validBlockSelection = true;

    private bool teleportAvailable = false;

    private Vector3 teleportPosition;

    [SerializeField]
    private GameObject player;
    PlayerMove playerMove;


  
    private void Start()
    {
        instance = this;
        peerToPeerManager =  multiplayerWorlds.GetComponent<PeerToPeerManager>();
        blockSystem = GetComponent<BlockSystem>();
        playerMove = player.GetComponent<PlayerMove>();
        Debug.Log("Building System Initialized!");
    }


    public void SetActiveBlock(int blockNumber)
    {
        blockSelectCounter = blockNumber;
        if (blockSelectCounter >= blockSystem.allBlocks.Count || blockSelectCounter < 0)
        {
            //Debug.Log("Block Selection "+blockNumber+" unvalid");
            validBlockSelection = false;
        }
        else
        {
            validBlockSelection = true;
        }
    }
    
    private void Teleport()
    {
        if (teleportAvailable)
        {
            Debug.Log("teleporting!");
            playerMove.moveTo(teleportPosition);
        }
    }

    private void UpdateTeleportPosition()
    {

        RaycastHit raycastPosHit;
        if (Physics.Raycast(playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out raycastPosHit, 1000, buildableSurfacesLayer))
        {
            Vector3 point = raycastPosHit.point;
            teleportPosition = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z));
            teleportAvailable = true;
            //                Debug.Log("distance to template = " + Vector3.Distance(buildPos, playerCamera.transform.position));
        }
        else
        {
            teleportAvailable = false;
        }    
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            Debug.Log("Placing random blocks");
            PlaceRandomBlocks(playerCamera.transform.position, 10, 10);
           
        }
    }

    void LateUpdate()
    {     
        if (!editModeOn && currentTemplateBlock != null)
        {
            ResetBuildingTemplate();
            ResetDemolitionObject();
            canExecute = false;
            return;
        }

        if (editModeOn)
        {
            if (buildingMode.Equals(BuildingModeName.Teleport))
            {
                UpdateTeleportPosition();
                if (Input.GetMouseButtonDown(0))
                {
                    Teleport();
                }

            }
            else if (buildingMode.Equals(BuildingModeName.Demolition))
            {
                UpdateDemolitionPosition();
                if (Input.GetMouseButtonDown(0))
                {
                    RemoveBlock();
                }

            }
            else if (buildingMode.Equals(BuildingModeName.Construction))
            {
                UpdateConstructionPosition();
                if (canExecute && currentTemplateBlock == null)
                {
                    currentTemplateBlock = Instantiate(blockTemplatePrefab, buildPos, Quaternion.identity);
                    currentTemplateBlock.GetComponent<MeshRenderer>().material = templateMaterial;
                }
                if (canExecute && currentTemplateBlock != null)
                {
                    currentTemplateBlock.transform.position = buildPos;                   
                }
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceBlock();
                }
                return;
            }
        }  
    }


    private void UpdateDemolitionPosition()
    {
        RaycastHit buildPosHit;

        if (Physics.Raycast(playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out buildPosHit, 10, placedBlockLayer))
        {
            if (currentDemolitionObject != buildPosHit.collider.gameObject)
            {
                ResetDemolitionObject();
                currentDemolitionObject = buildPosHit.collider.gameObject;
                currentDemolitionObjectMaterial = currentDemolitionObject.GetComponent<MeshRenderer>().material;
                currentDemolitionObject.GetComponent<MeshRenderer>().material = demolitionMaterial;
            }
            canExecute = true;
        }
        else
        {
            ResetDemolitionObject();
            canExecute = false;
        }
    }


    private void UpdateDemolitionPositionOld()
    {
        ResetBuildingTemplate();
        RaycastHit buildPosHit;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out buildPosHit, 10, placedBlockLayer))
        {
            if (currentDemolitionObject != buildPosHit.collider.gameObject)
            {
                ResetDemolitionObject();
                currentDemolitionObject = buildPosHit.collider.gameObject;
                currentDemolitionObjectMaterial = currentDemolitionObject.GetComponent<MeshRenderer>().material;
                currentDemolitionObject.GetComponent<MeshRenderer>().material = demolitionMaterial;
            }
            canExecute = true;
        }
        else
        {
            ResetDemolitionObject();
            canExecute = false;
        }
    }




    private void ResetBuildingTemplate()
    {
        if (currentTemplateBlock == null)
            return;

        Destroy(currentTemplateBlock.gameObject);
        currentTemplateBlock = null;
    }

    private void ResetDemolitionObject()
    {
        if (currentDemolitionObject != null)
        {
            currentDemolitionObject.GetComponent<MeshRenderer>().material = currentDemolitionObjectMaterial;
            currentDemolitionObject = null;
            currentDemolitionObjectMaterial = null; ;
        }
    }


    private void UpdateConstructionPosition()
    {
        RaycastHit buildPosHit;

        if (Physics.Raycast(playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out buildPosHit, 10, buildableSurfacesLayer))
        {
            Vector3 point = buildPosHit.point;
            buildPos = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z));
            canExecute = true;
            //                Debug.Log("distance to template = " + Vector3.Distance(buildPos, playerCamera.transform.position));
        }
        else
        {
            Vector3 point = playerCamera.transform.position + playerCamera.transform.forward * 10; //weg
            buildPos = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z)); //weg
            canExecute = true;

            //Destroy(currentTemplateBlock.gameObject);
            //canExecute = false;
        }
    }

    public void ToggleEditMode()
    {
        editModeOn = !editModeOn;

        if (editModeOn)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            ResetBuildingTemplate();
            ResetDemolitionObject();
        }
    }


    public void PlaceRandomBlocks(Vector3 basePosition, int amount, int distance)
    {
        int index = 0;
        while (index < amount)
        {
            index++;
            int textureId = Random.Range(0, blockSystem.allBlocks.Count);
            int x = Random.Range(-distance, distance);
            int y = Random.Range(-distance, distance);
            int z = Random.Range(-distance, distance);
            Vector3 position = new Vector3(basePosition.x + x, basePosition.y + y, basePosition.z + z);
            Debug.Log("Placing random block " + textureId + " at " + Message.GetStringFromVector3(position));
            PlaceBlockByPos(position, textureId);

            peerToPeerManager.AddBlockFromBuildingSystem(position, textureId);
        }
    }

    public void PlaceBlockByPos(Vector3 position, int texture)
    {
        GameObject newBlock = Instantiate(blockPrefab, position, Quaternion.identity);
        Block tempBlock = blockSystem.allBlocks[texture];
        newBlock.name = tempBlock.name + "-Block";
        newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
        AddBlockToDictionary(position, newBlock);
    }

    private void PlaceBlock()
    {
        if (!validBlockSelection)
            return;               
        GameObject newBlock = Instantiate(blockPrefab, buildPos, Quaternion.identity);
        Block tempBlock = blockSystem.allBlocks[blockSelectCounter];
        newBlock.name = tempBlock.name + "-Block";
        newBlock.GetComponent<MeshRenderer>().material = tempBlock.blockMaterial;
        peerToPeerManager.AddBlockFromBuildingSystem(buildPos,blockSelectCounter);
        AddBlockToDictionary(buildPos, newBlock);
    }

    private void AddBlockToDictionary(Vector3 pos, GameObject block)
    {
        if (blockGameObjects.ContainsKey(pos))
        {
            GameObject oldBlock = blockGameObjects[pos];
            blockGameObjects.Remove(pos);
            Destroy(oldBlock);
        }
        blockGameObjects.Add(pos, block);
    }

    public void RemoveBlockByPosition(Vector3 vector)
    {
        if (blockGameObjects.ContainsKey(vector))
        {
            Destroy(blockGameObjects[vector]);
        }
    }

    private void RemoveBlock()
    {
        if(currentDemolitionObject == null)
        {
            return;
        }
        peerToPeerManager.DeleteBlockFromBuildingSystem(currentDemolitionObject.transform.position);
        Destroy(currentDemolitionObject);
    }

    public void ActivateTeleport()
    {
        //Debug.Log("Selected Teleport");
        ResetBuildingTemplate();
        ResetDemolitionObject();
        buildingMode = BuildingModeName.Teleport;
    }

    
    public void ActivateViewer()
    {        
        //Debug.Log("Selected Viewer");
        ResetBuildingTemplate();
        ResetDemolitionObject();
        buildingMode = BuildingModeName.Observation;
    }


    public void ActivateDemolition()
    {
        //Debug.Log("Selected Demolition");
        ResetBuildingTemplate();
        ResetDemolitionObject();
        buildingMode = BuildingModeName.Demolition;
    }

    public void ActivateConstruction()
    {
        //Debug.Log("Selected Construction");
        ResetBuildingTemplate();
        ResetDemolitionObject();
        buildingMode = BuildingModeName.Construction;
    }





   
}
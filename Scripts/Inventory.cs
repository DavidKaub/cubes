using System;
using UnityEngine;
using UnityEngine.UI;


public class Inventory : MonoBehaviour
{
    public bool inventoryEnabled = false;
    private int totalQuickSlots = 9;
    private int totalSlots = 42;

    private int numAvailableSlots;
    private int numAvailableQuickSlots;

    private Slot[,] slots;
    private Slot[] quickSlots;

    private int numOfCategories;
    private int activeCategory = 0;

    private BuildingSystem buildingSystem;
    private BlockSystem blockSystem;


    public GameObject inventory;
    public GameObject slotHolder;
    public GameObject quickInventory;
    public GameObject categories;

    private Slot activeQuickSlot;
    private int activeQuickSlotIndex = 0;

    private Color unselectedColor;
    private Color selectedColor = new Color(1, 0, 0, 0.5f);

    public static Inventory instance;

    void Start()
    {
        instance = this;
        Debug.Log("Initializing inventory");           
        buildingSystem = GameObject.FindGameObjectWithTag("BlockManager").GetComponent<BuildingSystem>();
        //Debug.Log("got BuildingSystem");
        blockSystem = GameObject.FindGameObjectWithTag("BlockManager").GetComponent<BlockSystem>();
        //Debug.Log("got BuildingSystem");
        numAvailableSlots = Math.Min(totalSlots, blockSystem.allBlocks.Count);

        //numAvailableQuickSlots = Math.Min(totalQuickSlots, numAvailableSlots);
        numAvailableQuickSlots = totalQuickSlots;

        numOfCategories = categories.transform.childCount;

        slots = new Slot[numOfCategories, numAvailableSlots];
        quickSlots = new Slot[numAvailableQuickSlots];
        Debug.Log("All slots =  "+ numAvailableSlots + " quick slots = "+ numAvailableQuickSlots);


        for (int i = 0; i < numAvailableSlots; i++)
        {
            slots[activeCategory,i] = slotHolder.transform.GetChild(i).GetComponent<Slot>();
            slots[activeCategory,i].isQuickSlot = false;
            if (i < blockSystem.allBlocks.Count)
                slots[activeCategory,i].SetItem(blockSystem.allBlocks[i]);
        }
        /*
        for (int i = numAvailableSlots; i < totalSlots; i++)
        {
            GameObject go = slotHolder.transform.GetChild(i).gameObject;
            go.SetActive(false);
        }
        */
        Debug.Log("post slotHolder");
        for (int i = 0; i < numAvailableQuickSlots; i++)
        {
            quickSlots[i] = quickInventory.transform.GetChild(i).GetComponent<Slot>();
            quickSlots[i].isQuickSlot = true;
            quickSlots[i].slotIndex = i;
            if (i < blockSystem.allBlocks.Count)
                quickSlots[i].SetItem(blockSystem.allBlocks[i]);
        }
        /*
        for (int i = numAvailableQuickSlots; i < totalQuickSlots; i++)
        {
            GameObject go = quickInventory.transform.GetChild(i).gameObject;
            go.SetActive(false);
        }
        */
        Debug.Log("post quickSlots");
        activeQuickSlot = quickSlots[0];
        Image anImage = activeQuickSlot.GetComponent<Image>();
        unselectedColor = anImage.color;
        anImage.color = selectedColor;
        Debug.Log("Initialize Done");
    }

    public void ToggleInventory()
    {
      
        Debug.Log("toggled inventory");
            inventoryEnabled = !inventoryEnabled;
        


        if (inventoryEnabled == true)
        {
            inventory.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            inventory.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void Update()
    {     
        if (UpdateQuickSlotIndex())
        {
            UpdateActiveQuickSlot();
        }


        //update the game accordingly // combine the ui and the actual building system? -> not the best idea?
    }
    private void UpdateActiveQuickSlot()
    {
        Image img = activeQuickSlot.GetComponent<Image>();
        img.color = unselectedColor;
        activeQuickSlot = quickSlots[activeQuickSlotIndex];
        img = activeQuickSlot.GetComponent<Image>();
        img.color = selectedColor;
        //Debug.Log("new Quick slot index = " + activeQuickSlotIndex);
        if (!quickSlots[activeQuickSlotIndex].isUsed)
        {
            //Debug.Log("cant apply selection -> no quick slot set!");
            buildingSystem.SetActiveBlock(-1);
        }
        else
            buildingSystem.SetActiveBlock(activeQuickSlot.item.itemId);
    }

    public void SetActiveQuickSlot(int slotIndex)
    {

        Debug.Log("quick slot set to "+slotIndex);
        activeQuickSlotIndex = slotIndex;
        UpdateActiveQuickSlot();
    }

    private bool UpdateQuickSlotIndex()
    {
        for (int i = 1; i <= numAvailableQuickSlots; ++i)
        {
            if (Input.GetKeyDown("" + i))
            {
                activeQuickSlotIndex = i-1;
                return true;
            }
        }
             
        // quick Inventory selection!
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            activeQuickSlotIndex++;
            if (activeQuickSlotIndex > numAvailableQuickSlots - 1)
                activeQuickSlotIndex = 0;
            return true;
        }


        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            activeQuickSlotIndex--;
            if (activeQuickSlotIndex < 0)
                activeQuickSlotIndex = numAvailableQuickSlots - 1;
            return true;
        }
        return false;
    }
}




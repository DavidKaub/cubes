using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Item item;
    public Image icon;
    public bool isUsed = false;
    public bool isQuickSlot;
    private Inventory inventoryScript;
    public int slotIndex;
        
    public void SetItem(Item newItem)
    {
        if (isUsed)
        {
            Debug.LogWarning("Illegal State! - item already set!");
            return;
        }
        item = newItem;
        icon.sprite = item.icon;
        // Debug.LogWarning("Illegal State! - item already set!");
        icon.enabled = true;
        isUsed = true;
    }
        
    
    private void ClearSlot()
    {
        item = null;
        icon.sprite = null;

        icon.enabled = false;
        isUsed = false;
    }
        
    public void ReplaceSlot(Item newItem)
    {
        ClearSlot();
        SetItem(newItem);

    }


    public void SelectQuickSlot()
    {
        Debug.Log("select quick slot clicked!");
        if (!isQuickSlot) { 
            Debug.Log("no quick slot!");
            return;
    }
        if(inventoryScript == null)
        {
            inventoryScript = Inventory.instance;
            if(inventoryScript == null){
                Debug.Log("Inventory script still Null");
                return;
            }
        }
        inventoryScript.SetActiveQuickSlot(slotIndex);

    }
}
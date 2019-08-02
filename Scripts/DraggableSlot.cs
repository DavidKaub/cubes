using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject quickInventory;

  
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("On begin drag!");
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("While draging!");
        this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Destroy()
        for(int i = 0; i < quickInventory.transform.childCount; i++)
        {
            //Loop through all quick invenotory slots and search for fitting rect. if set new value and break;
            GameObject slot = quickInventory.transform.GetChild(i).gameObject;
            RectTransform slotsAsRect = slot.transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(slotsAsRect, eventData.position))
            {
                DragOnSlot(slot);
            }
        }      
        this.transform.localPosition = Vector3.zero;
        Debug.Log("On end drag!");
    }

    private void DragOnSlot(GameObject slotToDropOn)
    {
        Debug.Log("draged on slot!");
        Slot dropSlotScript = slotToDropOn.GetComponent<Slot>();
        Slot dragSlotScript = this.transform.GetComponentInParent<Slot>();
        if (!dropSlotScript.isUsed)
        {
            dropSlotScript.SetItem(dragSlotScript.item);
            return;
        }
        //when the slot is used decide weather to replace or swap slots
        if (dragSlotScript.isQuickSlot)
        {
            Item oldItem = dropSlotScript.item;
            dropSlotScript.ReplaceSlot(dragSlotScript.item);
            dragSlotScript.ReplaceSlot(oldItem);
        }
        else
        {
            dropSlotScript.ReplaceSlot(dragSlotScript.item);
        }       
    }
}

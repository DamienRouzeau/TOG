using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    public SlotPosition slotPosition;
    public bool hasItemInSlot = false;
    public GameObject objectInSlot = null;

    public enum SlotPosition { Left, Right, Front, Back };

    void Start()
    {
        
    }

    void Update()
    {
        
    }

}

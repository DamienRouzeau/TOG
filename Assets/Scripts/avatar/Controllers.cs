using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if STEAM_PRESENT
using UnityEngine.Networking;
using Valve.VR;
#endif

public class Controllers : MonoBehaviour
{
#if STEAM_PRESENT
    [Header("SteamVR params")]
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean Trigger;
    public SteamVR_Action_Boolean Grip;
    public SteamVR_Behaviour_Pose controllerPose;
#else
    public OVRInput.Controller controllerInput;
#endif

    [Header("Shoot params")]
    public bool wantsToShot;
    public GameObject collidingObject = null;
    public GameObject objectInHand = null;
    public Weapon weapon;
    public GameObject gunPostion;
    public GameObject musketPosition;
    public GameObject torchPosition;

    public Player player = null; // Player Owner Script

    public bool wantsToTakeObject = false;
    public bool wantsToLeaveObject = false;
    public bool wantsToTakeObjectInSlot = false;
    public bool wantsToLeaveObjectInSlot = false;

    public GameObject inventorySlotDetected = null;

    public GameObject canonLevelerDetected = null;

    public bool isAWeapon = false;

    void Start()
    {
        wantsToShot = false;
    }

    void Update()
    {
        if (player)
        {
            InputsVr();
        }
    }

    private void InputsVr()
    {
        if (GetTrigger()) // Trigger
        {
            if (objectInHand != null && weapon != null && isAWeapon)
                wantsToShot = true;
        }
        if (GetGrip()) // Grip
        {
            if (canonLevelerDetected != null && objectInHand == null) // Veut bouger la cale du canon
            {
                CanonLeveler cL = canonLevelerDetected.GetComponent<CanonLeveler>();
                if (cL.isHoldByPlayer)
                    cL.LeaveLeveler();
                else
                    cL.TakeLeveler(this.gameObject);    
            }
            else if (inventorySlotDetected == null) // L'objet concerné n'est pas dans la zone d'inventaire
            {
                if (objectInHand == null && collidingObject != null) // Prendre objet en main
                {
                    if (collidingObject.gameObject.tag == "Canon_Bait")
                        wantsToTakeObject = true;
                    else if (collidingObject.gameObject.tag == "Weapon")
                    {
                        if (collidingObject.GetComponent<Weapon>().playerWeapon == false)
                            wantsToTakeObject = true;
                    }

                }
                else if (objectInHand != null) // Lacher objet
                    wantsToLeaveObject = true;
            }
            else // Cela concerne l'inventaire
            {
                if (objectInHand == null && inventorySlotDetected.GetComponent<InventorySlot>().hasItemInSlot) // Prendre l'objet dans l'inventaire
                    wantsToTakeObjectInSlot = true;
                else if (objectInHand != null && inventorySlotDetected.GetComponent<InventorySlot>().hasItemInSlot == false) // Mettre objet dans inventaire
                    wantsToLeaveObjectInSlot = true;
            }
        }
    }

    public bool GetTrigger()
    {
#if STEAM_PRESENT
        return Trigger.GetState(handType);
#else
        return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controllerInput);
#endif
    }

    public bool GetGrip()
    {
#if STEAM_PRESENT
        return false;
//        return Grip.GetLastStateDown(handType);
#else
        return OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controllerInput);
#endif
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Canon_Bait") // METTRE LES TAGS DES OBJETS AUTORISES ICI
            collidingObject = other.gameObject;
        if (other.tag == "inventory")
            inventorySlotDetected = other.gameObject;
        if (other.tag == "CanonLeveler")
            canonLevelerDetected = other.gameObject;
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Canon_Bait") // METTRE LES TAGS DES OBJETS AUTORISES ICI
            collidingObject = null;
        if (other.tag == "inventory")
            inventorySlotDetected = null;
        if (other.tag == "CanonLeveler")
        {
            if (canonLevelerDetected != null)
                canonLevelerDetected.GetComponent<CanonLeveler>().LeaveLeveler();
            canonLevelerDetected = null;
        }
    }

}

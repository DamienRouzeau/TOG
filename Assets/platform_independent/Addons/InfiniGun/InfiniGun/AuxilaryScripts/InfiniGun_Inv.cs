using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InfiniGun_Inv : MonoBehaviour{
    public InfiniGun currentWeapon;
    public InfiniGun equippedPrimary;
    public InfiniGun equippedSecondary;
    bool mouseWheelCheck = false;
    public bool isPrimaryOrSecondary;
   List<InfiniGun> gunsInScene;




    void Update(){
        currentWeapon = isPrimaryOrSecondary ? equippedPrimary : equippedSecondary;
        
        if(currentWeapon && Input.GetKeyDown(currentWeapon.dropKey)){
                    currentWeapon.ToggleEquip(0);
                    currentWeapon._rb.isKinematic = false;
                    currentWeapon._rb.AddForce(Camera.main.transform.forward*2,ForceMode.Impulse);
                    currentWeapon._rb.AddTorque(Camera.main.transform.eulerAngles*5,ForceMode.Impulse);
                    if(currentWeapon == equippedPrimary){equippedPrimary = null;}
                    else if(currentWeapon == equippedSecondary){equippedSecondary = null;}
                    currentWeapon = null;
                    UpdateSelectedGun();
                }
     

        if(Input.GetAxisRaw("Mouse ScrollWheel")!=0 && !mouseWheelCheck){
            isPrimaryOrSecondary =! isPrimaryOrSecondary;
            UpdateSelectedGun();
            mouseWheelCheck = true;
                
        }else if(Input.GetAxis("Mouse ScrollWheel")==0){mouseWheelCheck = false;}

    }

    public void InitAuto(){
        gunsInScene = GameObject.FindObjectsOfType<InfiniGun>().ToList();
    }

    void UpdateSelectedGun(){
        if(isPrimaryOrSecondary){
            if(equippedPrimary){
                equippedSecondary?.ToggleEquip(2);
                equippedPrimary.ToggleEquip(1);
                
            }else if(equippedSecondary){isPrimaryOrSecondary = false; UpdateSelectedGun();}
        }else{
            if(equippedSecondary){
                equippedPrimary?.ToggleEquip(2);
                equippedSecondary.ToggleEquip(1);
            }else if(equippedPrimary){isPrimaryOrSecondary = true; UpdateSelectedGun();}
        }
    }

    public void EquipAt(InfiniGun inf){
            if(inf.gunClass == InfiniGun.GunClass.Primary&&!equippedPrimary){
                equippedPrimary = inf;
                inf.ToggleEquip(0);
                UpdateSelectedGun();
                }
            else if(inf.gunClass == InfiniGun.GunClass.Secondary && !equippedSecondary){  
                equippedSecondary = inf;
                inf.ToggleEquip(0);
                UpdateSelectedGun();
                
                }

    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(InfiniGun_Inv)),InitializeOnLoadAttribute]
public class InfiniGun_Inv_Editor : Editor{
 public override void OnInspectorGUI(){}
}
#endif
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InfiniGun_Depend_ProjectileImpactDetector : MonoBehaviour
{
    ParticleSystem ps;
    List<ParticleCollisionEvent> psce;
    [HideInInspector]public InfiniGun parentGunLogic;
    void Start(){
        ps = GetComponent<ParticleSystem>();
        psce = new List<ParticleCollisionEvent>();
        }
     private void OnParticleCollision(GameObject other) {
        int numCollisionEvents=0;
        if(psce != null) numCollisionEvents = ps.GetCollisionEvents(other, psce);
        else{Debug.LogWarning("Cannot properly register hits: \nCollision events are Null due to recompile at runtime. Please exit runtime and try again."); }

        InfiniGunTarget targ = other.GetComponent<InfiniGunTarget>();
        int i = 0;

        while (i < numCollisionEvents)
        {
            if(parentGunLogic.impactFX&&!targ&&other.gameObject.tag != "Player"){GameObject ImpactGO = Instantiate(parentGunLogic.impactFX, psce[i].intersection,Quaternion.LookRotation(psce[i].normal)); Destroy(ImpactGO, 1.5f);}
            if(parentGunLogic.impactDecal&&!targ&&other.gameObject.tag != "Player"){GameObject impactDecalGO = Instantiate(parentGunLogic.impactDecal, psce[i].intersection, Quaternion.LookRotation(parentGunLogic.flipDecal? psce[i].normal:-psce[i].normal)); if(parentGunLogic.impactDecalDecayTime!=0){Destroy(impactDecalGO, parentGunLogic.impactDecalDecayTime);}}
            if (targ){
               targ.TargateTakeDamage(Random.Range(parentGunLogic.minDamagePerround,parentGunLogic.maxDamagePerround)-Mathf.Clamp(Vector3.Distance(parentGunLogic.muzzleEnd.position,psce[i].intersection)*(parentGunLogic.damageDecreaseOverDistance),0,parentGunLogic.minDamagePerround), psce[i].intersection, parentGunLogic.headShotMultiplier);
            }
            i++;
        }
     }
}
#if UNITY_EDITOR
[CustomEditor(typeof(InfiniGun_Depend_ProjectileImpactDetector)),InitializeOnLoadAttribute]
public class InfiniGun_Depend_ProjectileImpactDetector_Editor : Editor{
 public override void OnInspectorGUI(){}
}
#endif
using UnityEngine;
[AddComponentMenu("InfiniGun/Attachment",1)]

public class InfiniGunAttachment : MonoBehaviour{
    public enum AttachmentType{Sight, Scope, Grip, miscAttachment, Suppressor, otherBarrelAttachment}
    public AttachmentType attachmentType;
    [Range(-10,10)]public float coneOfAccuracyModifier;
    public Vector3 recoilModifier;
    public float sightCenterY;
    public Sprite scopeOverlay;
    public float ScopedFOV = 25;
    public bool useSeparateScopeCam;
    public bool willHideFlash;
    public AudioClip scopeIn;
    public bool scopeSway = true;
    public float scopeSwStam = 3;
    void OnDrawGizmosSelected(){
        if(attachmentType==AttachmentType.Sight||attachmentType==AttachmentType.Scope){
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(0,sightCenterY,0),0.005f);
        }
    }
}

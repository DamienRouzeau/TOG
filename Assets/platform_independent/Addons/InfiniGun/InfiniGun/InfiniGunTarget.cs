using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[AddComponentMenu("InfiniGun/Target",2)]
public class InfiniGunTarget : MonoBehaviour
{
    public bool isDead;
    public bool CanDie = true;
    [Range(0,1000)] public int healthPoints = 100;
    [HideInInspector] public float internalHealthPoints;
    public bool Regenerate;
    public bool linearRegen;
    public float regenerationIncrement = 20;
    public float regenerationTick = 1;
    float regenTickTimer;
    public float waitToRegenerate = 2;
    float regenerateTimer;
    public bool useWorldSpaceGui = true;
    GameObject CanvObj;
    Canvas canvas;
    Image HealthMeter;
    public Gradient healthColor = new Gradient(){
        colorKeys = new GradientColorKey[]{
            new GradientColorKey(){color = Color.red, time = 0},
            new GradientColorKey(){color = new Color32(255, 165, 0,1), time = 0.75f},
            new GradientColorKey(){color = new Color32(255, 165, 0,1), time = 0.25f},
            new GradientColorKey(){color = Color.green, time = 1}
            }
        };
    public bool allowHeadshot = true;
    public Vector3 headPosition;
    [Range(1,10)]public float headRadius= 2.5f;
    public bool drawGizmos = true;
    Image hitmarker;
    bool smoothHitMark=false;
    public Sprite hitMarkerSprite;
    public AudioClip hitSound;
    AudioSource Aus;
    [Range(0,1)]public float volume=1;
    float smoothingRef;
    public List<ParticleCollisionEvent> projectileCollEvents;
    void Start(){
        projectileCollEvents = new List<ParticleCollisionEvent>();
        internalHealthPoints = healthPoints;
            if(useWorldSpaceGui){
                CanvObj = new GameObject("Canvas");
                CanvObj.transform.parent = this.transform;
                CanvObj.transform.localPosition = new Vector3(0,0, GetComponent<BoxCollider>().size.z+ 0.1f);
                canvas = CanvObj.AddComponent(typeof(Canvas)) as Canvas;
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
                HealthMeter = new GameObject("Health Meter").AddComponent(typeof(Image)) as Image;
                HealthMeter.transform.SetParent(CanvObj.transform,false);
                HealthMeter.rectTransform.sizeDelta = new Vector2(1,0.25f);
            }

        if(hitMarkerSprite && Camera.main){
                if(!GameObject.Find("hitMarker")){
                    hitmarker = new GameObject("hitMarker").AddComponent<Image>();
                    Canvas canv;
                    if(!GameObject.Find("AutoGunUI")){
                        canv = new GameObject("AutoGunUI").AddComponent<Canvas>();
                        canv.transform.position = Vector3.zero;
                        canv.transform.SetParent(Camera.main.transform);
                        canv.renderMode = RenderMode.ScreenSpaceOverlay;
                        canv.pixelPerfect = true;
                        canv.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    }
                        canv = GameObject.Find("AutoGunUI").GetComponent<Canvas>();
                        hitmarker.transform.SetParent(canv.transform);
                        hitmarker.rectTransform.anchoredPosition=Vector2.zero;
                        hitmarker.rectTransform.sizeDelta = new Vector2(25,25);
                        hitmarker.sprite = hitMarkerSprite;
                        hitmarker.color= new Color(1,1,1,0);
                        hitmarker.preserveAspect = true;
                }else{hitmarker = GameObject.Find("hitMarker").GetComponent<Image>();}
               if(hitSound){
                  Aus= gameObject.AddComponent<AudioSource>();
               }
            }
        }

    void Update(){ 
        
            if(Camera.main){CanvObj?.transform.LookAt(Camera.main.transform.position);}
            if(internalHealthPoints>0){
                if(useWorldSpaceGui){
                    HealthMeter.transform.localScale = new Vector3(Mathf.SmoothDamp(HealthMeter.transform.localScale.x, internalHealthPoints/healthPoints, ref smoothingRef, 0.1f),1,0);
                    HealthMeter.color = Vector4.MoveTowards(HealthMeter.color,healthColor.Evaluate(internalHealthPoints/healthPoints),0.05f);
                }
            }else{
                if(CanDie){isDead = true;}
                    if(useWorldSpaceGui){
                        HealthMeter.transform.localScale = new Vector3(Mathf.SmoothDamp(HealthMeter.transform.localScale.x, 0, ref smoothingRef, 0.1f),1,0);
                        HealthMeter.color = healthColor.Evaluate(internalHealthPoints/healthPoints) ;
                    }
            }
        internalHealthPoints = Mathf.Clamp(internalHealthPoints,0,healthPoints);
        if(Regenerate && Time.time > regenerateTimer && internalHealthPoints < healthPoints && !isDead){
            if(linearRegen){
                internalHealthPoints += regenerationTick*Time.deltaTime;
            }
            else if(Time.time > regenTickTimer){
                internalHealthPoints += regenerationIncrement;
                regenTickTimer = Time.time + regenerationTick;
            }
        }

                if(hitMarkerSprite&&smoothHitMark){
                 hitmarker.color = Vector4.MoveTowards(hitmarker.color,new Color(1,1,1,0),0.1f);
                 if( hitmarker.color == new Color(1,1,1,0)){smoothHitMark = false;}       
            }
    }

    public void TargateTakeDamage(float damage, Vector3 hitPoint, float headShotMultiplier){

        if(!isDead && internalHealthPoints >= 0){
        internalHealthPoints -= Vector3.Distance(transform.TransformPoint(new Vector3(headPosition.z,headPosition.x,headPosition.y)), hitPoint)< headRadius/10 ? damage*headShotMultiplier :damage;
        regenerateTimer = Time.time+waitToRegenerate;
        }
        if(hitMarkerSprite){
            hitmarker.sprite = null;
            hitmarker.sprite = hitMarkerSprite;
            hitmarker.color = Vector3.Distance(transform.TransformPoint(new Vector3(headPosition.z,headPosition.x,headPosition.y)), hitPoint)< headRadius/10 ? Color.red : Color.white;
            smoothHitMark =true;
            if(hitSound){Aus.PlayOneShot(hitSound,volume);}
        }
    }
    public void OnDrawGizmosSelected(){
        if(drawGizmos){
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(new Vector3(headPosition.z,headPosition.x,headPosition.y),headRadius/10);
        }
    }
}

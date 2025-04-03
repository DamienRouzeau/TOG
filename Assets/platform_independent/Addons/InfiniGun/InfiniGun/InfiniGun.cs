using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody)), AddComponentMenu("InfiniGun/InfiniGun",0)]
public class InfiniGun : MonoBehaviour
{
    #region Gun Setup
    public bool canFire = true;
    public enum MethodType{Raycast, Projectile}
    public MethodType methodType;
    public float minDamagePerround = 12, maxDamagePerround =20;
    public float headShotMultiplier=1;
    public float damageDecreaseOverDistance = 0.2f;
    public LayerMask CollidesWith = ~0;
    public int ammoCapacity = 30;
    public KeyCode reloadKey = KeyCode.R;
    public bool equipPreloaded = true;
    public float reloadTime = 1.5f;
    public bool isReloading;
    float reloadTime_Internal;
    public bool sequentialReload = false;
    int ammoRemaining=0;
    public int reserveAmmoCapacity = 120;

    
    public List<bool> SelectedFiremodes;
              // 0 = Safe
             // 1 = Semi
            // 2 = Auto
           // 3 = Burst
          // 4 = Auto-Burst
         // 5 = Shotgun
        // 6 = Auto-Shotgun
        
    int currentFiremode=0;
    public KeyCode fireModeSwitchKey = KeyCode.V;
    public int burstCount = 3;
    public int burstRate = 900;
    public int pelletCount;
    public int fireRate = 700;
    public KeyCode dropKey = KeyCode.E;
    float fireTimer = 0;

    #region Rigidbody setup
        public Rigidbody _rb;
        Collider _cl;
    #endregion

    #region Projectile Only Settings
    ParticleSystem firePoint = null;
    ParticleSystemRenderer firePointRenderer = null;
    InfiniGun_Depend_ProjectileImpactDetector piD;
    public int muzzleVelocity = 850;
    public float bulletDrop = 9.81f;
    public Color bulletColor = Color.yellow;
    float coneOfAccuracyInternal;
    public float coneOfAccuracy;
    public List<ParticleCollisionEvent> projectileCollEvents;
    #endregion

    #region Raycast Only Settings
    public float maxRange = 1000;
    #endregion

    #region ADS Settings
    public bool canAimDownSights = true;
    public float fOVOnADS=45;
    float normalFOV;
    float refFOVVel;
    public float gunADSCenterY;
    public float aDSSmoothTime=1.5f;
    public bool useSeperateScopeCam = true;
    public bool isADS = false;
    Vector3 adsTransitionVel;
    Vector3 centerScreen;
    #endregion
    
    #region Attachment Settings
    [System.Serializable] public class AttachmentMountPoints{
        public Transform mountPoint;
        public GameObject Attachment;
        public InfiniGunAttachment atchScript;
        public GameObject atchGO;
    }
        public List<AttachmentMountPoints> attachmentMountPoints;
        bool sightAttached = false;
        bool scopeAttached = false;
        bool swayScope;
        float swayIntensity =0;
        public KeyCode steadyScope = KeyCode.LeftShift;
        Image scopeSteadyStamDisp;
        float scopeSteadyStam;
        float scopeSteadyStamTimer;
        Canvas Overlay;
        GameObject ScopeGO;
        Image scopeOverlay;
        Camera scopeCam;
        float collectiveCOAReduction;
        float atchSightCenterY;
        float atchSightZOut;
        Vector3 collectiveRecoilForces;
        Vector3 attachmentRecoilForces;
        bool suppressed = false;
        bool hideFlash;
        List<MeshRenderer> childMeshRenderers; 
        List<LineRenderer> childLineRenderers;
       
    #endregion
    
    #region UI Settings
    public bool autoGenerateUIElements;
    public Font uiFont;
    Image uiPanel;
    Image uiReloadTimer;
    Text currentMagUI;
    Text ammoReserveUI;
    Text fireModeUI;
    public bool autoGenCrosshair = true;
    public Sprite CrosshairImg;
    Image CH;
   
    #endregion
    
    public Transform muzzleEnd; 
    public GameObject muzzleFlashPrefab;
    ParticleSystem muzzleFlashPS;
    public GameObject shellEjectionFX_prefab;
    public Transform shellEjectionFX_pos;
    ParticleSystem shellEjectionFX_ps;
    public GameObject impactFX;
    public GameObject impactDecal;
    public bool flipDecal = false;
    public float impactDecalDecayTime = 15;

    #region Audio Settings
        bool useAudio;
        public float sFXVolume = 0.75f;
        public bool useNormalAudio;
        bool useSuppressedAudio;
        bool dryShotAudio;
        public List<AudioClip> fireSounds;
        public List<AudioClip> suppressedSounds;
        public List<AudioClip> dryShot;
        public AudioClip scopeSound;
        bool scopesounded;
        public AudioClip boltOut;
        public AudioClip boltIn;
        public bool Reprime = false;
        AudioSource AS;
    #endregion
    
    public bool isSprinting = false;
    public float swayAmount= 50f;
    public bool useRecoil = true;
    public Vector3 recoilForceIntensity = new Vector3(1f,1f,1f);
    public float recoilRotationMultiplier = 1f;
    Vector3 recoilPos;
    Vector3 initLocalPos;
    Vector3 swayVel;

    #region Bundled Animation Settings
    public bool useBundled = false;
    Animator BundledAnimController = null;

    
    #endregion
    

    #region Equip Settings and Inventory Mng
    public bool isEquippedToArsenal = false;
    public bool isEquipped;
    Collider PlayerColl;
    public Vector3 equipPosition = new Vector3(0.3f,-0.3f,1f);
    public enum GunClass{Primary, Secondary}
    public GunClass gunClass;
    float pickUpTimer = 1f;
    InfiniGun_Inv invMng;
    #endregion
    public bool drawSetupGizmos = true;

    #endregion
    private void Start() {if(Camera.main){  
        #region VFX Initialization
        if(muzzleFlashPrefab){muzzleFlashPS = GameObject.Instantiate(muzzleFlashPrefab,muzzleEnd).GetComponent<ParticleSystem>();if(!muzzleFlashPS){Debug.Log("No particle system located on "+muzzleFlashPrefab.gameObject.name+". Muzzle Flash cannot be used.");}}
        if(shellEjectionFX_prefab){if(shellEjectionFX_pos){shellEjectionFX_ps = GameObject.Instantiate(shellEjectionFX_prefab,shellEjectionFX_pos).GetComponent<ParticleSystem>(); if(!shellEjectionFX_ps){Debug.Log("No particle system located on "+shellEjectionFX_prefab.gameObject.name+". Shell Ejection FX cannot be used.");}}else{Debug.LogError("No transform to create shell ejection fx on.");}}
        #endregion
        


        InitializeAttachments();        
        SwitchFireMode(0);
        if(SelectedFiremodes.Count>1){currentFiremode = SelectedFiremodes[2] ? 2 : SelectedFiremodes[3] ?  3 : SelectedFiremodes[4] ? 4 : SelectedFiremodes[5] ? 5 :  SelectedFiremodes[6] ? 6 : currentFiremode;} 
        InitializeUI();        
        if(methodType == MethodType.Projectile){InitializeProjectiles();}
        _rb = gameObject.GetComponent<Rigidbody>();
        _cl = gameObject.GetComponent<Collider>() ? gameObject.GetComponent<Collider>() : gameObject.AddComponent<BoxCollider>();

        #region AudioInitialization


        useAudio = fireSounds.Any()||suppressedSounds.Any()||dryShot.Any()||boltIn||boltOut;
        useNormalAudio = fireSounds.Any();
        if(suppressed){useSuppressedAudio = suppressedSounds.Any();}
        dryShotAudio = dryShot.Any();
        if(useAudio){
            if(useNormalAudio){useNormalAudio = fireSounds[0] ==null ? false:true;}
            if(useSuppressedAudio){useSuppressedAudio = suppressedSounds[0] == null? false:useSuppressedAudio;}
            if(dryShotAudio){dryShotAudio = dryShot[0] == null? false:dryShotAudio;}
        }
        #endregion

        #region Start Initializarion
       
        centerScreen = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 25));
        if(!Camera.main.GetComponent<InfiniGun_Inv>()){Camera.main.gameObject.AddComponent<InfiniGun_Inv>().InitAuto();}
        invMng = GameObject.FindObjectOfType<InfiniGun_Inv>();
        collectiveRecoilForces = new Vector3(recoilForceIntensity.x + (recoilForceIntensity.x * attachmentRecoilForces.x / 100), recoilForceIntensity.y + (recoilForceIntensity.y * attachmentRecoilForces.y / 100),recoilForceIntensity.z + (recoilForceIntensity.z * attachmentRecoilForces.z / 100));
        PlayerColl =  Camera.main.GetComponentInParent<Collider>();
        childMeshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();
        childLineRenderers = GetComponentsInChildren<LineRenderer>().ToList();
        normalFOV = Camera.main.fieldOfView;
        coneOfAccuracyInternal = coneOfAccuracy > 0? Mathf.Clamp((coneOfAccuracy - collectiveCOAReduction), 0, Mathf.Infinity):0;        
        if (useAudio||boltIn||boltOut) { AS = gameObject.AddComponent(typeof(AudioSource)) as AudioSource; }
        ammoRemaining = equipPreloaded? ammoCapacity:0;
        initLocalPos = this.transform.localPosition;
        if(useBundled){BundledAnimController = GetComponentInChildren<Animator>();}
        #endregion
        }else{Debug.LogError("No Main Camera found in scene. InfiniGun and sub-systems cannot function.");}

    }


    private void Update() {
        if(Camera.main){
        centerScreen = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, isADS?50:25));
        Quaternion lkAt = Quaternion.LookRotation( centerScreen-transform.position);
        
        if(!isEquippedToArsenal){
            
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _cl.enabled = true;
            if(Time.time > pickUpTimer){
                if(PlayerColl && Vector3.Distance(PlayerColl.transform.position, transform.localPosition)<2f){
                    invMng?.EquipAt(this);
                    pickUpTimer = (Time.time + 1.5f);
                }
            }
            if(scopeAttached && ScopeGO){
                if(Overlay){
                Overlay.gameObject.SetActive(false);
                scopeOverlay.color = new Color(255,255,255,0);
                }
                ScopeGO.SetActive(true);
                if(useSeperateScopeCam){scopeCam.gameObject.SetActive(false);}

                }
        }

        else if(isEquipped) {

            #region Auto UI
            if(autoGenerateUIElements){
                currentMagUI.text = ammoRemaining.ToString("0000");
                ammoReserveUI.text = reserveAmmoCapacity.ToString("/0000");
                string currentFiremodeLable = string.Empty;
                if(currentFiremode == 2 || currentFiremode == 6){currentFiremodeLable = "Auto";}
                else if(currentFiremode == 3 || currentFiremode == 4){currentFiremodeLable = "Burst";}
                else if(currentFiremode == 1 || currentFiremode == 5){currentFiremodeLable = "Semi";}
                else if(currentFiremode == 0){currentFiremodeLable = "Safe";}
                fireModeUI.text = ("["+currentFiremodeLable+"]");
            }
            #endregion 

            if(Input.GetKeyDown(fireModeSwitchKey)){SwitchFireMode(currentFiremode+1);}
            
            #region Reload/ADS Cancelling
            if(canAimDownSights){
                if(Input.GetButtonDown("Fire2")){
                    isReloading = false;
                    isADS = true;
                    if(useBundled){BundledAnimController.SetTrigger("Panic");}
                    
                }
                if(Input.GetButtonUp("Fire2")){
                    isADS = false;
                    scopesounded = false;
                }
            }
            if(isReloading&&Input.GetButtonDown("Fire1")&&ammoRemaining>0){isReloading = false;}
            #endregion
            
            if(!isADS){

                if(autoGenCrosshair){CH.color = Vector4.MoveTowards(CH.color, Color.white,0.25f);}

                if(scopeAttached && ScopeGO){
                    if(Overlay){
                        Overlay.gameObject.SetActive(false);
                        scopeOverlay.color = new Color(255,255,255,0);
                        if(swayScope && scopeSteadyStam>0){scopeSteadyStamTimer = scopeSteadyStam; scopeSteadyStamDisp.transform.localScale = Vector3.one;}
                    }
                ScopeGO.SetActive(true);
                if(childMeshRenderers.Any()){for(int i = 0; i<childMeshRenderers.Count; i++){childMeshRenderers[i].enabled = true;}}
                if(useSeperateScopeCam){scopeCam.gameObject.SetActive(false);}

                }
                Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, normalFOV, ref refFOVVel, aDSSmoothTime/10);
                
                if(swayAmount>0){
                    Vector3 swayTarget = new Vector3(Mathf.Clamp(Input.GetAxis("Mouse X") * swayAmount / 100, -0.2f, 0.2f), Mathf.Clamp(Input.GetAxis("Mouse Y") * swayAmount / 100, -0.02f, 0.02f), 0) + initLocalPos;
                    this.transform.localPosition = Vector3.SmoothDamp(this.transform.localPosition, swayTarget, ref swayVel, 0.15f);
                    transform.rotation = Quaternion.Slerp(transform.rotation,lkAt,5*Time.deltaTime);
                    float swing = Mathf.Clamp(-Input.GetAxis("Mouse X") * swayAmount, -20, 20);
                    this.transform.localRotation = isReloading ? this.transform.localRotation : Quaternion.Slerp(transform.localRotation, Quaternion.Euler(this.transform.localEulerAngles.x, this.transform.localEulerAngles.y, swing),0.15f);
                    }
                } else{
                    if(autoGenCrosshair){CH.color = Vector4.MoveTowards(CH.color, new Vector4(1,1,1,0),0.1f);}
                    if(scopeAttached && ScopeGO && Camera.main.fieldOfView - fOVOnADS <=2f){

                        if(swayScope){
                            bool isSteadying = scopeSteadyStam>0 ? (Input.GetKey(steadyScope) && scopeSteadyStamTimer > 0.015f) :(Input.GetKey(steadyScope));
                           if(isSteadying){
                                swayIntensity = Mathf.MoveTowards(swayIntensity, 0f,0.05f);
                                if(scopeSteadyStam>0){
                                    scopeSteadyStamTimer -= scopeSteadyStamTimer>0? 1*Time.deltaTime:0;
                                    float x = Mathf.Clamp(Mathf.MoveTowards(scopeSteadyStamDisp.transform.localScale.x, (scopeSteadyStamTimer/scopeSteadyStam), 0.01f), 0.001f,1);
                                    scopeSteadyStamDisp.transform.localScale = new Vector3(x,1,1);
                                    }
                                } else{
                                swayIntensity = Mathf.MoveTowards(swayIntensity, 2f,0.01f);
                                if(scopeSteadyStam>0 && !Input.GetKey(steadyScope)){
                                    scopeSteadyStamTimer += scopeSteadyStamTimer<scopeSteadyStam? 1*Time.deltaTime:0;
                                    float x = Mathf.Clamp(Mathf.MoveTowards(scopeSteadyStamDisp.transform.localScale.x, (scopeSteadyStamTimer/scopeSteadyStam), 0.01f), 0.001f,1);
                                    scopeSteadyStamDisp.transform.localScale = new Vector3(x,1,1);
                                    }
                                }
                            transform.localEulerAngles = new Vector3((Mathf.Sin(Time.time*2)/2)*swayIntensity,(Mathf.Cos(Time.time))*swayIntensity,transform.localEulerAngles.z);
                        }

                        Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, ScopeGO.GetComponent<InfiniGunAttachment>().ScopedFOV, ref refFOVVel, 0.075f);
                        if(Overlay){
                            Overlay.gameObject.SetActive(true);
                            scopeOverlay.color = Vector4.MoveTowards(scopeOverlay.color,new Color(255,255,255,1),0.15f);
                        }    
                        ScopeGO.SetActive(false);
                        if(useSeperateScopeCam){
                            scopeCam.gameObject.SetActive(true);
                            if(scopeAttached&&scopeSound&&!scopesounded){AS.PlayOneShot(scopeSound,sFXVolume);scopesounded = true;}

                            scopeCam.fieldOfView = Camera.main.fieldOfView;
                            }
                            if(childMeshRenderers.Any()){for(int i = 0; i<childMeshRenderers.Count; i++){childMeshRenderers[i].enabled = false;}}
                    }
                    
                    if(Camera.main.fieldOfView >= fOVOnADS){ Camera.main.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, fOVOnADS, ref refFOVVel, aDSSmoothTime/10);}
                    transform.rotation = Quaternion.Slerp(transform.rotation,lkAt,5*Time.deltaTime);
                    transform.localPosition =  Vector3.SmoothDamp(this.transform.localPosition,new Vector3(0,-(sightAttached||scopeAttached? atchSightCenterY  :gunADSCenterY),(scopeAttached? 0.5f:equipPosition.z-(sightAttached?atchSightZOut:0))), ref adsTransitionVel, aDSSmoothTime/10);
                    if(swayAmount>0 && !scopeAttached){ 
                        float swing = Mathf.Clamp(-Input.GetAxis("Mouse X") * swayAmount, -20, 20);
                        this.transform.localRotation = isReloading ? this.transform.localRotation : Quaternion.Slerp(transform.localRotation, Quaternion.Euler(this.transform.localEulerAngles.x, this.transform.localEulerAngles.y, swing),0.15f);
                    }
                }  
                

            if (methodType == MethodType.Projectile){
                var particleSystem_Shape = firePoint.shape;
                particleSystem_Shape.enabled = coneOfAccuracyInternal > 0 ? true : false;
                particleSystem_Shape.angle = coneOfAccuracyInternal > 0 ? coneOfAccuracyInternal / 2 : 0;
            }
            
            
            if(canFire && !isReloading && !isSprinting){
                if (ammoRemaining > 0  && Time.time > fireTimer){
                    if(currentFiremode != 0){
                    if((currentFiremode == 1|| currentFiremode == 3||currentFiremode == 5) ? Input.GetButtonDown("Fire1") : Input.GetButton("Fire1")){
                        StartCoroutine(Fire());
                        fireTimer = Time.time + (1f / (currentFiremode == 3 || currentFiremode == 4 ? ((burstRate/burstCount)/2)*Time.deltaTime : fireRate * Time.deltaTime));
                    }
                }
            } else if (Input.GetButtonDown("Fire1") && Time.time > fireTimer) {SFX_FireShot(dryShotAudio); fireTimer = Time.time + 0.5f / (fireRate * Time.deltaTime);}
        }

            #region Reload

        
        //This is called when a reload is initiated.
         if (Input.GetKeyDown(reloadKey) && reserveAmmoCapacity > 0 &&(sequentialReload? ammoRemaining != ammoCapacity: ammoRemaining != ammoCapacity+1) && !isReloading){
            reloadTime_Internal = Time.time + (reloadTime / (sequentialReload? ammoCapacity:1));
            if(boltOut){AS.PlayOneShot(boltOut,sFXVolume);}
            isReloading = true;
            isADS = false;
            if (useBundled){BundledAnimController.SetTrigger("Reload");}  
        }
        //This is called when a reload is finished.
            if((Time.time>reloadTime_Internal && isReloading)){
                 if(boltIn){AS.PlayOneShot(boltIn,sFXVolume);}
                if(sequentialReload){
                    if(ammoRemaining < ammoCapacity){
                        if(useBundled && ammoRemaining < ammoCapacity -1){BundledAnimController.SetTrigger("Reload");}
                        ammoRemaining++;
                        reserveAmmoCapacity--;
                        reloadTime_Internal = Time.time + (reloadTime/ammoCapacity);
                    }   else{
                        isReloading = false;
                        this.transform.LookAt(centerScreen);
                        isADS = Input.GetButton("Fire2");
                    }
                }
                
                else{
                    if(ammoRemaining == 0 && ammoCapacity > 0){ ammoRemaining = Mathf.Clamp(reserveAmmoCapacity, ammoRemaining, ammoCapacity); reserveAmmoCapacity -= ammoRemaining; }
                    else if (ammoRemaining <= ammoCapacity) { 
                        int dif= Mathf.Abs(ammoRemaining - (ammoCapacity+1));
                        ammoRemaining += Mathf.Clamp(reserveAmmoCapacity, 0, dif); 
                        reserveAmmoCapacity -= dif;
                        reserveAmmoCapacity = reserveAmmoCapacity<0? 0 : reserveAmmoCapacity; 
                        }
                    isReloading = false;
                    isADS = Input.GetButton("Fire2");
                    this.transform.LookAt(centerScreen);
                }
            }


            if(autoGenerateUIElements){
                uiReloadTimer.enabled = isReloading;
                if(isReloading){uiReloadTimer.rectTransform.sizeDelta = new Vector2(uiPanel.rectTransform.sizeDelta.x*(-(Time.time-reloadTime_Internal)/reloadTime),31);}
            }

            #endregion
            
            }
        
    
        
        }
    }

    IEnumerator Fire(){
        
        int bulletsPerfire = currentFiremode == 3 || currentFiremode == 4 ? burstCount :
                currentFiremode == 5 || currentFiremode == 6? pelletCount : 1;
        if(methodType == MethodType.Projectile){
            if(currentFiremode != 5&& currentFiremode != 6){
               for(int i = 0; i < bulletsPerfire; i++){
                        if(ammoRemaining>0){
                            if(muzzleFlashPS&& !hideFlash){muzzleFlashPS.Play();}
                            if(shellEjectionFX_ps){shellEjectionFX_ps.Play();}
                            if(useBundled) { BundledAnimController.SetTrigger("Fire"); }
                            ammoRemaining--;
                            firePoint.Emit(1);
                                if(useRecoil){
                                   StartCoroutine(Recoil());
                                }
                            SFX_FireShot(false);
                        yield return currentFiremode == 3 || currentFiremode == 4 ?  new WaitForSeconds( 1f / (burstRate * Time.deltaTime)) : null;
                    }
                  }  
                }
                else{
                    firePoint.Emit(bulletsPerfire);
                    if(muzzleFlashPS && !hideFlash){muzzleFlashPS.Play();}
                    if(shellEjectionFX_ps){shellEjectionFX_ps.Play();}
                    if(useBundled) { BundledAnimController.SetTrigger("Fire"); }
                    ammoRemaining--;
                    if(useRecoil){StartCoroutine(Recoil());}
                    SFX_FireShot(false);
                    yield return null;
                    }
                if(boltOut&&boltIn&&Reprime){StartCoroutine(SFX_Prime((1/(fireRate*Time.deltaTime))/2)); if(scopeAttached){isADS = false;}}
            }

        else if(methodType == MethodType.Raycast){
            Vector3 startForward = muzzleEnd.forward;
            Vector3 startPos = muzzleEnd.position;
            RaycastHit hit;
            InfiniGunTarget target = null;
                for(int i = 0; i < bulletsPerfire; i++){
                    
                    
                    if(currentFiremode != 5|| currentFiremode != 6){
                            if(Physics.Raycast(startPos,Quaternion.Euler(Random.Range(coneOfAccuracyInternal/2, -coneOfAccuracyInternal/2),Random.Range(coneOfAccuracyInternal/2, -coneOfAccuracyInternal/2), Random.Range(coneOfAccuracyInternal/2, -coneOfAccuracyInternal/2))*startForward , out hit,maxRange,CollidesWith)){
                                target = hit.transform.gameObject.GetComponent<InfiniGunTarget>();
                                
                                if(impactFX&&!target&&hit.transform.gameObject.tag!="Player"){GameObject impactFXGO = Instantiate(impactFX, hit.point, Quaternion.LookRotation(hit.normal)); Destroy(impactFXGO, 1.5f);}
                                if(impactDecal&&!target&&hit.transform.gameObject.tag!="Player"){GameObject impactDecalGO = Instantiate(impactDecal, hit.point+new Vector3(0,0.01f,0), Quaternion.LookRotation(flipDecal? hit.normal:-hit.normal)); Destroy(impactDecalGO, impactDecalDecayTime);}
                                }
                        if(i==0){
                            ammoRemaining--;
                            if(muzzleFlashPS && !hideFlash){muzzleFlashPS.Play();}
                            if(shellEjectionFX_ps){shellEjectionFX_ps.Play();}
                            if(useAudio){AS.PlayOneShot(useSuppressedAudio? suppressedSounds[Random.Range(0,suppressedSounds.Count)]: fireSounds[Random.Range(0, fireSounds.Count)],sFXVolume);}
                            if(useBundled) { BundledAnimController.SetTrigger("Fire"); }
                            if(useRecoil){StartCoroutine(Recoil());}
                            if(boltOut&&boltIn&&Reprime){StartCoroutine(SFX_Prime((1/(fireRate*Time.deltaTime))/2)); if(scopeAttached){isADS = false;}}
                        }
                        target?.TargateTakeDamage(Random.Range(minDamagePerround,maxDamagePerround)-Mathf.Clamp(Vector3.Distance(startPos,hit.point)*(damageDecreaseOverDistance),0,minDamagePerround), hit.point, headShotMultiplier);
                    }

                        else if(ammoRemaining>0){

                            if(Physics.Raycast(muzzleEnd.position,Quaternion.Euler(Random.Range(coneOfAccuracyInternal/2, -coneOfAccuracyInternal/2),Random.Range(coneOfAccuracyInternal/2, -coneOfAccuracyInternal/2), Random.Range(coneOfAccuracyInternal/2, -coneOfAccuracyInternal/2))*muzzleEnd.forward , out hit,maxRange,CollidesWith)){
                                target = hit.transform.gameObject.GetComponent<InfiniGunTarget>();
                                
                                if(impactFX&&!target&&hit.transform.gameObject.tag!="Player"){GameObject impactFXGO = Instantiate(impactFX, hit.point, Quaternion.LookRotation(hit.normal)); Destroy(impactFXGO, 1.5f);}
                                if(impactDecal&&!target&&hit.transform.gameObject.tag!="Player"){GameObject impactDecalGO = Instantiate(impactDecal, hit.point+new Vector3(0,0.01f,0), Quaternion.LookRotation(flipDecal? hit.normal:-hit.normal));if(impactDecalDecayTime!=0){ Destroy(impactDecalGO, impactDecalDecayTime);}}
                                }
                            if(muzzleFlashPS && !hideFlash){muzzleFlashPS.Play();}
                            if(shellEjectionFX_ps){shellEjectionFX_ps.Play();}
                            if(useAudio){AS.PlayOneShot(useSuppressedAudio? suppressedSounds[Random.Range(0,suppressedSounds.Count)]: fireSounds[Random.Range(0, fireSounds.Count)],sFXVolume);}
                            if(useBundled) { BundledAnimController.SetTrigger("Fire"); }
                            ammoRemaining--;
                            target?.TargateTakeDamage(Random.Range(minDamagePerround,maxDamagePerround)-Mathf.Clamp(Vector3.Distance(startPos,hit.point)*(damageDecreaseOverDistance),0,minDamagePerround), hit.point, headShotMultiplier);
                            if(useRecoil){StartCoroutine(Recoil());}
                            if(boltOut&&boltIn&&Reprime){StartCoroutine(SFX_Prime((1/(fireRate*Time.deltaTime))/2)); if(scopeAttached){isADS = false;}}
                        }
                    
                    yield return currentFiremode == 3 || currentFiremode == 4 ?  new WaitForSeconds( 1f / (burstRate * Time.deltaTime)) : null;
                
            }
        }
    }

    IEnumerator Recoil(){
        recoilPos = new Vector3(Random.Range(collectiveRecoilForces.x, -collectiveRecoilForces.x)/50,Random.Range(collectiveRecoilForces.y, -collectiveRecoilForces.y)/50,Random.Range(collectiveRecoilForces.z, -collectiveRecoilForces.z)/-50);
        Quaternion recoilRotation = Quaternion.Euler(transform.localEulerAngles -new Vector3(collectiveRecoilForces.y,Random.Range(collectiveRecoilForces.x,-collectiveRecoilForces.x),0)*recoilRotationMultiplier);
        this.transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition+ recoilPos, 0.5f);
        transform.localRotation = recoilRotation;
        yield return null;
    }

    IEnumerator SFX_Prime(float waitBeforePlay){
        yield return new WaitForSeconds(waitBeforePlay);
        AS.PlayOneShot(boltOut,sFXVolume);
        yield return new WaitForSeconds(boltOut.length);
        AS.PlayOneShot(boltIn,sFXVolume);
        isADS = Input.GetButton("Fire2");
        yield return null;
    }


    public void SFX_FireShot(bool isDry){
        if(useAudio){
            if(!isDry){
                if(useNormalAudio && !useSuppressedAudio){AS.PlayOneShot(fireSounds[Random.Range(0, fireSounds.Count)],sFXVolume);}
                if(useSuppressedAudio){AS.PlayOneShot(suppressedSounds[Random.Range(0,suppressedSounds.Count)],sFXVolume);}
            }else{AS.PlayOneShot(dryShot[Random.Range(0,dryShot.Count)],sFXVolume);}
        }
    }
    public void SwitchFireMode(int cycleStart){
        if(SelectedFiremodes.Contains(true)){
            if(cycleStart < SelectedFiremodes.Count-1){
                if(SelectedFiremodes[cycleStart]){
                    currentFiremode = cycleStart;
                }else{SwitchFireMode(cycleStart+1);}
            }
            else{SwitchFireMode(0);}
        }
        else{print("No firemodes selected! Avoided stack overflow. Please stop playmode and enable a firemode.");}
    }
    public void ToggleEquip(int mode){
        if(mode == 0){
            if(!isEquippedToArsenal){
                isEquippedToArsenal=true;
                if(muzzleEnd){
                    _cl.enabled = false;
                    this.transform.parent = Camera.main.transform;
                    transform.localPosition = equipPosition;
                    this.transform.LookAt(centerScreen);
                    initLocalPos = this.transform.localPosition;
                    isEquippedToArsenal = true;
                    if(boltOut&&boltIn){StartCoroutine(SFX_Prime(0));}
                    if(autoGenerateUIElements){
                        uiPanel.enabled = true;
                        currentMagUI.enabled = true;
                        ammoReserveUI.enabled = true;
                        fireModeUI.enabled = true;
                        Font fontU = uiFont ? uiFont : Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                        currentMagUI.font = fontU; ammoReserveUI.font = fontU; fireModeUI.font = fontU;
                    }
                    if(autoGenCrosshair){
                        CH.enabled = true;
                        CH.color = Color.clear;
                        CH.sprite = CrosshairImg;
                    }
                    _rb.useGravity = false;
                    _rb.isKinematic = true;
                    _cl.enabled = false;
                                if(AS){AS.volume = sFXVolume;}

                }else{Debug.LogError("No 'Muzzle Tip' assigned. Cannot proceed to equip this gun."); }  
            }
            else{
                Camera.main.fieldOfView = normalFOV;
                isADS = false;
                _cl.enabled = true;
                this.transform.parent = null;
                if(childMeshRenderers.Any()){for(int i = 0; i<childMeshRenderers.Count; i++){childMeshRenderers[i].enabled = true;}}
                if(childLineRenderers.Any()){for(int i = 0; i<childLineRenderers.Count; i++){childLineRenderers[i].enabled = true;}}
                _rb.useGravity = true;
                _rb.isKinematic = false;
                isEquippedToArsenal = false;
                if(autoGenerateUIElements){
                    uiPanel.enabled = false;
                    currentMagUI.enabled = false;
                    ammoReserveUI.enabled = false;
                    fireModeUI.enabled = false;
                    uiReloadTimer.enabled = false;
                }
                 if(autoGenCrosshair){
                        CH.enabled = false;
                        CH.color = Color.clear;
                        CH.sprite = CrosshairImg;
                    }
                 pickUpTimer = (Time.time + 1.5f);
                 if(AS){AS.volume = 0;} 
            }

        }

        if(mode == 1 && !isEquipped){
            isADS = false;
            isReloading = false;
            transform.localPosition = equipPosition;
            this.transform.LookAt(centerScreen);
            isEquipped = true;
            if(boltOut&&boltIn){StartCoroutine(SFX_Prime(0));}
            if(AS){AS.volume = sFXVolume;}

            if(autoGenerateUIElements){
                        uiPanel.enabled = true;
                        currentMagUI.enabled = true;
                        ammoReserveUI.enabled = true;
                        fireModeUI.enabled = true;
                        Font fontU = uiFont ? uiFont : Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                        currentMagUI.font = fontU; ammoReserveUI.font = fontU; fireModeUI.font = fontU;
                    }
                    if(autoGenCrosshair){
                        CH.enabled = true;
                        CH.color = Color.clear;
                        CH.sprite = CrosshairImg;
                    }
            if(childMeshRenderers.Any()){for(int i = 0; i<childMeshRenderers.Count; i++){childMeshRenderers[i].enabled = true;}}
            if(childLineRenderers.Any()){for(int i = 0; i<childLineRenderers.Count; i++){childLineRenderers[i].enabled = true;}}
        }
    
        if(mode == 2){
            if(scopeAttached && ScopeGO){
                    if(Overlay){
                        Overlay.gameObject.SetActive(false);
                        scopeOverlay.color = new Color(255,255,255,0);
                        if(swayScope && scopeSteadyStam>0){scopeSteadyStamTimer = scopeSteadyStam; scopeSteadyStamDisp.transform.localScale = Vector3.one;}
                    }
                ScopeGO.SetActive(true);
                if(useSeperateScopeCam){scopeCam.gameObject.SetActive(false);}
                }   
            if(AS){AS.volume = 0;}    
            Camera.main.fieldOfView = normalFOV;
            isADS = false;
            transform.localPosition = equipPosition;
            this.transform.LookAt(centerScreen);    
            isReloading = false;
            isEquipped = false;
            if(childMeshRenderers.Any()){for(int i = 0; i<childMeshRenderers.Count; i++){childMeshRenderers[i].enabled = false;}}
            if(childLineRenderers.Any()){for(int i = 0; i<childLineRenderers.Count; i++){childLineRenderers[i].enabled = false;}}

        }
    }
    public void InitializeAttachments(){
        
        for(int i = 0; i<attachmentMountPoints.Count; i++){
            if(attachmentMountPoints[i].mountPoint != null){
                if(attachmentMountPoints[i].mountPoint.GetComponentInChildren<InfiniGunAttachment>()){DestroyImmediate(attachmentMountPoints[i].mountPoint.GetComponentInChildren<InfiniGunAttachment>().gameObject);}
                if(attachmentMountPoints[i].Attachment){
                    attachmentMountPoints[i].atchGO = Instantiate(attachmentMountPoints[i].Attachment,attachmentMountPoints[i].mountPoint);
                    attachmentMountPoints[i].atchScript = attachmentMountPoints[i].atchGO.GetComponent<InfiniGunAttachment>();
                    if(attachmentMountPoints[i].atchScript){
                        collectiveCOAReduction += attachmentMountPoints[i].atchScript.coneOfAccuracyModifier;
                        attachmentRecoilForces += attachmentMountPoints[i].atchScript.recoilModifier;
                        if(attachmentMountPoints[i].atchScript.attachmentType == InfiniGunAttachment.AttachmentType.Sight){
                            sightAttached = true;
                            atchSightCenterY = (attachmentMountPoints[i].mountPoint.localPosition.y+attachmentMountPoints[i].atchScript.sightCenterY)*transform.localScale.y;
                            atchSightZOut = attachmentMountPoints[i].mountPoint.localPosition.z;
                            }
                        if(attachmentMountPoints[i].atchScript.attachmentType == InfiniGunAttachment.AttachmentType.Scope){
                            scopeAttached=true; 
                            ScopeGO = attachmentMountPoints[i].atchGO; 
                            atchSightCenterY = (attachmentMountPoints[i].mountPoint.localPosition.y+attachmentMountPoints[i].atchScript.sightCenterY);
                            scopeSound = attachmentMountPoints[i].atchScript.scopeIn;
                            swayScope = attachmentMountPoints[i].atchScript.scopeSway ? true : swayScope;
                            useSeperateScopeCam = attachmentMountPoints[i].atchScript.useSeparateScopeCam ? true : useSeperateScopeCam;
                            scopeSteadyStam = attachmentMountPoints[i].atchScript.useSeparateScopeCam ? attachmentMountPoints[i].atchScript.scopeSwStam : scopeSteadyStam;
                            scopeSteadyStamTimer = scopeSteadyStam;
                            }
                        if(attachmentMountPoints[i].atchScript.attachmentType == InfiniGunAttachment.AttachmentType.Suppressor){suppressed = true;}
                        if(attachmentMountPoints[i].atchScript.attachmentType == InfiniGunAttachment.AttachmentType.otherBarrelAttachment && attachmentMountPoints[i].atchScript.willHideFlash || suppressed){hideFlash = true;} 
                    }
                }
        }
        else{print("No mount point found found for attachment No." + (i+1) + ", Skipping attachment...");}
    }
        if(Overlay){DestroyImmediate(Overlay.gameObject);}
        if(scopeCam){DestroyImmediate(scopeCam.gameObject);}
        if(scopeAttached){
        if(ScopeGO && ScopeGO.GetComponent<InfiniGunAttachment>().scopeOverlay){
            if(!Overlay){Overlay = new GameObject("Overlay").AddComponent<Canvas>();}
            Overlay.transform.SetParent(transform);
            Overlay.renderMode = RenderMode.ScreenSpaceOverlay;
            Overlay.pixelPerfect = true;
            CanvasScaler csl = Overlay.gameObject.AddComponent<CanvasScaler>();
            csl.referenceResolution = new Vector2(1920,1080);
            csl.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            csl.matchWidthOrHeight = 1;
            
            scopeOverlay = new GameObject("overlay").AddComponent<Image>();
            scopeOverlay.transform.SetParent(Overlay.transform);
            scopeOverlay.transform.localPosition = Vector3.zero;
            scopeOverlay.sprite = ScopeGO.GetComponent<InfiniGunAttachment>().scopeOverlay;
            scopeOverlay.SetNativeSize(); 
            scopeOverlay.color = new Color(255,255,255,0);
            Overlay.gameObject.SetActive(false);
                if(swayScope){
                Text tx = new GameObject("Hold To Steady").AddComponent<Text>();
                tx.transform.SetParent(Overlay.transform);
                tx.rectTransform.anchoredPosition = new Vector2(0,-488);
                tx.fontSize = 25;
                tx.rectTransform.sizeDelta = new Vector2(275,30);
                tx.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                tx.text = "Hold "+steadyScope.ToString()+" To Steady Gun";
                if(scopeSteadyStam  > 0){
                    scopeSteadyStamDisp = new GameObject("ScopeSteadyStamDisp").AddComponent<Image>();
                    scopeSteadyStamDisp.transform.SetParent(Overlay.transform);
                    scopeSteadyStamDisp.rectTransform.sizeDelta = new Vector2(350,10);
                    scopeSteadyStamDisp.rectTransform.anchoredPosition = new Vector2(0,-515);
                }
            }
        }
            if(useSeperateScopeCam){
                if(!scopeCam){scopeCam = new GameObject("ScopeCam").AddComponent<Camera>();}
                scopeCam.gameObject.SetActive(false);
                scopeCam.transform.SetParent(transform);
                scopeCam.transform.localPosition = new Vector3(0,atchSightCenterY,-0.5f);
                scopeCam.transform.localRotation = Quaternion.identity;
                scopeCam.fieldOfView = ScopeGO.GetComponent<InfiniGunAttachment>().ScopedFOV;
            }
        }
        
    }
    public void InitializeProjectiles(){
            projectileCollEvents = new List<ParticleCollisionEvent>();
            firePoint = muzzleEnd.gameObject.AddComponent(typeof(ParticleSystem)) as ParticleSystem;
            piD = muzzleEnd.gameObject.AddComponent(typeof(InfiniGun_Depend_ProjectileImpactDetector)) as InfiniGun_Depend_ProjectileImpactDetector; 
            piD.parentGunLogic = this;
            var particleSystem_Main = firePoint.main;
            particleSystem_Main.playOnAwake = false;
            particleSystem_Main.startLifetime = 1f;
            particleSystem_Main.startSize3D = true;
            particleSystem_Main.startSizeX = 0.01f;
            particleSystem_Main.startSizeY = 0.01f;
            particleSystem_Main.startSizeZ = 0.5f;
            particleSystem_Main.startColor = bulletColor;
            particleSystem_Main.startSpeed = muzzleVelocity/5;
            particleSystem_Main.gravityModifier = bulletDrop/2;
            particleSystem_Main.simulationSpace = ParticleSystemSimulationSpace.World;

            var particleSystem_Emission = firePoint.emission;
            particleSystem_Emission.rateOverTime = 0;

            firePointRenderer = firePoint.GetComponent<ParticleSystemRenderer>();
            firePointRenderer.renderMode=ParticleSystemRenderMode.Mesh;
            firePointRenderer.mesh =  Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            firePointRenderer.material =  new Material(Shader.Find("Particles/Standard Unlit"));
            firePointRenderer.alignment = ParticleSystemRenderSpace.Velocity;

            var particleSystem_Shape = firePoint.shape;
            particleSystem_Shape.enabled= coneOfAccuracyInternal>0 ? true:false;
            particleSystem_Shape.shapeType =  ParticleSystemShapeType.Cone;
            particleSystem_Shape.radius = 0;
            particleSystem_Shape.angle = coneOfAccuracyInternal/2;

            var particleSystem_Collision = firePoint.collision;
            particleSystem_Collision.enabled = true;
            particleSystem_Collision.bounce = 0f;
            particleSystem_Collision.maxKillSpeed = 0f;
            particleSystem_Collision.type = ParticleSystemCollisionType.World;
            particleSystem_Collision.mode = ParticleSystemCollisionMode.Collision3D;
            particleSystem_Collision.radiusScale = 0;
            particleSystem_Collision.sendCollisionMessages=true;
            particleSystem_Collision.collidesWith = CollidesWith;
    }
    public void InitializeUI(){
        if(autoGenerateUIElements){
              Canvas canvas;
              
            if(!GameObject.Find("AutoGunUI")){
                canvas = new GameObject("AutoGunUI").AddComponent<Canvas>();
                canvas.transform.position = Vector3.zero;
                canvas.transform.SetParent(Camera.main.transform);
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.pixelPerfect = true;
                canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }
                canvas = GameObject.Find("AutoGunUI").GetComponent<Canvas>();
                if(!GameObject.Find("GunUIPanel")){
                uiPanel = new GameObject("GunUIPanel").AddComponent<Image>();
                uiPanel.transform.SetParent(canvas.transform);
                uiPanel.rectTransform.anchorMax = new Vector2(1,0);
                uiPanel.rectTransform.anchorMin = new Vector2(1,0);
                uiPanel.rectTransform.anchoredPosition = new Vector2(-107, 30);
                uiPanel.rectTransform.sizeDelta = new Vector2(165,31);
                uiPanel.color = new Color32(58,58,58,85);

                uiReloadTimer = new GameObject("UIReloadTimer").AddComponent<Image>();
                uiReloadTimer.transform.SetParent(uiPanel.transform);
                uiReloadTimer.rectTransform.anchoredPosition = new Vector2(0,0);
                uiReloadTimer.rectTransform.sizeDelta = new Vector2(uiPanel.rectTransform.sizeDelta.x,31);
                uiReloadTimer.color = new Color(0,0.01f,0.04f,0.25f);
                
                Font defaultFont = uiFont ? uiFont : Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                
                currentMagUI = new GameObject("CurrentAmmoDisp").AddComponent<Text>();
                currentMagUI.transform.SetParent(uiPanel.transform);
                currentMagUI.font = defaultFont;
                currentMagUI.resizeTextForBestFit = true;
                currentMagUI.alignment = TextAnchor.MiddleCenter;
                currentMagUI.rectTransform.anchoredPosition = new Vector2(-51,0);
                currentMagUI.rectTransform.sizeDelta = new Vector2(60,29);
                currentMagUI.text = "0000";

                ammoReserveUI = new GameObject("ReserveAmmoDisp").AddComponent<Text>();
                ammoReserveUI.transform.SetParent(uiPanel.transform);
                ammoReserveUI.font = defaultFont;
                ammoReserveUI.resizeTextForBestFit = true;
                ammoReserveUI.resizeTextMinSize = 1;
                ammoReserveUI.alignment = TextAnchor.MiddleCenter;
                ammoReserveUI.rectTransform.anchoredPosition = new Vector2(-6,-3.8f);
                ammoReserveUI.rectTransform.sizeDelta = new Vector2(30,15);
                ammoReserveUI.text = "/0000";

                fireModeUI = new GameObject("CurrentFireModeDisp").AddComponent<Text>();
                fireModeUI.transform.SetParent(uiPanel.transform);
                fireModeUI.font = defaultFont;
                fireModeUI.resizeTextForBestFit = true;
                fireModeUI.alignment = TextAnchor.MiddleCenter;
                fireModeUI.rectTransform.anchoredPosition = new Vector2(45,0);
                fireModeUI.rectTransform.sizeDelta = new Vector2(49,15);
                fireModeUI.text = "[FireMode]";
                fireModeUI.color = new Color(255,234,0);

        

                uiPanel.enabled = false;
                currentMagUI.enabled = false;
                ammoReserveUI.enabled = false;
                fireModeUI.enabled = false;
                uiReloadTimer.enabled = false;
            }
                else{
                    uiPanel = GameObject.Find("GunUIPanel").GetComponent<Image>();
                    currentMagUI = GameObject.Find("CurrentAmmoDisp").GetComponent<Text>();
                    ammoReserveUI = GameObject.Find("ReserveAmmoDisp").GetComponent<Text>();
                    fireModeUI = GameObject.Find("CurrentFireModeDisp").GetComponent<Text>();
                    uiReloadTimer= GameObject.Find("UIReloadTimer").GetComponent<Image>();
                    
                }   
            }
            if(autoGenCrosshair){
                Canvas canvas;
                
                if(!GameObject.Find("AutoGunUI")){
                    canvas = new GameObject("AutoGunUI").AddComponent<Canvas>();
                    canvas.transform.position = Vector3.zero;
                    canvas.transform.SetParent(Camera.main.transform);
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.pixelPerfect = true;
                    canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                }
                canvas = GameObject.Find("AutoGunUI").GetComponent<Canvas>();                
                if(!GameObject.Find("AutoGunCrosshair")){
                    CH = new GameObject("AutoGunCrosshair").AddComponent<Image>();
                    CH.rectTransform.SetParent(canvas.transform);
                    CH.rectTransform.anchoredPosition = Vector2.zero;
                    CH.rectTransform.sizeDelta = new Vector2(25,25);
                    CH.color = Color.clear;
                    CH.enabled = false;
                }
                else{CH = GameObject.Find("AutoGunCrosshair").GetComponent<Image>();}
            }

    }       
    void OnDrawGizmosSelected(){
        if(drawSetupGizmos){
            Gizmos.matrix = transform.localToWorldMatrix;
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(0,gunADSCenterY,0),0.025f);

            #region ConeOfAccuracy/MuzzleEnd
            Gizmos.color = Color.magenta;
            if(muzzleEnd){Gizmos.DrawWireSphere(muzzleEnd.localPosition, 0.05f);}
            if(coneOfAccuracy>0&&muzzleEnd){
                
                Gizmos.DrawLine(muzzleEnd.transform.localPosition,muzzleEnd.transform.localPosition+new Vector3(0,coneOfAccuracy/2,25));
                Gizmos.DrawLine(muzzleEnd.transform.localPosition,muzzleEnd.transform.localPosition+new Vector3(0,-coneOfAccuracy/2,25));
                Gizmos.DrawLine(muzzleEnd.transform.localPosition,muzzleEnd.transform.localPosition+new Vector3(coneOfAccuracy/2,0,25));
                Gizmos.DrawLine(muzzleEnd.transform.localPosition,muzzleEnd.transform.localPosition+new Vector3(-coneOfAccuracy/2,0 ,25));
                Gizmos.DrawLine(muzzleEnd.transform.localPosition+new Vector3(-coneOfAccuracy/2,0 ,25) ,muzzleEnd.transform.localPosition+new Vector3(0,coneOfAccuracy/2,25));
                Gizmos.DrawLine(muzzleEnd.transform.localPosition+new Vector3(coneOfAccuracy/2,0,25) ,muzzleEnd.transform.localPosition+new Vector3(0,-coneOfAccuracy/2,25));
                Gizmos.DrawLine(muzzleEnd.transform.localPosition+new Vector3(0,coneOfAccuracy/2,25) ,muzzleEnd.transform.localPosition+new Vector3(coneOfAccuracy/2,0,25));
                Gizmos.DrawLine(muzzleEnd.transform.localPosition+new Vector3(0,-coneOfAccuracy/2,25) ,muzzleEnd.transform.localPosition+new Vector3(-coneOfAccuracy/2,0 ,25));
            }
            #endregion
            
            #region Recoil
            if(useRecoil){
                Gizmos.color = Color.red;
                Gizmos.DrawLine(Vector3.zero, new Vector3(recoilForceIntensity.x/10,0,0));
                Gizmos.DrawWireSphere(new Vector3(recoilForceIntensity.x/10,0,0),0.05f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(Vector3.zero, new Vector3(0,recoilForceIntensity.y/10,0));
                Gizmos.DrawWireSphere(new Vector3(0,recoilForceIntensity.y/10,0),0.05f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(Vector3.zero, new Vector3(0,0,recoilForceIntensity.z/10));
                Gizmos.DrawWireSphere( new Vector3(0,0,recoilForceIntensity.z/10 ),0.05f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(Vector3.zero, new Vector3(recoilForceIntensity.x/10,recoilForceIntensity.y/10,recoilForceIntensity.z/10));
                Gizmos.DrawWireSphere( new Vector3(recoilForceIntensity.x/10,recoilForceIntensity.y/10,recoilForceIntensity.z/10),0.05f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(Vector3.zero,recoilRotationMultiplier/5);
            }
            #endregion
        }
    }
}
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectileCannonWithError : MonoBehaviour
{

    public bool have2axis = false;
    [SerializeField] private bool cannonActive = true;
    [SerializeField] private GameObject canonHorizontal = null;
    [SerializeField] private GameObject projectile = null;
    [SerializeField] public Vector2 minMaxAngle = default;
    [SerializeField] private Transform cannonTurnBase = null;
    [SerializeField] private Transform cannonBarrel = null;
    [SerializeField] private GameObject cannonFireTrigger = null;
    [SerializeField] private ParticleSystem cannonSmoke = null;
    [SerializeField] private float fireVelocity = 10;
    [SerializeField] private bool arcFire = false;
    [SerializeField] private float _coolDown = 5f;

    [SerializeField] private Transform targetObject = null;
    [SerializeField] private float errorDistance = 0.0f;
    [SerializeField] private float maxDistance = 5000.0f;
    [SerializeField] private float minDistance = 0.0f;

    public Vector3 currentError = new Vector3(0, 0, 0);
    public float distanceToTarget = 0f;
    public bool debugAimTarget = false;
    public bool isInCoolDown = false;

    Vector3 GetTargetPosition()
    {
        return (targetObject.position + currentError);
    }

    void RandomError()
    {
        currentError.x = Random.Range(0.0f, errorDistance * 2.0f) - errorDistance;
        currentError.z = Random.Range(0.0f, errorDistance * 2.0f) - errorDistance;
    }

    IEnumerator Start()
    {
        /*
        PhotonNetworkController cont = GameObject.FindObjectOfType<PhotonNetworkController>();
        while(cont.ready == false)
            yield return null;

        if (MultiPlayer_Helper.InitiateMultiplayer(gameObject))
        {
            foreach (activationdetection act in gameObject.GetComponentsInChildren<activationdetection>())
                Destroy(act.gameObject);
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
                Destroy(col.gameObject);
            foreach (Rigidbody rig in gameObject.GetComponentsInChildren<Rigidbody>())
                Destroy(rig.gameObject);
            ProjectileCannonWithError script = gameObject.GetComponent<ProjectileCannonWithError>();
            Destroy(script);
        }
        */
        yield return null;
    }

    private void Awake()
    {
        targetObject = null;
        if (cannonFireTrigger != null)
        {
            CanonFireTrigger trg = cannonFireTrigger.GetComponent<CanonFireTrigger>();
            if (trg != null)
                trg.Init(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetObject == null)

            if (debugAimTarget)
            {
                Target[] targets = GameObject.FindObjectsOfType<Target>();
                targetObject = targets[0].transform;
                return;
            }
            else
            {
                boat_followdummy[] boats = gameflowmultiplayer.allBoats;
                if(boats != null && boats.Length > 0)
                    targetObject = boats[0].transform;
                return;
            }
        Debug.DrawRay(cannonBarrel.position, cannonBarrel.forward * fireVelocity, Color.white);
        Vector3 xComponenet = cannonBarrel.forward * fireVelocity;
        Vector3 yComponenet = cannonBarrel.forward * fireVelocity;
        xComponenet.y = 0;
        yComponenet.x = 0;
        yComponenet.z = 0;

        Debug.DrawRay(cannonBarrel.position, xComponenet, Color.red);
        Debug.DrawRay(cannonBarrel.position, yComponenet, Color.green);

        if (cannonActive)
        {
            //TurnCannonBase();
            TurnBarrel();
        }

    }

    public void TurnCannonBase()
    {
        if (cannonTurnBase == null)
            return;
        Vector3 baseTurnDir = GetTargetPosition() - cannonTurnBase.position;
        baseTurnDir.y = 0;
        cannonTurnBase.forward = Vector3.Lerp(cannonTurnBase.forward, baseTurnDir, Time.deltaTime / 20.0f);
    }

    public void ChangeFireMode()
    {
        arcFire = !arcFire;
    }

    private float z;
    public void TurnBarrel()
    {
        Vector3 H_levelPos = GetTargetPosition();
        H_levelPos.y = cannonBarrel.position.y;

        float Rx = Vector3.Distance(H_levelPos, cannonBarrel.position);
        distanceToTarget = Rx;

        float k = 0.5f * Physics.gravity.y * Rx * Rx * (1 / (fireVelocity * fireVelocity));

        float h = GetTargetPosition().y - cannonBarrel.position.y;

        float j = (Rx * Rx) - (4 * (k * (k - h)));

        if (j >= 0)
        {
            if (arcFire) z = (-Rx - Mathf.Sqrt(j)) / (2 * k);

            else z = (-Rx + Mathf.Sqrt(j)) / (2 * k);
        }
        cannonBarrel.localEulerAngles = new Vector3(Mathf.LerpAngle(cannonBarrel.localEulerAngles.x, Mathf.Clamp((Mathf.Atan(z) * 57.2958f), 0, 0) * -1, Time.deltaTime * 10), 0, 0);
    }

    public void FireCannon()
    {
        if ((distanceToTarget > minDistance) && (distanceToTarget < maxDistance))
            NetworkFire();
    }

    [PunRPC]
    public void NetworkFire()
    {
        if (isInCoolDown)
            return;
        isInCoolDown = true;
        cannonSmoke.Play();
        GameObject fireObj = PhotonNetworkController.InstantiateSoloOrMulti(projectile.name, cannonBarrel.position, cannonBarrel.rotation);
        Rigidbody rb = fireObj.GetComponent<Rigidbody>();
        rb.velocity = cannonBarrel.forward * fireVelocity;
        Destroy(fireObj, 10);
        StartCoroutine("_WaitToChangeAngle");
        StartCoroutine("CoolDown");
    }

    IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(_coolDown);
        isInCoolDown = false;
    }

    IEnumerator _WaitToChangeAngle()
    {
        yield return new WaitForSeconds(1.0f);
        RandomError();
    }
}

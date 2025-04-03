using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarshipShooting : MonoBehaviour
{
    public GameObject self;
    public GameObject humanBullet;
    public GameObject kaireyhsBullet;

    public Transform humanBulletLocationA;
    public Transform humanBulletLocationB;
    public Transform kaireyhsBulletLocationA;
    public Transform kaireyhsBulletLocationB;

    public Vector3 humanBulletAngleDirection;
    public Vector3 kaireyhsBulletAngleDirection;

    public Vector3 locationToReach;

    public float humanShootingDelay;
    public float kaireyhsShootingDelay;

    private bool humanCanShoot = true;
    private bool kaireyhsCanShoot = true;
    private bool targetReach = false;

    private float timer = 0;

    public void Update()
    {
        if (!targetReach)
        {
            Vector3 diff = transform.localPosition - locationToReach;
            float sqrDist = diff.sqrMagnitude;
            targetReach = sqrDist < 0.01f;
        }

        if (targetReach)
        {
            if (humanCanShoot)
            {
                StartCoroutine(HumanShotPace());
            }

            if (kaireyhsCanShoot)
            {
                StartCoroutine(KaireyhsShotPace());
            }
        }
    }

    private void HumanShipShooting()
    {
        Instantiate(humanBullet, new Vector3(humanBulletLocationA.position.x, humanBulletLocationA.position.y, humanBulletLocationA.position.z), Quaternion.Euler(humanBulletAngleDirection), humanBulletLocationA);
        Instantiate(humanBullet, new Vector3(humanBulletLocationB.position.x, humanBulletLocationB.position.y, humanBulletLocationB.position.z), Quaternion.Euler(humanBulletAngleDirection), humanBulletLocationB);
    }

    private void KaireyhsShipShooting()
    {
        Instantiate(kaireyhsBullet, new Vector3(kaireyhsBulletLocationA.position.x, kaireyhsBulletLocationA.position.y, kaireyhsBulletLocationA.position.z), Quaternion.Euler(kaireyhsBulletAngleDirection), kaireyhsBulletLocationA);
        Instantiate(kaireyhsBullet, new Vector3(kaireyhsBulletLocationB.position.x, kaireyhsBulletLocationB.position.y, kaireyhsBulletLocationB.position.z), Quaternion.Euler(kaireyhsBulletAngleDirection), kaireyhsBulletLocationB);
    }

    IEnumerator KaireyhsShotPace()
    {
        kaireyhsCanShoot = false;
        KaireyhsShipShooting();
        yield return new WaitForSeconds(kaireyhsShootingDelay);
        kaireyhsCanShoot = true;
    }

    IEnumerator HumanShotPace()
    {
        humanCanShoot = false;
        HumanShipShooting();
        yield return new WaitForSeconds(humanShootingDelay);
        humanCanShoot = true;
    }
}

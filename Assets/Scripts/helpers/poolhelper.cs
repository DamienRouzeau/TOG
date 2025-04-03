using System.Collections;
using UnityEngine;

public class poolhelper : MonoBehaviour
{
    public static poolhelper myself;

    public enum ProjectileSize
    {
        small,
        medium,
        large,
        custom,
        none
    }

    public string endRaceWonBulletPoolName => _endRaceWonBulletPool?.name;
    public string endRaceLooseBulletPoolName => _endRaceLooseBulletPool?.name;
    public string sinkingBulletPoolName => _sinkingBulletPool?.name;
    public string cantShootBulletPoolName => _cantShootBulletPool?.name;
    public string fallbackPlayerBulletPoolName => _fallbackPlayerBulletPool?.name;

    [SerializeField]
    private GameObject[] _playerBulletPools = null;
    [SerializeField]
    private GameObject[] _boatBulletPools = null;
    [SerializeField]
    private GameObject _endRaceWonBulletPool = null;
    [SerializeField]
    private GameObject _endRaceLooseBulletPool = null;
    [SerializeField]
    private GameObject _sinkingBulletPool = null;
    [SerializeField]
    private GameObject _cantShootBulletPool = null;
    [SerializeField]
    private GameObject _fallbackPlayerBulletPool = null;

    private void Awake()
    {
        myself = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsPlayerBulletPool(string name)
	{
        if (_playerBulletPools != null)
		{
            foreach (GameObject go in _playerBulletPools)
			{
                if (go.name == name)
                    return true;
			}
		}
        return false;
	}

    public bool IsBoatBulletPool(string name)
    {
        if (_boatBulletPools != null)
        {
            foreach (GameObject go in _boatBulletPools)
            {
                if (go.name == name)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// IMPACT OBJECT POOL
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="position"></param>
    /// <param name="materialobj"></param>
    /// <param name="size"></param>
    public void CreateImpact(GameObject obj,Vector3 position,GameObject materialobj,poolhelper.ProjectileSize size = poolhelper.ProjectileSize.small, string customSize=null)
    {
//        Debug.Log("Create Impact:"+obj.name+" WITH "+materialobj.name+ " " +size);
        if (size == poolhelper.ProjectileSize.none) return;

        string particulename = "woodimpact_pool";
        impactmaterial_tag tg = materialobj.GetComponentInChildren<impactmaterial_tag>();
        if (tg) particulename = tg.particle_type_name;
        GameObject poolfather = gameObject.FindInChildren(particulename);
        if (poolfather == null)
        {
            Debug.LogError($"No {particulename} pool found!");
            return;
        }
        GameObject poolchild = null;
        poolhelper.ProjectileSize oldsize = size;

        if (size == ProjectileSize.custom)
		{
            poolchild = poolfather.FindInChildren(customSize);
            if (poolchild == null)
            {
                Debug.LogError($"No {customSize} custom size found!");
                size = poolhelper.ProjectileSize.small;
            }
        }
        if (size == poolhelper.ProjectileSize.large)
        {
            poolchild = poolfather.FindInChildren("large");
            if (poolchild == null) poolchild = poolfather.FindInChildren("Large");
            if (poolchild == null) size = poolhelper.ProjectileSize.medium;
        }
        if (size == poolhelper.ProjectileSize.medium)
        {
            poolchild = poolfather.FindInChildren("medium");
            if (poolchild == null) poolchild = poolfather.FindInChildren("Medium");
            if (poolchild == null) size = poolhelper.ProjectileSize.small;
        }
        if (size == poolhelper.ProjectileSize.small)
        {
            poolchild = poolfather.FindInChildren("small");
            if (poolchild == null) poolchild = poolfather.FindInChildren("Small");
        }
        if (poolchild == null)
        {
            size = oldsize;
            if (size == poolhelper.ProjectileSize.small)
            {
                poolchild = poolfather.FindInChildren("small");
                if (poolchild == null) poolchild = poolfather.FindInChildren("Small");
                if (poolchild == null) size = poolhelper.ProjectileSize.medium;
            }
            if (size == poolhelper.ProjectileSize.medium)
            {
                poolchild = poolfather.FindInChildren("medium");
                if (poolchild == null) poolchild = poolfather.FindInChildren("Medium");
                if (poolchild == null) size = poolhelper.ProjectileSize.large;
            }
            if (size == poolhelper.ProjectileSize.large)
            {
                poolchild = poolfather.FindInChildren("large");
                if (poolchild == null) poolchild = poolfather.FindInChildren("Large");
            }
        }

        if (poolchild == null) poolchild = poolfather;
//        Debug.Log("POOLFATHER:" + poolchild.name);
        poolmaster pm = poolchild.GetComponent<poolmaster>();
        if (pm)
        {
            foreach (impactparticles_tag ip in pm.myimpacts)
            {
                if (!ip.gameObject.activeInHierarchy)
                {
                    ip.gameObject.transform.position = position;
                    ip.gameObject.transform.LookAt(obj.transform);
                    StartCoroutine(SpawnImpact(ip.gameObject));
                    return;
                }
            }
        }
        else
        {
            foreach (impactparticles_tag ip in poolchild.GetComponentsInChildren<impactparticles_tag>(true))
            {
                if (!ip.gameObject.activeInHierarchy)
                {
                    ip.gameObject.transform.position = position;
                    ip.gameObject.transform.LookAt(obj.transform);
                    StartCoroutine(SpawnImpact(ip.gameObject));
                    return;
                }
            }
        }
    }

    IEnumerator SpawnImpact(GameObject obj)
    {
//        Debug.Log("Spawn Impact:" + obj.name);
        ParticleSystem ps = obj.GetComponentInChildren<ParticleSystem>(true);
        ps.gameObject.SetActive(true);
        ps.Stop();
        ps.Clear();
        ps.Play();
        obj.SetActive(true);
        while (ps.isPlaying)
            yield return null;
        ps.Stop();
        ps.Clear();
        obj.SetActive(false);
    }

    /// <summary>
    /// Global Get PoolItem
    /// </summary>
    /// <param name="particulename"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="objfather"></param>
    /// <returns></returns>
    public GameObject GetNextPoolItem(string particulename,Vector3 pos,Quaternion rot,GameObject objfather)
    {
//        Debug.Log("REQUEST:" + particulename);
        GameObject poolfather = gameObject.FindInChildren(particulename);
        // BULLETS ?????
        bullet_tag[] bullets = null;
        poolmaster pm = poolfather.GetComponent<poolmaster>();
        if (pm != null)
            bullets = pm.mybullets;
        else
            bullets = poolfather.GetComponentsInChildren<bullet_tag>(true);

        if (bullets != null)
        {
            foreach (bullet_tag ip in bullets)
            {
                if (ip != null && ip.myObject != null)
                {
                    if (!ip.myObject.activeInHierarchy)
                    {
                        Projectile pr = ip.myProjectile;
                        if (!pr.invisiblealive)
                        {
                            pr.Initialize();
                            ip.gameObject.transform.position = pos;
                            ip.gameObject.transform.rotation = rot;
                            ip.myObject.SetActive(true);
                            //pr.SetOldCoord(pos);

                            Player pl = objfather.GetComponentInParent<Player>();
                            if (pl != null)
                            {
                                if (pr.tailfocus != null)
                                {
                                    pr.tailfocus.transform.SetParent(pl.gameObject.transform);
                                    pr.tailfocus.transform.position = pos;
                                }
                            }

                            pr.Restart();
                            return ip.gameObject;
                        }
                    }
                }
            }
        }

        // COLLECTABKLES ?????
        foreach (collector_tag ip in poolfather.GetComponentsInChildren<collector_tag>(true))
        {
            if (!ip.gameObject.activeInHierarchy)
            {
                ip.gameObject.transform.position = pos;
                ip.gameObject.transform.rotation = rot;
                ip.gameObject.SetActive(true);
                return ip.gameObject;
            }
        }

        return null;
    }
}

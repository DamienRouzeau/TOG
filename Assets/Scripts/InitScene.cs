//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

// TODO : REMOVE THIS CLASS
public class InitScene { }
//public class InitScene : NetworkBehaviour
//{
//    public List<GameObject> gunSpawnPositions = new List<GameObject>();
//    public GameObject gun;

//    private bool spawned = false;

//    void Start()
//    {

//    }

//    public override void OnStartServer()
//    {
//        base.OnStartServer();
//        foreach (GameObject g in gunSpawnPositions)
//        {
//            GameObject newGun = Instantiate(gun, g.transform.position, Quaternion.Euler(0, 0, -90));
//            NetworkServer.Spawn(newGun);
//        }
//    }

//    void Update()
//    {
//    }
//}

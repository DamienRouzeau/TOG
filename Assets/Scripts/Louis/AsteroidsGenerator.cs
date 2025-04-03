using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidsGenerator : MonoBehaviour
{
    //Tableau regroupant tous les astéroïdes pouvant être instanciés
    public GameObject[] asteroids;
    //Tableau regroupant tous les spawn points où les astéroïdes peuvent être instanciés
    public Transform[] asteroidsSpawnPoints;
    private bool asteroidCanBeInstantiate;
    public float instantiationDelay;

    public void Update()
    {
        if (asteroidCanBeInstantiate)
        {
            StartCoroutine(InstantiateAsteroid());
        }
    }

    void LoadNewAsteroid()
    {
        //On tire au sort l'un des asteroid à instancier
        int asteroidIndex = Random.Range(0, asteroids.Length);
        GameObject selectedAsteroid = asteroids[asteroidIndex];
        //On tire au sort l'un des spawn points d'astéroïde
        int transformIndex = Random.Range(0, asteroidsSpawnPoints.Length);
        Transform selectedTransform = asteroidsSpawnPoints[transformIndex];
        //On instancie l'astéroïde tiré au sort au transform de spawn point tiré au sort
        Instantiate(selectedAsteroid, selectedTransform);
    }

    public IEnumerator InstantiateAsteroid()
    {
        asteroidCanBeInstantiate = false;
        LoadNewAsteroid();
        yield return new WaitForSeconds(instantiationDelay);
        asteroidCanBeInstantiate = true;
    }
}

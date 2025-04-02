using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TOG_BatterieHitMark : MonoBehaviour
{
    [SerializeField] private TOG_BatterieBehaviour batterie;
    [SerializeField] private int column;
    [SerializeField] private float moveSpeed;
    private float spawnDistance;
    private SplineController spline;
    public bool isPerfect = false, isNormal = false;

    private void Start()
    {
        if (transform.parent != null)
        {
            spawnDistance = transform.parent.position.z;
        }
    }


    public void SetColumn(int newcolumn, TOG_BatterieBehaviour _batterie)
    {
        column = newcolumn;
        batterie = _batterie;
        isNormal = false;
    }

    private void Update()
    {
        transform.localPosition += new Vector3(0, 0, 1 * moveSpeed);
    }

    public void Hit()
    {
        moveSpeed = 0;
        if (isPerfect)
        {
            Debug.Log("PERFECT " + gameObject.name);
            batterie.PerfectHit(column);
            batterie.RemoveMark(this, column);
            //Destroy(gameObject);
        }
        else if (isNormal)
        {
            Debug.Log("NORMAL " + gameObject.name);
            batterie.Hit(column);
            batterie.RemoveMark(this, column);
            //Destroy(gameObject);
        }
        else
        {
            Debug.Log("TOO SOON " + gameObject.name);
            batterie.MissHit(column);
            batterie.RemoveMark(this, column);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NormalArea"))
        {
            batterie.MissHit(column);
            batterie.RemoveMark(this, column);
            isNormal = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PerfectArea"))
        {
            isPerfect = true;
        }
        if (other.CompareTag("NormalArea"))
        {
            isNormal = true;
        }
    }

    public float GetDistance() { return spawnDistance; }
}

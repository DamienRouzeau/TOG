using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TOG_BatterieHitArea : MonoBehaviour
{
    public Transform markSpawnPoint;
    [SerializeField] private MeshRenderer mesh;
    private Queue<TOG_BatterieHitMark> markInThisColumn = new Queue<TOG_BatterieHitMark>(); // Queue
    [SerializeField] private List<TOG_BatterieHitMark> list = new List<TOG_BatterieHitMark>();
    [SerializeField] private GameObject perfectHit, okHit, failHit, idleHit;
    public int column;
    private Coroutine resetHitArea;
    [SerializeField] private TOG_BatterieBehaviour batterie;
    [SerializeField] private AudioSource hitSound, missSound;


    private void Start()
    {
        okHit.SetActive(false);
        failHit.SetActive(false);
        idleHit.SetActive(true);
        perfectHit.SetActive(false);
    }


    public void AddMarkToQueue(TOG_BatterieHitMark mark)
    {
        markInThisColumn.Enqueue(mark);
        list.Clear();
        foreach (TOG_BatterieHitMark _mark in markInThisColumn)
        {
            list.Add(_mark);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (resetHitArea != null) return;
        if (other.CompareTag("Weapon"))
        {
            if (other.ClosestPoint(transform.position).y > other.transform.position.y)
            {
                if (markInThisColumn.Count > 0)
                {
                    TOG_BatterieHitMark closestMark = list[0];
                    float dist = Vector3.Distance(closestMark.transform.position, transform.position);
                    foreach (TOG_BatterieHitMark mark in list)
                    {
                        if(Vector3.Distance(mark.transform.position, transform.position) < dist)
                        {
                            closestMark = mark;
                        }
                    }
                    resetHitArea = StartCoroutine(HitDelay());
                    closestMark.Hit();
                    list.Remove(closestMark);

                    /*resetHitArea = StartCoroutine(HitDelay());
                    markInThisColumn.Peek().Hit();
                    markInThisColumn.Dequeue();*/
                }
            }
        }
    }

    private IEnumerator HitDelay()
    {
        yield return new WaitForSeconds(0.5f);
        resetHitArea = null;
    }

    public void OnPerfect()
    {
        okHit.SetActive(false);
        failHit.SetActive(false);
        idleHit.SetActive(false);
        perfectHit.SetActive(true);
        hitSound.Play();
        list.Clear();
        foreach (TOG_BatterieHitMark _mark in markInThisColumn)
        {
            list.Add(_mark);
        }
        StartCoroutine(ResetColor());
    }

    public void OnMedium()
    {
        okHit.SetActive(true);
        failHit.SetActive(false);
        idleHit.SetActive(false);
        perfectHit.SetActive(false);
        hitSound.Play();
        list.Clear();
        foreach (TOG_BatterieHitMark _mark in markInThisColumn)
        {
            list.Add(_mark);
        }
        StartCoroutine(ResetColor());
    }

    public void OnSoon()
    {
        okHit.SetActive(false);
        failHit.SetActive(true);
        idleHit.SetActive(false);
        perfectHit.SetActive(false);
        missSound.Play();
        StartCoroutine(ResetColor());
    }

    public void RemoveMark(TOG_BatterieHitMark mark)
    {
        if (markInThisColumn.Count > 0)
            markInThisColumn.Dequeue();
        list.Clear();
        foreach (TOG_BatterieHitMark _mark in markInThisColumn)
        {
            list.Add(_mark);
        }

        //Destroy(mark.gameObject);
    }

    private IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(0.4f);
        okHit.SetActive(false);
        failHit.SetActive(false);
        idleHit.SetActive(true);
        perfectHit.SetActive(false);

    }
}

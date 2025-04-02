using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TOG_BatterieBehaviour : MonoBehaviour
{
    public bool enableDebug;
    [SerializeField] private List<TOG_BatterieHitArea> hitArea = new List<TOG_BatterieHitArea>();
    [SerializeField] private TOG_BatterieHitMark mark_A_Prefab;
    [SerializeField] private TOG_BatterieHitMark mark_B_Prefab;
    [SerializeField] private TOG_BatterieHitMark mark_C_Prefab;
    [SerializeField] private TOG_BatterieHitMark mark_D_Prefab;

    [SerializeField] private float markSpawnSpeed;
    [SerializeField] private float minSpawnSpeed, maxSpawnSpeed;
    //[SerializeField] private List<TOG_BatterieHitMark> mark_A_Column;
    [SerializeField] private List<TOG_BatterieHitMark> mark_B_Column;
    [SerializeField] private List<TOG_BatterieHitMark> mark_C_Column;
    [SerializeField] private List<TOG_BatterieHitMark> mark_D_Column;

    private Coroutine spawnCoroutine;
    [SerializeField] private float chanceOfDoubleMark;
    
    [Header("Gauge")]
    private float gaugeEnergy;
    [SerializeField] private float energyAddedPerPerfectHit;
    [SerializeField] private float energyAddedPerHit;
    [SerializeField] private float maxEnergy;
    [SerializeField] private float stepEnergy;
    [SerializeField] private Slider slider, sliderBack;

    private void Start()
    {
        spawnCoroutine = StartCoroutine(StartSpawning());
        gaugeEnergy = 0;
        UpdateGaugeValue();
    }

    private IEnumerator StartSpawning()
    {
        yield return new WaitForSeconds(5);
        spawnCoroutine = StartCoroutine(SpawnMark());
    }

    private IEnumerator SpawnMark()
    {
        markSpawnSpeed = Random.Range(minSpawnSpeed, maxSpawnSpeed);
        int column = Random.Range(0, hitArea.Count);
        PoolMark(column);
        float chanceDouble = Random.Range(0, 100);
        if(chanceDouble <= chanceOfDoubleMark)
        {
            int column2 = column;
            int i = 0;
            while(column == column2 || i<10)
            {
                column2 = Random.Range(0, hitArea.Count);
                i++;
            }
            if(column != column2)
                PoolMark(column2);
        }
        yield return new WaitForSeconds(markSpawnSpeed);
        spawnCoroutine = null;
    }

    private void Update()
    {
        if(spawnCoroutine == null) { spawnCoroutine = StartCoroutine(SpawnMark()); }
    }

    private TOG_BatterieHitMark PoolMark(int column)
    {
        TOG_BatterieHitMark _mark;

        switch (column)
        {
            case 0:
                _mark = Instantiate(mark_A_Prefab, hitArea[column].markSpawnPoint);
                hitArea[0].AddMarkToQueue(_mark);

                /*        hitArea[column].AddMarkToQueue(_mark);

                                if (mark_A_Column.Count > 0)
                                {
                                    _mark = mark_A_Column[0];
                                    mark_A_Column.Remove(_mark);
                                    _mark.transform.position = hitArea[column].markSpawnPoint.position;
                                    _mark.transform.rotation = hitArea[column].markSpawnPoint.rotation;
                                    _mark.transform.parent = hitArea[column].markSpawnPoint;
                                    _mark.gameObject.SetActive(true);
                                    _mark.isPerfect = false;
                                    _mark.isNormal = false;
                                }
                                else
                                {
                                    _mark = Instantiate(mark_A_Prefab, hitArea[column].markSpawnPoint);
                                }*/
                break;


            case 1:
                _mark = Instantiate(mark_B_Prefab, hitArea[column].markSpawnPoint);
                hitArea[1].AddMarkToQueue(_mark);

                /*                hitArea[0].AddMarkToQueue(_mark);

                                if (mark_B_Column.Count > 0)
                                {
                                    _mark = mark_B_Column[0];
                                    mark_B_Column.Remove(_mark);
                                    _mark.transform.position = hitArea[column].markSpawnPoint.position;
                                    _mark.transform.rotation = hitArea[column].markSpawnPoint.rotation;
                                    _mark.transform.parent = hitArea[column].markSpawnPoint;
                                    _mark.gameObject.SetActive(true);
                                    _mark.isPerfect = false;
                                    _mark.isNormal = false;
                                }
                                else
                                {
                                    _mark = Instantiate(mark_B_Prefab, hitArea[column].markSpawnPoint);
                                }*/
                break;


            case 2:
                _mark = Instantiate(mark_C_Prefab, hitArea[column].markSpawnPoint);
                hitArea[2].AddMarkToQueue(_mark);

                /*
                                if (mark_C_Column.Count > 0)
                                {
                                    _mark = mark_C_Column[0];
                                    mark_C_Column.Remove(_mark);
                                    _mark.transform.position = hitArea[column].markSpawnPoint.position;
                                    _mark.transform.rotation = hitArea[column].markSpawnPoint.rotation;
                                    _mark.transform.parent = hitArea[column].markSpawnPoint;
                                    _mark.gameObject.SetActive(true);
                                    _mark.isPerfect = false;
                                    _mark.isNormal = false;
                                }
                                else
                                {
                                    _mark = Instantiate(mark_C_Prefab, hitArea[column].markSpawnPoint);
                                }*/
                break;


            case 3:
                _mark = Instantiate(mark_D_Prefab, hitArea[column].markSpawnPoint);
                hitArea[3].AddMarkToQueue(_mark);

                /*if (mark_D_Column.Count > 0)
                {
                    _mark = mark_D_Column[0];
                    mark_D_Column.Remove(_mark);
                    _mark.transform.position = hitArea[column].markSpawnPoint.position;
                    _mark.transform.rotation = hitArea[column].markSpawnPoint.rotation;
                    _mark.transform.parent = hitArea[column].markSpawnPoint;
                    _mark.gameObject.SetActive(true);
                    _mark.isPerfect = false;
                    _mark.isNormal = false;
                }
                else
                {
                    _mark = Instantiate(mark_D_Prefab, hitArea[column].markSpawnPoint);
                }*/
                break;


            default:
                return null;
        }        
        _mark.SetColumn(column, this);
        return _mark;
    }


    public void MissHit(int column)
    {
        //don't apply speed boost to the boat for a mark hitted very to soon
        ResetGauge();
        hitArea[column].OnSoon();
        UpdateGaugeValue();
    }

    public void Hit(int column)
    {
        //apply a medium speed boost to the boat for a mark hitted a bit to soon
        gaugeEnergy += energyAddedPerHit;
        //boatFollow.SetSpeedBoost(mediumSpeedBoost);
        hitArea[column].OnMedium();
        UpdateGaugeValue();
    }

    public void PerfectHit(int column)
    {
        //apply a big speed boost to the boat for a mark hitted perfectly
        gaugeEnergy += energyAddedPerPerfectHit;
        //boatFollow.SetSpeedBoost(bigSpeedBoost);
        hitArea[column].OnPerfect();
        UpdateGaugeValue();
    }

    private void ResetGauge()
    {
        if(gaugeEnergy<stepEnergy)
        {
            gaugeEnergy = 0;
        }
        else if(gaugeEnergy<maxEnergy)
        {
            gaugeEnergy = stepEnergy;
        }
        else
        {
            gaugeEnergy = maxEnergy;
        }
        UpdateGaugeValue();
    }

    public bool UseEnergy()
    {
        if(gaugeEnergy >= stepEnergy)
        {
            gaugeEnergy -= stepEnergy;
            UpdateGaugeValue();
            return true;
        }
        return false;
    }

    public void RemoveMark(TOG_BatterieHitMark mark, int column)
    {
        mark.transform.parent = null;
        switch(column)
        {
            case 0:
                //mark_A_Column.Add(mark);
                hitArea[column].RemoveMark(mark);
                break;


            case 1:
                //mark_B_Column.Add(mark);
                hitArea[column].RemoveMark(mark);
                break;


            case 2:
                //mark_C_Column.Add(mark);
                hitArea[column].RemoveMark(mark);
                break;


            case 3:
                //mark_D_Column.Add(mark);
                hitArea[column].RemoveMark(mark);

                break;


            default:
                Destroy(mark.gameObject);
                return;
        }
        Destroy(mark.gameObject);
        //mark.gameObject.SetActive(false);
    }

    private void UpdateGaugeValue()
    {
        slider.value = gaugeEnergy;
        sliderBack.value = gaugeEnergy;
    }



    public TOG_BatterieHitArea GetHitArea(int column) { return hitArea[column]; }
}

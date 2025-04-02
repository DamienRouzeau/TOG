using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeObjectiveMiniMapBOD : MonoBehaviour
{
    public GameObject GuideA1;
    public GameObject GuideA2;
    public GameObject GuideB1;
    public GameObject GuideB2;
    public GameObject GuideC;
    public GameObject GuideAnomalie;

    public void ArchiveA2()
    {
        GuideA1.SetActive(false);
        GuideA2.SetActive(true);
    }

    public void ArchiveB1()
    {
        GuideA2.SetActive(false);
        GuideB1.SetActive(true);
    }

    public void ArchiveB2()
    {
        GuideB1.SetActive(false);
        GuideB2.SetActive(true);
    }

    public void ArchiveC()
    {
        GuideB2.SetActive(false);
        GuideC.SetActive(true);
    }

    public void ArchiveAnomalie()
    {
        GuideC.SetActive(false);
        GuideAnomalie.SetActive(true);
    }
}

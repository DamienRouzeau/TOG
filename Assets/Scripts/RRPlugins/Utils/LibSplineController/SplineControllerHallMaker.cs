using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class SplineControllerHallMaker : MonoBehaviour
{
    [SerializeField]
    SplineController original = null;
    [SerializeField]
    PathHall pathHall = null;
    [HideInInspector]
    [SerializeField]
    public float fVariance = 0.1f;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MakeHall()
    {
        int nOriginalId = fVariance < 0 ? 1 : 0;
        if (pathHall != null)
        {
            int nCurrentOriginalId = pathHall.GetSplineId(original);
            if( nCurrentOriginalId==nOriginalId )
            {
                pathHall.RemoveSpline(1 - nOriginalId);
            }
            else
            {
                pathHall.RemoveSpline( nOriginalId);
                pathHall.SetSpline(nOriginalId, original);
            }
        }
        original.Init();

        string sHallName = "Hall" + (nOriginalId == 0 ? "Right" : "Left");
        GameObject hallLeft = new GameObject(sHallName);
        hallLeft.transform.SetParent(transform);
        Transform[] originals = original.transforms;
        Transform[] hallTransforms = new Transform[originals.Length];
        for (int i = 0; i < hallTransforms.Length; i++)
        {
            GameObject obj = new GameObject("left_" + i);
            obj.transform.parent = hallLeft.transform;
            hallTransforms[i] = obj.transform;
        }


        // first
        Vector3 vDir = originals[1].position - originals[0].position;
        Vector3 vTangent = new Vector3(-vDir.z, vDir.y, vDir.x);
        hallTransforms[0].position = originals[0].position - vTangent * fVariance;

        // middle
        for (int i = 1; i < hallTransforms.Length - 1; i++)
        {
            vDir = originals[i + 1].position - originals[i - 1].position;
            vTangent = new Vector3(-vDir.z, vDir.y, vDir.x);
            hallTransforms[i].position = originals[i].position - vTangent * fVariance;
        }

        // last 
        vDir = originals[originals.Length - 1].position - originals[originals.Length - 2].position;
        vTangent = new Vector3(-vDir.z, vDir.y, vDir.x);
        hallTransforms[originals.Length - 1].position = originals[originals.Length - 1].position - vTangent * fVariance;

        hallLeft.AddComponent<SplineInterpolator>();
        SplineController splineController = hallLeft.AddComponent<SplineController>();
        splineController.SplineRoot = hallLeft;
        splineController.AutoClose = original.AutoClose;
        splineController.ComputeNodeTimeWithDistance = original.ComputeNodeTimeWithDistance;
        splineController.Duration = 1f;
        splineController.HideOnExecute = false;
        splineController.Init();

        if (pathHall != null)
        {
            pathHall.SetSpline(1 - nOriginalId, splineController);
        }
    }

    public void EraseHall()
    {
        int nOriginalId = fVariance < 0 ? 1 : 0;
        if (pathHall != null)
        {
            pathHall.RemoveSpline(1 - nOriginalId);
            pathHall.SetSpline(nOriginalId, original);
        }
    }
}

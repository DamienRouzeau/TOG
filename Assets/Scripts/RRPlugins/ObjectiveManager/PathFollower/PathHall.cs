using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathHall : MonoBehaviour
{
    public static float averagePathLength => allpath_average;
    private static float allpath_average = 0.0f;

    [SerializeField]
    private SplineController[] m_splinePath = null;

    // how long we move in world space for an increment of 1
    public float m_fSplineWorldUnitForOneIncrement = 0f;

    public void Init()
    {
        for (int i = 0; i < m_splinePath.Length; i++)
        {
            m_splinePath[i].Init();
        }
        ComputeIncrementForWorlUnit();
    }

    public bool IsLoopPath()
    {
        bool bLoop = true;
        int index = 0;
        while( bLoop && index < m_splinePath.Length )
        {
            bLoop = m_splinePath[index].AutoClose;
            if( bLoop )
            {
                index++;
            }
        }
        return bLoop;
    }

    public static void InitAllPathAverage()
	{
        if (allpath_average == 0.0f)
        {
            PathHall[] allph = GameObject.FindObjectsOfType<PathHall>();
            foreach (PathHall ph in allph)
            {
                ph.Init();
                allpath_average += ph.m_fSplineWorldUnitForOneIncrement;
            }
            allpath_average = allpath_average / allph.Length;     // average length
        }
    }

    public float ComputeIncrementFromWorlUnit(float worldUnit)
    {
        if (gameflowmultiplayer.gameMode == gameflowmultiplayer.GameMode.Endless)
        {
            return worldUnit / m_fSplineWorldUnitForOneIncrement;
        }        
        // speed must always be the same
        float factor = 1f; //m_fSplineWorldUnitForOneIncrement / allpath_average;
        return worldUnit * factor / allpath_average;
    }

    public Vector3 GetPositionAtTime( float fTime, float fLateralVariance )
    {
        Vector3 vPos0 = m_splinePath[0].GetPositionAtTime(fTime);
        if (fLateralVariance == 0f || m_splinePath.Length == 1)
            return vPos0;
        Vector3 vPos1 = m_splinePath[1].GetPositionAtTime(fTime);
        return Vector3.Lerp(vPos0, vPos1, fLateralVariance);
    }

    public Quaternion GetRotationAtTime(float fTime, float fLateralVariance)
    {
        Quaternion qRot0 = m_splinePath[0].GetRotationAtTime(fTime);
        if (fLateralVariance == 0f || m_splinePath.Length == 1)
            return qRot0;
        Quaternion qRot1 = m_splinePath[1].GetRotationAtTime(fTime);
        return Quaternion.Lerp(qRot0, qRot1, fLateralVariance);
    }

    public float GetLoopTime(float fLateralVariance)
    {
        float loopTime = m_splinePath[0].loopValue;
        if (fLateralVariance == 0f || m_splinePath.Length == 1)
            return loopTime;
        float loopTime1 = m_splinePath[1].loopValue;
        return Mathf.Lerp(loopTime, loopTime1, fLateralVariance);
    }

    public void SetSpline( int nSplineId, SplineController spline )
    {
        if( m_splinePath.Length > nSplineId )
        {
            m_splinePath[nSplineId] = spline;
        }
    }

    public SplineController GetSpline(int nSplineId)
    {
        if (m_splinePath.Length > nSplineId)
        {
            return m_splinePath[nSplineId];
        }
        return null;
    }

    public int GetSplineId( SplineController spline )
    {
        int index = 0;
        bool bFound = false;
        while( !bFound && index <m_splinePath.Length )
        {
            if( m_splinePath[index] == spline )
            {
                bFound = true;
            }
            else
            {
                index++;
            }
        }
        return bFound ? index : -1;
    }

    public void RemoveSpline(int nSplineId )
    {
        if (m_splinePath.Length > nSplineId && m_splinePath[nSplineId]!=null )
        {
            GameObject.DestroyImmediate(m_splinePath[nSplineId].gameObject);
            m_splinePath[nSplineId] = null;
        }
    }

    private void ComputeIncrementForWorlUnit()
    {
        m_fSplineWorldUnitForOneIncrement = 0;
        for (int i = 0; i < m_splinePath.Length; i++)
        {
            m_fSplineWorldUnitForOneIncrement += m_splinePath[i].SplineMagnitude / m_splinePath[i].Duration;
        }

        m_fSplineWorldUnitForOneIncrement = m_fSplineWorldUnitForOneIncrement / m_splinePath.Length;
    }
}

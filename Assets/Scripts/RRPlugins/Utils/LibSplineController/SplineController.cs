using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum eOrientationMode { NODE = 0, TANGENT }

[AddComponentMenu("Splines/Spline Controller")]
[RequireComponent(typeof(SplineInterpolator))]
public class SplineController : MonoBehaviour
{
    public Transform[] transforms { get { return mTransforms; } }


    public GameObject SplineRoot;
	public float Duration = 10;
	public eOrientationMode OrientationMode = eOrientationMode.NODE;
	public eWrapMode WrapMode = eWrapMode.ONCE;
	public float loopValue;
	public bool AutoStart = true;
	public bool AutoClose = true;
	public bool ForceDirectionWithNode = true;
	public bool HideOnExecute = true;
	public bool DisplayOnDrawGizmos = true;
	public bool UseLocalPosition = true;
	public bool ComputeNodeTimeWithDistance = false;
	public SplineController ComputeNodeTimeFromOther = null;
	public int SegmentCount = 250;
    public int ColorSegment = 0;
    public float SplineMagnitude = 0f;
	[Range(0f,1f)]
	public float SimulatePosition = 0f;

    SplineInterpolator mSplineInterp;
	Transform[] mTransforms;
	private Vector3 _drawGizmosPos = Vector3.zero;
	public Vector3 drawGizmosPos => _drawGizmosPos;

	private Vector3 GetPosition(Transform tr)
	{
		if (UseLocalPosition)
			return tr.localPosition;
		else
			return tr.position;
	}

	void OnDrawGizmos()
	{
		if( DisplayOnDrawGizmos && !Application.isPlaying)
		{
			Transform[] trans = GetTransforms();
			if ( trans == null || trans.Length < 2)
				return;
	
			SplineInterpolator interp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;
			SetupSplineInterpolator(interp, trans);
			interp.StartInterpolation(null, false, WrapMode);
	
			Vector3 prevPos = GetPosition(trans[0]);
            Color color = new Color(0, 0, 0, 1);
            switch(ColorSegment )
            {
                case 0:
                    color.r = 1f;
                    break;
                case 1:
                    color.g = 1f;
                    break;
                case 2:
                    color.b = 1f;
                    break;
            }

            for( int i=0; i<trans.Length; i++ )
            {
                Gizmos.color = i % 2 == 0 ? Color.black : Color.white;
                Gizmos.DrawSphere(GetPosition(trans[i]), 2f);
            }



            for (int c = 1; c <= SegmentCount; c++)
			{
				float currTime = c * Duration / SegmentCount;
				Vector3 currPos = interp.GetHermiteAtTime(currTime);
				float mag = (currPos-prevPos).magnitude * 2;
                Gizmos.color = new Color(color.r*mag, color.g * mag, color.b * mag, 1);
				Gizmos.DrawLine(prevPos, currPos);
				prevPos = currPos;
			}

			Gizmos.color = Color.magenta;
			_drawGizmosPos = interp.GetHermiteAtTime(SimulatePosition);
			Gizmos.DrawSphere(_drawGizmosPos, 2f);
		}
		else
		{
			/*
			if( mSplineInterp != null && mTransforms != null )
			{
				Vector3 prevPos = mTransforms[0].position;
			
				for (int c = 1; c <= SegmentCount; c++)
				{
					float currTime = c * Duration / SegmentCount;
					Vector3 currPos = mSplineInterp.GetHermiteAtTime(currTime);
					float mag = (currPos-prevPos).magnitude * 2;
					Gizmos.color = new Color(mag, 0, 0, 1);
					Gizmos.DrawLine(prevPos, currPos);
					prevPos = currPos;
				}
			}
			*/
		}
	}



	[ContextMenu("Recompute Spline")]
	public void Init()
	{
		mSplineInterp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;

		mTransforms = GetTransforms();

		if (HideOnExecute)
			DisableTransforms();

		if (AutoStart)
			FollowSpline();		
	}

	void SetupSplineInterpolator(SplineInterpolator interp, Transform[] trans)
	{
		interp.Reset();
		
		if(ComputeNodeTimeWithDistance )
        {
			if (ComputeNodeTimeFromOther != null)
			{
				if (ComputeNodeTimeFromOther.transforms == null || ComputeNodeTimeFromOther.transforms.Length == 0)
					ComputeNodeTimeFromOther.Init();
				ComputeSplineMagnitude(ComputeNodeTimeFromOther.transforms);
			}
			else
			{
				ComputeSplineMagnitude(trans);
			}
        }
		float step = (AutoClose) ? Duration / trans.Length :
			Duration / (trans.Length - 1);
        float currentMagnitude = 0;

		int c;
		for (c = 0; c < trans.Length; c++)
		{
            float fNodeTime = step * c;
			
			if (ComputeNodeTimeWithDistance && c>0 )
            {
				if (ComputeNodeTimeFromOther != null && ComputeNodeTimeFromOther.transforms.Length == trans.Length)
					currentMagnitude += (ComputeNodeTimeFromOther.transforms[c].position - ComputeNodeTimeFromOther.transforms[c - 1].position).magnitude;
				else
					currentMagnitude += (trans[c].position - trans[c - 1].position).magnitude;
                fNodeTime = currentMagnitude / SplineMagnitude;
            }

            if (OrientationMode == eOrientationMode.NODE)
			{
				interp.AddPoint(GetPosition(trans[c]), trans[c].rotation, fNodeTime, new Vector2(0, 1));
			}
			else if (OrientationMode == eOrientationMode.TANGENT)
			{
				Quaternion rot;
				if (c != trans.Length - 1)
					rot = Quaternion.LookRotation(GetPosition(trans[c + 1]) - GetPosition(trans[c]), trans[c].up);
				else if (AutoClose)
					rot = Quaternion.LookRotation(GetPosition(trans[0]) - GetPosition(trans[c]), trans[c].up);
				else
					rot = trans[c].rotation;

				interp.AddPoint(GetPosition(trans[c]), rot, fNodeTime, new Vector2(0, 1));
			}
		}

		if (AutoClose)
			interp.SetAutoCloseMode(Duration);

		interp.ForceDirectionWithNode(ForceDirectionWithNode);
	}

	[ContextMenu("Orientate Nodes To Follow Simple Spline")]
	private void OrientateNodesToFollowSimpleSpline()
	{
		Init();

		int nodeCount = mSplineInterp.nodeCount;
		for (int i = 1; i < nodeCount - 1; ++i)
		{
			float time = mSplineInterp.GetTimeAtNode(i);
			Quaternion rot = mSplineInterp.GetRotationAtTime(time);
			mTransforms[i-1].rotation = rot;
		}
	}

	[ContextMenu("Show time at nodes")]
	private void ShowTimeAtNodes()
	{
		Init();

		int nodeCount = mSplineInterp.nodeCount;
		for (int i = 1; i < nodeCount - 1; ++i)
		{
			float time = mSplineInterp.GetTimeAtNode(i);
			Debug.Log($"[SPLINE] ShowTimeAtNodes {i} : {time} at pos {mSplineInterp.GetHermiteAtTime(time)}");
		}
	}

	/// <summary>
	/// compute the spline magnitude
	/// </summary>
	void ComputeSplineMagnitude(Transform[] trans )
    {
        SplineMagnitude = 0f;
        for( int i=0; i<trans.Length-1; i++ )
        {
            Vector3 v = trans[i + 1].position - trans[i].position;
            SplineMagnitude += v.magnitude;
        }
        if (AutoClose)
        {
            Vector3 v = trans[0].position - trans[trans.Length-1].position;
            SplineMagnitude += v.magnitude;
        }
    }

    /// <summary>
    /// Returns children transforms, sorted by name.
    /// </summary>
    Transform[] GetTransforms()
	{
		if (SplineRoot != null)
		{
			List<Component> components = new List<Component>(SplineRoot.GetComponentsInChildren(typeof(Transform)));
		#if !UNITY_FLASH
			List<Transform> transforms = components.ConvertAll(c => (Transform)c);
		#else
			List<Transform> transforms = new List<Transform>();
			foreach( Component c in components )
			{
				transforms.Add( c.transform );
			}
		#endif

			transforms.Remove(SplineRoot.transform);
			transforms.Sort(delegate(Transform a, Transform b)
			{
				return a.name.CompareTo(b.name);
			});

			return transforms.ToArray();
		}

		return null;
	}

	/// <summary>
	/// Disables the spline objects, we don't need them outside design-time.
	/// </summary>
	void DisableTransforms()
	{
		if (SplineRoot != null)
		{
			SplineRoot.SetActive(false);
		}
	}


	/// <summary>
	/// Starts the interpolation
	/// </summary>
	void FollowSpline()
	{
		if (mTransforms.Length > 0)
		{
			SetupSplineInterpolator(mSplineInterp, mTransforms);
			mSplineInterp.StartInterpolation(null, true, WrapMode);
		}
	}
	
	/// <summary>
	/// GetPositionAtTime
	/// </summary>
	public Vector3 GetPositionAtTime( float fTime )
	{
		if ( mSplineInterp != null )
		{
			return mSplineInterp.GetHermiteAtTime( fTime );
		}
		return Vector3.zero;
	}

#if UNITY_EDITOR
	public Vector3 GetPositionAtTimeForEditor(float fTime)
	{
		Transform[] trans = GetTransforms();
		if (trans == null || trans.Length < 2)
			return Vector3.zero;

		SplineInterpolator interp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;
		SetupSplineInterpolator(interp, trans);
		interp.StartInterpolation(null, false, WrapMode);

		return interp.GetHermiteAtTime(fTime);
	}
#endif

	/// <summary>
	/// Get Rotation At given Time
	/// </summary>
	/// <param name="time"></param>
	/// <returns></returns>
	public Quaternion GetRotationAtTime(float time)
	{
		if (mSplineInterp != null)
		{
			return mSplineInterp.GetRotationAtTime(time);
		}
		return Quaternion.identity;
	}

#if UNITY_EDITOR
	public Quaternion GetRotationAtTimeForEditor(float time)
	{
		Transform[] trans = GetTransforms();
		if (trans == null || trans.Length < 2)
			return Quaternion.identity;

		SplineInterpolator interp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;
		SetupSplineInterpolator(interp, trans);
		interp.StartInterpolation(null, false, WrapMode);

		return interp.GetRotationAtTime(time);
	}
#endif

}
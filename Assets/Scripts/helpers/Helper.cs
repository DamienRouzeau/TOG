using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public static class Helper
{
    public static int GetDictionaryInt(IDictionary dic, string item)
    {
        int val = 0;
        bool ok = true;

        try
        {
            val = (int)(float)dic[item];
        }
        catch
        {
            ok = false;
        }

        if (!ok)
        {
            try
            {
                val = (int)(long)dic[item];
            }
            catch
            {
                ok = false;
            }
        }
        if (!ok)
        {
            try
            {
                int.TryParse((string)dic[item], out val);
            }
            catch
            {
                ok = false;
            }
        }
        return (val);
    }
    public static float GetDictionaryValue(IDictionary dic, string item)
    {
        float val = 0.0f;
        bool ok = true;

        try
        {
            val = (float)(double)dic[item];
        }
        catch
        {
            ok = false;
        }

        if (!ok)
        {
            try
            {
                val = (float)(long)dic[item];
            }
            catch
            {
                ok = false;
            }
        }
        if (!ok)
        {
            try
            {
                float.TryParse((string)dic[item], out val);
            }
            catch
            {
                ok = false;
            }
        }
        return (val);
    }

    public static void MySetActive(GameObject obj, bool act)
    {
        Debug.Assert(obj != null, "MySetActive with null GameObject param!");
        Debug.Log("SET " + obj.name + " " + act);
        obj.SetActive(act);
    }

    public static void MySetSetvisible(GameObject obj, bool act)
    {
        Debug.Assert(obj != null, "MySetSetvisible with null GameObject param!");
        //       Debug.Log("VIS " + obj.name + " " + act);
        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.SetVisible(act);
        }
        obj.SetVisible(act);
    }

    public static bool IsDestroyed(this GameObject gameObject)
	{
        Debug.Assert(gameObject != null, "IsDestroyed with null GameObject param!");
        return gameObject == null && !ReferenceEquals(gameObject, null);
	}

	public static GameObject FindInChildren(this GameObject go, string name)
    {
        Debug.Assert(go != null, "FindInChildren with null GameObject param!");
        Debug.Assert(!string.IsNullOrEmpty(name), "FindInChildren with empty or null Name param!");
        foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
	    {
			if (child.gameObject.name == name)		return (child.gameObject);
    	}
		return null;
    }



	public static void SetVisible(this GameObject go, bool bVisible)
	{
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
		
		foreach (Renderer child in renderers)
		{
			child.enabled = bVisible;
		}
		
		if (go.GetComponent<Renderer>() != null)
		{
			go.GetComponent<Renderer>().enabled = bVisible;
		}
            
	}

	public static void ForceOpaque(this GameObject go)
	{
		Renderer rend;
		rend = (Renderer)go.GetComponent<Renderer>();
		if (rend != null)
		{
			if (rend.material.HasProperty("_Color"))
			{
				Color color = rend.material.GetColor("_Color");
				color.a = 1.0f;
				rend.material.SetColor("_Color",color);
			}
		}
	}

    public static Quaternion QuaternionFromMatrix(Matrix4x4 m) 
	{
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		return q;
	}
	
}
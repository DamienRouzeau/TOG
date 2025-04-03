using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class connection : MonoBehaviour
{
    public static connection myself;

    const int retries = 5;
    const int nr = 32;
    double[] keys = new double[nr];
    string[] results = new string[nr];
    string[] errors = new string[nr];

    private void Awake()
    {
        for (int i=0;i<nr;i++)
        {
            keys[i] = 0;
            results[i] = "";
            errors[i] = "";
        }
        myself = gameObject.GetComponent<connection>();
    }


    double NewId()
    {
        TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        double id = span.TotalMilliseconds;
        for (int i = 0; i < nr; i++)
        {
            if (keys[i] == 0.0)
            {
                keys[i] = id;
                results[i] = "";
                errors[i] = "";
                return (id);
            }
        }
        return (0);
    }

    public bool Wait(double id)
    {
        for (int i = 0; i < nr; i++)
        {
            if (keys[i] == id)
            {
                if ((results[i] != "") || (errors[i] != ""))
                    return (false);
                else
                    return (true);
            }
        }
        return (false);
    }

    public string Error(double id)
    {
        for (int i = 0; i < nr; i++)
        {
            if (keys[i] == id)
            {
                string ret = errors[i];
                return (ret);
            }
        }
        return ("");
    }

    public string Result(double id)
    {
        for (int i = 0; i < nr; i++)
        {
            if (keys[i] == id)
            {
                string ret = results[i];
                keys[i] = 0;
                results[i] = "";
                errors[i] = "";
                return (ret);
            }
        }
        return ("");
    }

    public double Post(string uri,WWWForm form)
    {
        Debug.Log("POST called:"+ uri);
        double id = NewId();
        if (id != 0) StartCoroutine(_Operate(uri, id,form));
        return (id);
    }

    public double Get(string uri)
    {
        Debug.Log("GET called:" + uri);
        double id = NewId();
        if (id != 0)        StartCoroutine(_Operate(uri,id));
        return (id);
    }

    static int maxrepeat = 3;
    static int priority = 0;
    IEnumerator _Operate(string uri,double id,WWWForm form = null)
    {
        string error="";
        string ret="";
        int index = 0;
        for (int i = 0; i < nr; i++)
        {
            if (keys[i] == id)
            {
                index = i;
                break;
            }
        }
        for (int i = 0; i < retries; i++)
        {
            error = "";
            ret = "";
            if (form == null)
            {
                for (int rep =0;rep< maxrepeat; rep++)
                {
                    using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
                    {
                        yield return webRequest.SendWebRequest();

                        if (webRequest.isNetworkError)
                        {
                            if (rep == (maxrepeat -1))                            error = webRequest.error;
                        }
                        else
                        {
                            rep = maxrepeat;
                            ret = webRequest.downloadHandler.text;
                        }
                    }
                }
            }
            else
            {
                for (int rep = 0; rep < maxrepeat; rep++)
                {
                    using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
                    {
                        yield return webRequest.SendWebRequest();

                        if (webRequest.isNetworkError)
                        {
                            if (rep == (maxrepeat - 1)) error = webRequest.error;
                        }
                        else
                        {
                            rep = maxrepeat;
                            ret = webRequest.downloadHandler.text;
                        }
                    }
                }
            }
            if (ret != "")
            {
                results[index] = ret; 
                break;
            }
            else
            {
                if (i == (retries-1))
                {
                    errors[index] = error;
                }
            }
        }
    }
}

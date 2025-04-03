using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cheat codes class
/// </summary>
public sealed class CheatCodes : MonoBehaviour
{
	public delegate void OnCheatCode(int nCode);
	public delegate bool OnSpecialCheatCode(string sLeftCode, string sRightCode);
	public enum TriggerMode { Corners, Touch3 };
	public TriggerMode 	m_eTriggerMode = TriggerMode.Corners;

	private string _keys = "";
	private float _lastKeyTime;

	public OnCheatCode cheatCodeCbk = null;
	public OnSpecialCheatCode specialCheatCodeCbk = null;

	public List<string> m_sCodeList = new List<string>();
	public Dictionary<string,int> m_dictSpecialCodes = new Dictionary<string,int>();
	public float m_fClearDelay = 1.0f;
	public bool m_bIsEnabled = true;
	private bool _wasEnabled = false;

	// Use this for initialization
	void Start()
	{
		_lastKeyTime = Time.realtimeSinceStartup;
		for( int i = 0; i < m_sCodeList.Count; ++i )
		{
			m_sCodeList[i] = m_sCodeList[i].ToLower();
		}
	}

	// Update is called once per frame
	void Update()
	{
		bool bWasEnabled = _wasEnabled;
		_wasEnabled = m_bIsEnabled;
	
		if( !m_bIsEnabled || !bWasEnabled )
			return;
	
		if( Input.anyKey )
		{
			_lastKeyTime = Time.realtimeSinceStartup;
			_keys += Input.inputString.ToLower();
		}
		else if( Time.realtimeSinceStartup - _lastKeyTime > m_fClearDelay )
		{
			_keys = "";
		}
	
		if( Input.anyKey )
		{
			if( TestInputKeys( _keys ) )
				_keys = "";
		}
	}

	/// <summary>
	/// Tests the input keys.
	/// </summary>
	/// <param name='sKeys'>
	/// S keys.
	/// </param>
	public bool TestInputKeys( string sKeys )
	{
		for( int i = 0; i < m_sCodeList.Count; ++i )
		{
			if( m_sCodeList[i] == sKeys )
			{
				if (cheatCodeCbk != null)
					cheatCodeCbk(i);
				return true;
			}
		}
		foreach( KeyValuePair<string,int> kvp in m_dictSpecialCodes )
		{
			if( kvp.Value==sKeys.Length && kvp.Key==sKeys.Substring(0,kvp.Key.Length) )
			{
				if( specialCheatCodeCbk!=null )
				{
					//bool bResult = false;
					if (specialCheatCodeCbk != null)
					{
						if (specialCheatCodeCbk(kvp.Key, sKeys.Substring(kvp.Key.Length)))
							return true;
					}
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Gets the length of the keys.
	/// </summary>
	/// <returns>
	/// The keys length.
	/// </returns>
	public int GetKeysLength()
	{
		return _keys.Length;
	}

	/// <summary>
	/// Sets the cheat code cbk.
	/// </summary>
	/// <param name='cbk'>
	/// Cbk.
	/// </param>
	public void SetCheatCodeCbk(OnCheatCode cbk)
	{
		cheatCodeCbk = cbk;
	}

	/// <summary>
	/// Adds the cheat code cbk.
	/// </summary>
	/// <param name='cbk'>
	/// Cbk.
	/// </param>
	public void AddCheatCodeCbk(OnCheatCode cbk)
	{
		cheatCodeCbk += cbk;
	}

	/// <summary>
	/// Removes the cheat code cbk.
	/// </summary>
	/// <param name='cbk'>
	/// Cbk.
	/// </param>
	public void RemoveCheatCodeCbk(OnCheatCode cbk )
	{
		cheatCodeCbk -= cbk;
	}

	/// <summary>
	/// Adds the cheatcode cbk.
	/// </summary>
	public void AddSpecialCheatCodeCbk(OnSpecialCheatCode cbk)
	{
		specialCheatCodeCbk += cbk;
	}

	/// <summary>
	/// Removes the cheatcode cbk.
	/// </summary>
	public void RemoveSpecialCheatCodeCbk(OnSpecialCheatCode cbk)
	{
		specialCheatCodeCbk -= cbk;
	}
}

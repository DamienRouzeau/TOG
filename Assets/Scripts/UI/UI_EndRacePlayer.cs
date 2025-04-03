using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RRLib;

public class UI_EndRacePlayer : MonoBehaviour
{
	#region Enums

	public enum ValueType
	{
        Treasures,
        Kills,
        Deaths,
        BoatKills,
        Turrets,
        Obstacles,
        Mermaids,
        Monsters,
        Skulls,
        Drone,
        Mine,
        SuperDrone,
        MegaDroid,
        PlasmaBomb,
        Bomber,
        Conveyor,
        DroneUltra,
        Archives,
        Scientists
    }

    public enum ValuePanel
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    #endregion

    #region Classes

    [System.Serializable]
    public class ValueTypeCoef
    {
        public ValueType valueType = ValueType.Treasures;
        public float valueCoef = 1f;
    }

    [System.Serializable]
    public class ValueTypeList
	{
        public List<ValueTypeCoef> valueTypeCoefs = null;

        public ValueType GetBetterValueType(ValueByValueType values)
		{
            ValueType betterValueType = ValueType.Treasures;
            if (valueTypeCoefs != null)
			{
                float betterValue = -1f;
                foreach(ValueTypeCoef valTypeCoef in valueTypeCoefs)
				{
                    float val = values[valTypeCoef.valueType] * Mathf.Abs(valTypeCoef.valueCoef);
                    if (val > betterValue)
					{
                        betterValue = val;
                        betterValueType = valTypeCoef.valueType;
                    }
				}
			}
            return betterValueType;
        }
    }

    #endregion

    #region EnumArrays

    [System.Serializable] public class ValueTypesByPanel : RREnumArray<ValuePanel, ValueTypeList> { }
    [System.Serializable] public class TitleByValueType : RREnumArray<ValueType, string> { }
    [System.Serializable] public class TitleFieldByValuePanel : RREnumArray<ValuePanel, TMP_Text> { }
    [System.Serializable] public class ValueFieldByValuePanel : RREnumArray<ValuePanel, TMP_Text> { }
    [System.Serializable] public class ValueByValueType : RREnumArray<ValueType, int> { }

    #endregion

    #region Properties

    [SerializeField]
    private ValueTypesByPanel _valueTypesByPanel = null;
    [SerializeField]
    private TitleByValueType _titleByValueType = null;
    [SerializeField]
    private TitleFieldByValuePanel _titleFieldByValuePanel = null;
    [SerializeField]
    private ValueFieldByValuePanel _valueFieldByValuePanel = null;
    [SerializeField]
    private Image[] pic = null;
    [SerializeField]
    private TMP_Text pirateName = null;
    [SerializeField]
    private TMP_Text pirateTitle = null;
    [SerializeField]
    private TMP_Text pirateScore = null;
    [SerializeField]
    private TMP_Text malusScore = null;

    #endregion

    public void Setup(UI_EndRaceResult.EndPlayerData endPlayerData)
    {
        //Color teamColor = endPlayerData.team == 0 ? Color.blue : Color.red;
        for (int i = 0; i < pic.Length; i++)
        {
            pic[i].gameObject.SetActive(i == endPlayerData.skinId);
        }
        //pirateName.color = teamColor;
        pirateName.text = endPlayerData.sName;
        pirateTitle.text = endPlayerData.sTitle;
        pirateScore.text = $"{RRLanguageManager.instance.GetString("str_score")} : {endPlayerData.score}";
        if (malusScore != null)
        {
            if (endPlayerData.malus > 0f)
            {
                malusScore.gameObject.SetActive(true);
                malusScore.text = $"{RRLanguageManager.instance.GetString("str_bod_playerresults_killed")} : {endPlayerData.score + endPlayerData.malus} - {endPlayerData.malus}";
            }
            else
			{
                malusScore.gameObject.SetActive(false);
            }
        }

        ValueByValueType values = new ValueByValueType();
        values[ValueType.Treasures] = (int)endPlayerData.treasures;
        values[ValueType.Kills] = (int)endPlayerData.kills;
        values[ValueType.Deaths] = (int)endPlayerData.deaths;
        values[ValueType.Obstacles] = (int)endPlayerData.obstacles;
        values[ValueType.BoatKills] = (int)endPlayerData.boatsKills;
        values[ValueType.Turrets] = (int)endPlayerData.tourret;
        values[ValueType.Mermaids] = (int)endPlayerData.mermaids;
        values[ValueType.Monsters] = (int)endPlayerData.monsters;
        values[ValueType.Skulls] = (int)endPlayerData.skulls;
        values[ValueType.Drone] = (int)endPlayerData.drone;
        values[ValueType.Mine] = (int)endPlayerData.mine;
        values[ValueType.SuperDrone] = (int)endPlayerData.superDrone;
        values[ValueType.MegaDroid] = (int)endPlayerData.megaDroid;
        values[ValueType.PlasmaBomb] = (int)endPlayerData.plasmaBomb;
        values[ValueType.Bomber] = (int)endPlayerData.bomber;
        values[ValueType.Conveyor] = (int)endPlayerData.conveyor;
        values[ValueType.DroneUltra] = (int)endPlayerData.droneUltra;
        values[ValueType.Archives] = (int)endPlayerData.archives;
        values[ValueType.Scientists] = (int)endPlayerData.scientists;
        FillPanels(values);
    }

    private void FillPanels(ValueByValueType values)
    {
        foreach (ValuePanel panel in System.Enum.GetValues(typeof(ValuePanel)))
        {
            ValueTypeList list = _valueTypesByPanel[panel];
            ValueType valType = list.GetBetterValueType(values);
            string title = _titleByValueType[valType];
            string value = values[valType].ToString();
            _titleFieldByValuePanel[panel].text = RRLanguageManager.instance.GetString(title);
            _valueFieldByValuePanel[panel].text = value;
        }
    }
}

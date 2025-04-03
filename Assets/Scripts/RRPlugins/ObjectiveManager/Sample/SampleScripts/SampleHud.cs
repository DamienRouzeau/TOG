using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RRObjective
{
    public class SampleHud : MonoBehaviour
    {
        [SerializeField]
        Text m_timeText = null;
        [SerializeField]
        Text m_endText = null;
        [SerializeField]
        Text[] m_lapValueText = null;
        [SerializeField]
        Text[] m_coinValueText = null;

        private int m_nLapTarget = 0;
        private int m_nCoinsTarget = 0;

        public void Init()
        {
            RRObjectiveManager objectiveManager = RRObjectiveManager.instance;

            objectiveManager.AddObjectiveUpdateDlg("Time", OnTimeUpdate);
            objectiveManager.AddObjectiveUpdateDlg("Path", OnPathUpdate);

            objectiveManager.AddObjectiveUpdateDlg("Coins", OnCoinsUpdate);
            objectiveManager.AddObjectiveReachDlg("Path", OnPathComplete);

            objectiveManager.AddObjectiveReachDlg("Coins", OnCoinsComplete);

            // Set initial values
            Objectives.ObjData initial, target;
            objectiveManager.GetInitialAndTargetValue("Path", out initial, out target);
            m_nLapTarget = (int)( ((ObjectivePath.PathObjData)target).v.x );
            objectiveManager.GetInitialAndTargetValue("Coins", out initial, out target);
            m_nCoinsTarget = ((ObjectiveInt.IntObjData)target).m_value;

            for( int i=0; i< m_lapValueText.Length; i++ )
            {
                m_lapValueText[i].text = "0/" + m_nLapTarget;
            }

            for (int i = 0; i < m_coinValueText.Length; i++)
            {
                m_coinValueText[i].text = "0/" + m_nCoinsTarget;
            }

            m_endText.text = "";
        }

        public void SetFinalText( string s )
        {
            m_endText.text = s;
        }

        private void OnTimeUpdate(int playerId, string sObjectiveId, Objectives.ObjData oldValue, Objectives.ObjData newValue)
        {
            ObjectiveFloat.FloatObjData floatObjData = (ObjectiveFloat.FloatObjData)newValue;
            m_timeText.text = floatObjData.m_value.ToString("00");
        }

        private void OnPathUpdate(int playerId, string sObjectiveId, Objectives.ObjData oldValue, Objectives.ObjData newValue)
        {
            ObjectivePath.PathObjData pathObjDataOld = (ObjectivePath.PathObjData)oldValue;
            ObjectivePath.PathObjData pathObjDataNew = (ObjectivePath.PathObjData)newValue;

            int nOldTurn = Mathf.FloorToInt(pathObjDataOld.v.x);
            int nNewTurn = Mathf.FloorToInt(pathObjDataNew.v.x);
            if( nOldTurn!=nNewTurn )
            {
                m_lapValueText[playerId].text = nNewTurn.ToString() + "/" + m_nLapTarget.ToString();
            }
        }

        private void OnCoinsUpdate(int playerId, string sObjectiveId, Objectives.ObjData oldValue, Objectives.ObjData newValue)
        {
            ObjectiveInt.IntObjData intObjData = (ObjectiveInt.IntObjData)newValue;
            m_coinValueText[playerId].text = intObjData.m_value.ToString() +  "/" + m_nCoinsTarget;
        }

        private void OnPathComplete(int playerId, string sObjectiveId )
        {
            m_lapValueText[playerId].text = "DONE";
        }

        private void OnCoinsComplete(int playerId, string sObjectiveId)
        {
            m_coinValueText[playerId].text = "DONE";
        }


    }
}

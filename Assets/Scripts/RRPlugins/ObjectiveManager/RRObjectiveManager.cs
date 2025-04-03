using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RRLib;

namespace RRObjective
{
    public class RRObjectiveManager : RRSingletonMonoBehaviour<RRObjectiveManager>
    {
        public delegate void OnWin(int playerId );
        public delegate void OnLose(int playerId);

        public enum ObjectifManagerState { notReady, playing, pause }

        public ObjectifManagerState state { get { return m_state; } }

        public OnWin onAddWinDlg { set { m_onWinDlg += value; } }
        public OnWin onSubWinDlg { set { m_onWinDlg -= value; } }
        public OnLose onAddLoseDlg { set { m_onLoseDlg += value; } }
        public OnLose onSubLoseDlg { set { m_onLoseDlg -= value; } }


        #region public methods
        /// <summary>
        ///  Pause the Objective Manager
        /// </summary>
        public void Pause()
        {
            m_state = ObjectifManagerState.pause;
        }

        /// <summary>
        ///  Activate the Objective Manager
        /// </summary>
        public void Play()
        {
            m_state = ObjectifManagerState.playing;
        }

        /// <summary>
        ///  to add a callback on UpdateObjective for sObjId Objective
        /// </summary>
        public void AddObjectiveUpdateDlg( string sObjId, Objectives.OnObjectiveUpdate OnObjectiveUpdate )
        {
            Objectives objectives = null;
            if( m_objectives.TryGetValue( sObjId, out objectives ) )
            {
                objectives.AddObjectiveUpdateDlg(OnObjectiveUpdate);
            }
            else
            {
                Debug.LogWarning("objectives " + sObjId + " don't exist");
            }
        }

        /// <summary>
        ///  to release callback on UpdateObjective for sObjId Objective
        /// </summary>
        public void SubObjectiveUpdateDlg(string sObjId, Objectives.OnObjectiveUpdate OnObjectiveUpdate)
        {
            Objectives objectives = null;
            if (m_objectives.TryGetValue(sObjId, out objectives))
            {
                objectives.SubObjectiveUpdateDlg(OnObjectiveUpdate);
            }
            else
            {
                Debug.LogWarning("objectives " + sObjId + " don't exist");
            }
        }

        /// <summary>
        ///  to add callback when sObjId Objective is complete
        /// </summary>
        public void AddObjectiveReachDlg(string sObjId, Objectives.OnObjectiveReach OnObjectiveReach)
        {
            Objectives objectives = null;
            if (m_objectives.TryGetValue(sObjId, out objectives))
            {
                objectives.AddObjectiveReachDlg(OnObjectiveReach);
            }
            else
            {
                Debug.LogWarning("objectives " + sObjId + " don't exist");
            }
        }

        /// <summary>
        ///  to release complete callback on sObjId Objective
        /// </summary>
        public void SubObjectiveReachDlg(string sObjId, Objectives.OnObjectiveReach OnObjectiveReach)
        {
            Objectives objectives = null;
            if (m_objectives.TryGetValue(sObjId, out objectives))
            {
                objectives.SubObjectiveReachDlg(OnObjectiveReach);
            }
            else
            {
                Debug.LogWarning("objectives " + sObjId + " don't exist");
            }
        }

        /// <summary>
        ///  to know initial anf target value for a given objective
        /// </summary>
        public void GetInitialAndTargetValue(string sObjId, out Objectives.ObjData initial, out Objectives.ObjData target)
        {
            initial = null;
            target = null;
            Objectives objectives = null;
            if (m_objectives.TryGetValue(sObjId, out objectives))
            {
                objectives.GetInitialAndTargetValue(out initial, out target );
            }
            else
            {
                Debug.LogWarning("objectives " + sObjId + " don't exist");
            }
        }

        /// <summary>
        ///  to update objective with an add value
        /// </summary>
        public void UpdateObjective( string sObjId, int nPlayerId, Objectives.ObjData add )
        {
            if( m_state!= ObjectifManagerState.playing )
            {
                return;
            }

            Objectives objectives = null;
            if (m_objectives.TryGetValue(sObjId, out objectives))
            {
                if( objectives.UpdateData(nPlayerId, add) )
                {
                    // objective complete
                    switch ( objectives.objectiveType )
                    {
                        case Objectives.ObjectiveType.lose: // it's the defeat
                            {
                                m_onLoseDlg?.Invoke(nPlayerId);
                            }
                            break;
                        case Objectives.ObjectiveType.mandatory: // check if win
                            {
                                bool bWin = CheckWin(nPlayerId);
                                if( bWin )
                                {
                                    m_onWinDlg?.Invoke(nPlayerId);
                                }
                                else if( objectives.isGameEndWhenReach ) // we don't win and have to end game, it's a lose
                                {
                                    m_onLoseDlg?.Invoke(nPlayerId);
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                Debug.LogWarning("objectives " + sObjId + " don't exist");
            }
        }

        #endregion

        private void Awake()
        {
            m_objectivePlayers = new List<RRObjectivePlayer>();
            InitObjectives();
            m_state = ObjectifManagerState.playing;
        }

        private bool CheckWin( int nPlayerId )
        {
            bool bWin = true;
            // check if we have only one mandatory not finish, he doesn't win
            foreach (KeyValuePair<string, Objectives> pair in m_objectives)
            {
                if (pair.Value.objectiveType == Objectives.ObjectiveType.mandatory && !pair.Value.IsObjectiveComplete(nPlayerId) )
                {
                    bWin = false;
                }
            }
            return bWin;
        }

        internal int RegisterPlayer( RRObjectivePlayer player )
        {
            int nId = m_objectivePlayers.Count;
            player.SetId(nId);
            m_objectivePlayers.Add(player);

            foreach( KeyValuePair<string, Objectives> pair in m_objectives )
            {
                if( !pair.Value.isGlobalObjective )
                {
                    pair.Value.AddPlayer(player);
                }
            }
            return nId;
        }

        internal void InitObjectives()
        {
            m_objectives = new Dictionary<string, Objectives>();
            Objectives[] objectives = transform.GetComponents<Objectives>();
            for( int i=0; i<objectives.Length; i++ )
            {
                m_objectives.Add(objectives[i].objectiveId, objectives[i]);
                objectives[i].Setup();
            }
        }

#region private attributes
        private OnWin m_onWinDlg;
        private OnLose m_onLoseDlg;

        private List<RRObjectivePlayer> m_objectivePlayers;
        private Dictionary<string, Objectives> m_objectives;
        private ObjectifManagerState m_state = ObjectifManagerState.notReady;

#endregion
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class Objectives : MonoBehaviour 
    {
        public enum ObjectiveType { mandatory, optional, lose }

        public delegate void OnObjectiveUpdate(int playerId, string sObjectiveId, ObjData oldValue, ObjData newValue);
        public delegate void OnObjectiveReach(int playerId, string sObjectiveId);


        public class ObjData
        {
            public virtual void UpdateData( ObjData other )
            {
                Debug.LogWarning("WARNING ! your objData class don't implement Add function");
            }

            public virtual ObjData Copy()
            {
                Debug.LogWarning("WARNING ! your objData class don't implement copy function");
                return null;
            }
        }


        public class ObjectiveInstant
        {
            public float fTime;
            public ObjData data;

            public ObjectiveInstant(float _fTime, ObjData _data )
            {
                fTime = _fTime;
                data = _data;
            }
        }

        public class PlayerInstants
        {
            public float fLastEntryTime;
            public Queue<ObjectiveInstant> instants;

            public PlayerInstants()
            {
                fLastEntryTime = 0f;
                instants = new Queue<ObjectiveInstant>();
            }
        }

        public string objectiveId { get { return m_objectiveId; } }
        public bool isGlobalObjective { get { return m_globalObjective; } }
        public bool isGameEndWhenReach { get { return m_gameEndWhenReach; } }
        public ObjectiveType objectiveType { get { return m_objectiveType; } }


        [SerializeField]
        [Tooltip("The id to found the objective")]
        private string m_objectiveId = "";
        [SerializeField]
        [Tooltip("Is this objective need to win, or make it lose")]
        ObjectiveType m_objectiveType = ObjectiveType.mandatory;
        [SerializeField]
        [Tooltip("Is this objective the same for everyone like game time or is individual")]
        private bool m_globalObjective = false;
        [SerializeField]
        private bool m_gameEndWhenReach = false;

        [SerializeField]
        [Header("replay save data")]
        private float m_fMinTimeBetweenRecords = 0.1f;
        /*[SerializeField]
        private TObjData m_initialValue;
        [SerializeField]
        private TObjData m_targetValue;*/

        protected Dictionary<int, ObjData> m_playersValues;
        protected Dictionary<int, PlayerInstants> m_playersInstants;

        protected OnObjectiveUpdate m_onObjectiveUpdate;
        protected OnObjectiveReach m_onObjectiveReach;

        internal void AddObjectiveUpdateDlg( OnObjectiveUpdate OnObjectiveUpdate)
        {
            m_onObjectiveUpdate += OnObjectiveUpdate;
        }

        internal void SubObjectiveUpdateDlg( OnObjectiveUpdate OnObjectiveUpdate)
        {
            m_onObjectiveUpdate -= OnObjectiveUpdate;
        }

        internal void AddObjectiveReachDlg(OnObjectiveReach OnObjectiveReach)
        {
            m_onObjectiveReach += OnObjectiveReach;
        }

        internal void SubObjectiveReachDlg(OnObjectiveReach OnObjectiveReach)
        {
            m_onObjectiveReach -= OnObjectiveReach;
        }

        internal virtual void GetInitialAndTargetValue( out ObjData initial, out ObjData target )
        {
            initial = new ObjData();
            target = new ObjData();
        }



        internal virtual void Setup()
        {
            m_playersValues = new Dictionary<int, ObjData>();
            m_playersInstants = new Dictionary<int, PlayerInstants>();

            if( m_globalObjective )
            {
                AddPlayerWithInitialValues( -1 );
            }
        }

        internal virtual void AddPlayer( RRObjectivePlayer player )
        {
            AddPlayerWithInitialValues(player.playerId);
        }

        internal virtual void AddPlayerWithInitialValues( int playerId)
        {
            Debug.LogWarning("WARNING ! your objective class don't implement InitPlayerValues");
        }

        internal virtual bool UpdateData( int playerId, ObjData add )
        {
            ObjData playerData = null;
            if (m_playersValues.TryGetValue(playerId, out playerData ))
            {
                ObjData old = playerData.Copy();
                playerData.UpdateData(add);
                ObjData newOne = playerData.Copy();
                RegisterPlayerInstant(playerId, newOne);
                m_onObjectiveUpdate?.Invoke(playerId, objectiveId, old, newOne);

                if (CheckObjectiveComplete( playerId, newOne ))
                {
                    m_onObjectiveReach?.Invoke(playerId, objectiveId);
                    return true;
                }
            }
            return false;
        }

        internal bool RegisterPlayerInstant( int playerId, ObjData data )
        {
            PlayerInstants playerInstants = null;
            float currentTime = Time.time;
            if ( m_playersInstants.TryGetValue( playerId, out playerInstants ))
            {
                if(currentTime - playerInstants.fLastEntryTime > m_fMinTimeBetweenRecords )
                {
                    playerInstants.instants.Enqueue(new ObjectiveInstant(currentTime, data));
                    playerInstants.fLastEntryTime = currentTime;
                    return true;
                }
            }
            else // instant not created, so we add the entry
            {
                playerInstants = new PlayerInstants();
                playerInstants.instants.Enqueue(new ObjectiveInstant(currentTime, data));
                m_playersInstants.Add(playerId, playerInstants);
            }
            return false;
        }

        internal virtual bool IsObjectiveComplete(int playerId )
        {
            ObjData playerData = null;
            if (m_playersValues.TryGetValue(playerId, out playerData))
            {
                return CheckObjectiveComplete(playerId, playerData);
            }
            return false;
        }

        internal virtual bool CheckObjectiveComplete(int playerId, ObjData data )
        {
            return false;
        }
    }
}

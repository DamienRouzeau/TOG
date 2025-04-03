using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class RRObjectivePlayer : MonoBehaviour
    {
        public int playerId { get { return m_nObjPlayerId; } }


        public void UpdateObjectiveFloat( string sObjectiveId, float value )
        {
            RRObjectiveManager.instance.UpdateObjective(sObjectiveId, playerId, new ObjectiveFloat.FloatObjData(value));
        }

        public void UpdateObjectiveInt(string sObjectiveId, int value)
        {
            RRObjectiveManager.instance.UpdateObjective(sObjectiveId, playerId, new ObjectiveInt.IntObjData(value));
        }

        public void UpdateObjectiveInt(string sObjectiveId, Objectives.ObjData value)
        {
            RRObjectiveManager.instance.UpdateObjective(sObjectiveId, playerId, value );
        }


        // Start is called before the first frame update
        void Start()
        {
            RRObjectiveManager.instance.RegisterPlayer(this);
        }

#region internal and Private 
        internal void SetId( int nPlayerId )
        {
            m_nObjPlayerId = nPlayerId;
        }
        private int m_nObjPlayerId = -1;
#endregion
    }
}

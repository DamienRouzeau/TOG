using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class ObjectiveInt : Objectives
    {
        public class IntObjData : ObjData
        {
            public int m_value;

            public IntObjData(int n)
            {
                m_value = n;
            }

            public override void UpdateData(ObjData other)
            {
                m_value += ((IntObjData)other).m_value;
            }

            public override ObjData Copy()
            {
                IntObjData intObjData = new IntObjData(m_value);
                return intObjData;
            }
        }

        [SerializeField]
        int initialValue = 0;
        [SerializeField]
        int goalValue = 1;
        [SerializeField]
        [Tooltip("value to know if we increment or reduce the value")]
        bool isIncremental = true;

        internal override void AddPlayerWithInitialValues(int nPlayerId)
        {
            IntObjData intObjData = new IntObjData(initialValue);
            m_playersValues.Add(nPlayerId, intObjData);
            RegisterPlayerInstant(nPlayerId, intObjData);
        }

        internal override void GetInitialAndTargetValue(out ObjData initial, out ObjData target)
        {
            initial = new IntObjData(initialValue);
            target = new IntObjData(goalValue);
        }

        internal override bool CheckObjectiveComplete(int playerId, ObjData data)
        {
            IntObjData intObjData = (IntObjData)data;
            return (isIncremental && intObjData.m_value >= goalValue) ||
                (!isIncremental && intObjData.m_value <= goalValue);
        }

    }
}

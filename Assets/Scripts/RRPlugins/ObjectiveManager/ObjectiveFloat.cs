using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class ObjectiveFloat : Objectives
    {
        public class FloatObjData : ObjData
        {
            public float m_value;

            public FloatObjData( float f )
            {
                m_value = f;
            }

            public override void UpdateData(ObjData other)
            {
                m_value += ((FloatObjData)other).m_value;
            }

            public override ObjData Copy()
            {
                FloatObjData floatObjData = new FloatObjData(m_value);
                return floatObjData;
            }
        }

        [SerializeField]
        float initialValue = 0f;
        [SerializeField]
        float goalValue = 1f;
        [SerializeField]
        [Tooltip("value to know if we increment or reduce the value")]
        bool isIncremental = true;

        internal override void GetInitialAndTargetValue(out ObjData initial, out ObjData target)
        {
            initial = new FloatObjData(initialValue);
            target = new FloatObjData(goalValue);
        }

        internal override void AddPlayerWithInitialValues( int nPlayerId )
        {
            FloatObjData floatObjData = new FloatObjData(initialValue);
            m_playersValues.Add(nPlayerId, floatObjData);
            RegisterPlayerInstant(nPlayerId, floatObjData);
        }

        internal override bool CheckObjectiveComplete(int playerId, ObjData data)
        {
            FloatObjData floatObjData = (FloatObjData)data;
            return (isIncremental && floatObjData.m_value >= goalValue) ||
                (!isIncremental && floatObjData.m_value <= goalValue);
        }

    }
}


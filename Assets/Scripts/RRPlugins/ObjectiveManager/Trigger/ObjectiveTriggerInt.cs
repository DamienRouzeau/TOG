using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class ObjectiveTriggerInt : ObjectiveTrigger
    {
        [SerializeField]
        int m_value = 0;

        protected override void UpdateObjective(RRObjectivePlayer objectivePlayer)
        {
            objectivePlayer.UpdateObjectiveInt(m_objectiveId, m_value);
        }
    }
}


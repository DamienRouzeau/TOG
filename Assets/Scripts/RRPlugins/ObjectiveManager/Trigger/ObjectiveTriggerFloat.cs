using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class ObjectiveTriggerFloat : ObjectiveTrigger
    {
        [SerializeField]
        float m_value = 0f;

        protected override void UpdateObjective(RRObjectivePlayer objectivePlayer)
        {
            objectivePlayer.UpdateObjectiveFloat(m_objectiveId, m_value);
        }
    }
}


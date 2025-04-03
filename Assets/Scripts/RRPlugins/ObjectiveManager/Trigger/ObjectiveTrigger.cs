using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RRObjective
{
    public class ObjectiveTrigger : MonoBehaviour
    {
        public delegate void OnObjectiveTriggerEnterDlg(GameObject gameObject);

        [SerializeField]
        UnityEvent m_onTriggerCbk = null;
        [SerializeField]
        protected string m_objectiveId = null;

        private void OnTriggerEnter(Collider other)
        {
            RRObjectivePlayer objectivePlayer = other.gameObject.GetComponent<RRObjectivePlayer>();
            if (objectivePlayer != null)
            {
                UpdateObjective(objectivePlayer);
                m_onTriggerCbk?.Invoke();
            }
        }

        public void DestroyMySelf( GameObject gameObject )
        {
            GameObject.Destroy(gameObject);
        }

        protected virtual void UpdateObjective(RRObjectivePlayer objectivePlayer)
        {

        }
    }
}

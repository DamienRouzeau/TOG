//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class HoverButton : MonoBehaviour
    {
        public Transform movingPart;

        public Vector3 localMoveDistance = new Vector3(0, -0.1f, 0);

        [Range(0, 1)]
        public float engageAtPercent = 0.95f;

        [Range(0, 1)]
        public float disengageAtPercent = 0.9f;

        public HandEvent onButtonDown;
        public HandEvent onButtonUp;
        public UnityEvent onTriggerMulti;

        public bool engaged = false;
        public bool buttonDown = false;
        public bool buttonUp = false;

        public bool needOnePress = true;

        private Vector3 startPosition;
        private Vector3 endPosition;

        private Vector3 handEnteredPosition;

        private bool hovering;

        private Hand lastHoveredHand;

        private bool _pressDone = false;

        private void Start()
        {
            if (movingPart == null && this.transform.childCount > 0)
                movingPart = this.transform.GetChild(0);

            startPosition = movingPart.localPosition;
            endPosition = startPosition + localMoveDistance;
            handEnteredPosition = endPosition;
        }
#if USE_TESTS
        private void Update()
		{
            if (Input.GetKeyDown(KeyCode.B) && (Camera.main.transform.position - transform.position).sqrMagnitude < 10f)
            {
                InvokeEvents(false, true, true);
            }
        }
#endif
        private void HandHoverUpdate(Hand hand)
        {
            hovering = true;
            lastHoveredHand = hand;

            bool wasEngaged = engaged;

            float currentDistance = Vector3.Distance(movingPart.parent.InverseTransformPoint(hand.transform.position), endPosition);
            float enteredDistance = Vector3.Distance(handEnteredPosition, endPosition);

            if (currentDistance > enteredDistance)
            {
                enteredDistance = currentDistance;
                handEnteredPosition = movingPart.parent.InverseTransformPoint(hand.transform.position);
            }

            float distanceDifference = enteredDistance - currentDistance;

            float lerp = Mathf.InverseLerp(0, localMoveDistance.magnitude, distanceDifference);

            if (lerp > engageAtPercent)
                engaged = true;
            else if (lerp < disengageAtPercent)
                engaged = false;

            movingPart.localPosition = Vector3.Lerp(startPosition, endPosition, lerp);

            InvokeEvents(wasEngaged, engaged, true);
        }

        private void LateUpdate()
        {
            if (hovering == false)
            {
                movingPart.localPosition = startPosition;
                handEnteredPosition = endPosition;

                InvokeEvents(engaged, false, true);
                engaged = false;
            }

            hovering = false;
        }

        private void InvokeEvents(bool wasEngaged, bool isEngaged, bool triggerMulti = false)
        {
            buttonDown = wasEngaged == false && isEngaged == true;
            buttonUp = wasEngaged == true && isEngaged == false;

            if (buttonDown && onButtonDown != null)
                onButtonDown.Invoke(lastHoveredHand);
            if (buttonUp && onButtonUp != null)
                onButtonUp.Invoke(lastHoveredHand);
            if (triggerMulti)
            {
                if (buttonDown && onTriggerMulti != null)
                {
                    if (!_pressDone || !needOnePress)
                    {
                        onTriggerMulti.Invoke();
                        _pressDone = true;
                    }
                }
            }
        }

        [ContextMenu("Press Button")]
        public void PressButton()
		{
            InvokeEvents(false, true);
            InvokeEvents(true, false);
        }
    }
}

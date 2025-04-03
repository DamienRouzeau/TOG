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
	[RequireComponent( typeof( Interactable ) )]
	public class LinearDrive : MonoBehaviour
	{
		public delegate void OnDrivingEvent(bool drive, float value);

		public Transform startPosition;
		public Transform endPosition;
		public LinearMapping linearMapping;
		public bool repositionGameObject = true;
		public bool maintainMomemntum = true;
		public float momemtumDampenRate = 5.0f;

		[HeaderAttribute("Driving events")]
		[Tooltip("Event when driving start")]
		public UnityEvent onDrivingStart = null;
		[Tooltip("Event when driving stop")]
		public UnityEvent onDrivingStop = null;

		protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;

        protected float initialMappingOffset;
        protected int numMappingChangeSamples = 5;
        protected float[] mappingChangeSamples;
        protected float prevMapping = 0.0f;
        protected float mappingChangeRate;
        protected int sampleCount = 0;

        protected Interactable interactable;

		public OnDrivingEvent onDrivingEvent = null;
		private bool _driving = false;

		protected virtual void Awake()
        {
            mappingChangeSamples = new float[numMappingChangeSamples];
            interactable = GetComponent<Interactable>();
        }

        protected virtual void Start()
		{
			if ( linearMapping == null )
			{
				linearMapping = GetComponent<LinearMapping>();
			}

			if ( linearMapping == null )
			{
				linearMapping = gameObject.AddComponent<LinearMapping>();
			}

            initialMappingOffset = linearMapping.value;

			if ( repositionGameObject )
			{
				UpdateLinearMapping( transform );
			}
		}

        protected virtual void HandHoverUpdate( Hand hand )
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();

			if (startingGrabType != GrabTypes.None)
			{
				initialMappingOffset = linearMapping.value - CalculateLinearMapping(hand.transform);
				sampleCount = 0;
				mappingChangeRate = 0.0f;

				if (interactable.attachedToHand == null)
				{
					hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
					onDrivingStart?.Invoke();
					onDrivingEvent?.Invoke(true, linearMapping.value);
					_driving = true;
				}
			}
		}

        protected virtual void HandAttachedUpdate(Hand hand)
        {
            UpdateLinearMapping(hand.transform);

            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject);
				onDrivingStop?.Invoke();
				onDrivingEvent?.Invoke(false, linearMapping.value);
				_driving = false;
			}
        }

        protected virtual void OnDetachedFromHand(Hand hand)
        {
            CalculateMappingChangeRate();
        }


        protected void CalculateMappingChangeRate()
		{
			//Compute the mapping change rate
			mappingChangeRate = 0.0f;
			int mappingSamplesCount = Mathf.Min( sampleCount, mappingChangeSamples.Length );
			if ( mappingSamplesCount != 0 )
			{
				for ( int i = 0; i < mappingSamplesCount; ++i )
				{
					mappingChangeRate += mappingChangeSamples[i];
				}
				mappingChangeRate /= mappingSamplesCount;
			}
		}

        protected void UpdateLinearMapping( Transform updateTransform )
		{
			prevMapping = linearMapping.value;
			linearMapping.value = Mathf.Clamp01( initialMappingOffset + CalculateLinearMapping( updateTransform ) );

			mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = ( 1.0f / Time.deltaTime ) * ( linearMapping.value - prevMapping );
			sampleCount++;

			if ( repositionGameObject )
			{
				transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
			}

			onDrivingEvent?.Invoke(true, linearMapping.value);
		}

        protected float CalculateLinearMapping( Transform updateTransform )
		{
			Vector3 direction = endPosition.position - startPosition.position;
			float length = direction.magnitude;
			direction.Normalize();

			Vector3 displacement = updateTransform.position - startPosition.position;

			return Vector3.Dot( displacement, direction ) / length;
		}


		protected virtual void Update()
        {
            if ( maintainMomemntum && mappingChangeRate != 0.0f )
			{
				//Dampen the mapping change rate and apply it to the mapping
				mappingChangeRate = Mathf.Lerp( mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime );
				linearMapping.value = Mathf.Clamp01( linearMapping.value + ( mappingChangeRate * Time.deltaTime ) );

				if ( repositionGameObject )
				{
					transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
				}

				onDrivingEvent?.Invoke(true, linearMapping.value);
			}
#if USE_TESTS
			if ((Camera.main.transform.position - transform.position).sqrMagnitude < 10f)
			{
				if (Input.GetKey(KeyCode.UpArrow))
				{
					if (Input.GetKeyDown(KeyCode.UpArrow))
					{
						_driving = true;
						onDrivingEvent?.Invoke(_driving, linearMapping.value);
					}
					else
					{
						linearMapping.value = Mathf.Min(linearMapping.value + Time.deltaTime, 1f);
						onDrivingEvent?.Invoke(_driving, linearMapping.value);
						if (repositionGameObject)
						{
							transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
						}
					}
				}
				else if (Input.GetKeyUp(KeyCode.UpArrow))
				{
					_driving = false;
					onDrivingEvent?.Invoke(_driving, linearMapping.value);
					onDrivingStop?.Invoke();
				}


				if (Input.GetKey(KeyCode.DownArrow))
				{
					if (Input.GetKeyDown(KeyCode.DownArrow))
					{
						_driving = true;
						onDrivingEvent?.Invoke(_driving, linearMapping.value);
					}
					else
					{
						linearMapping.value = Mathf.Max(linearMapping.value - Time.deltaTime, 0f);
						onDrivingEvent?.Invoke(_driving, linearMapping.value);
						if (repositionGameObject)
						{
							transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
						}
					}
				}
				else if (Input.GetKeyUp(KeyCode.DownArrow))
				{
					_driving = false;
					onDrivingEvent?.Invoke(_driving, linearMapping.value);
					onDrivingStop?.Invoke();
				}
			}
#endif
		}

		public void SetValue(float value)
		{
			linearMapping.value = Mathf.Clamp01(value);
			if (repositionGameObject)
			{
				transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
			}
		}
	}
}

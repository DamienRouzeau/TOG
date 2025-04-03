//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Animator whose speed is set based on a linear mapping
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class LinearAnimator : MonoBehaviour
	{
		public LinearMapping linearMapping;
		public Animator animator;
		public int disableAnimatorAfterFrames = 20;

		private float currentLinearMapping = float.NaN;
		private int framesUnchanged = 0;


		//-------------------------------------------------
		void Awake()
		{
			if ( animator == null )
			{
				animator = GetComponent<Animator>();
			}

			animator.speed = 0.0f;

			if ( linearMapping == null )
			{
				linearMapping = GetComponent<LinearMapping>();
			}
		}


		//-------------------------------------------------
		void Update()
		{
			if ( currentLinearMapping != linearMapping.value )
			{
				currentLinearMapping = linearMapping.value;
				animator.enabled = true;
				animator.Play( 0, 0, currentLinearMapping );
				framesUnchanged = 0;
			}
			else
			{
				framesUnchanged++;
				if (disableAnimatorAfterFrames > 0 && framesUnchanged > disableAnimatorAfterFrames)
				{
					animator.enabled = false;
					if (currentLinearMapping != 0f && currentLinearMapping != 1f && gameObject.name.Contains("Sail"))
						Debug.Log($"[LINEAR_ANIMATOR_BUG] currentLinearMapping {currentLinearMapping} on {gameObject.name}");
				}
			}
		}
	}
}

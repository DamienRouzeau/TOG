namespace ObjectRandomization {
	using UnityEngine;
	using System;
	using System.Collections;

	[Serializable]
	public class ObjectInfo {

		// these are set in the editor
		public UnityEngine.Object obj;
		public float weighting;

		// constructor
		public ObjectInfo() {
			weighting = 1f; // set default weighting
			rangeStart = 0f;
			rangeEnd = 0f;
		}

		// range within which a random number will cause this prefab to be returned
		[SerializeField]
		float rangeStart;
		public float RangeStart {
			get {
				return rangeStart;
			}
			set {
				rangeStart = value;
			}
		}
		[SerializeField]
		float rangeEnd;
		public float RangeEnd {
			get {
				return rangeEnd;
			}
			set {
				rangeEnd = value;
			}
		}

		// tell if a random number gives this prefab
		public bool RangeContains(float number) {
			if ((number >= rangeStart) && (number < rangeEnd)) return true;
			else return false;
		}
	}

}

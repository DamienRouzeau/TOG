using UnityEngine;  
using UnityEditor; 
using UnityEditorInternal;
using ObjectRandomization;

[CustomEditor(typeof(ObjectRandomizer))]
public class ObjectRandomizerEditor : Editor { 

	// list of prefabs
	ReorderableList list;

	// the target script
	ObjectRandomizer targetScript;

	// list of weightings
	float[] normalizedWeightings;

	// normalized values how much space prefab, weighting and progress bar take up in each line
	const float normalizedPrefabWidth = .36f;
	const float normalizedWeightingWidth = .12f;
	const float normalizedProgressBarWidth = .50f;
	const float normalizedSpacingsWidth = .01f; // there's two spacings atm
	
	void OnEnable() {
		// get script as serialized object
		targetScript = target as ObjectRandomizer;

		// calculate normalized weightings
		UpdateNormalizedWeightings();

		// init reorderable list
		list = new ReorderableList(serializedObject, 
		                           serializedObject.FindProperty("objectInfos"), 
		                           true, true, true, true);

		// add method for drawing list items
		list.drawElementCallback =  
		(Rect rect, int index, bool isActive, bool isFocused) => {
			// remember gui color
			Color guiColor = GUI.color;
			// get current element
			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			// visual stuff
			rect.y += 2; // move everything a bit for more space
			float rectWidth = rect.width;
			float spacingWidth = rectWidth * normalizedSpacingsWidth;
			float prefabWidth = rectWidth * normalizedPrefabWidth;
			float weightingX = prefabWidth + spacingWidth;
			float weightingWidth = rectWidth * normalizedWeightingWidth;
			float progressBarX = weightingX + weightingWidth + spacingWidth;
			float progressBarWidth = rectWidth * normalizedProgressBarWidth;

			// draw prefab
			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, prefabWidth, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("obj"), GUIContent.none);
			// draw weighting
			EditorGUI.PropertyField(
				new Rect(rect.x + weightingX, rect.y, weightingWidth, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("weighting"), GUIContent.none);
			// dirty hacks so that it's possible to change weighting values by dragging over the progress bars
			GUI.color = Color.clear;
			EditorGUI.PropertyField(
				new Rect(rect.x + progressBarX, rect.y, progressBarWidth, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("weighting"), new GUIContent("P"));
			GUI.color = guiColor;

			// draw progress bar for weighting
			EditorGUI.ProgressBar (
				new Rect(rect.x + progressBarX, rect.y, progressBarWidth, EditorGUIUtility.singleLineHeight),
				normalizedWeightings[index], "");
		};

		// update weightings when reordering list
		list.onReorderCallback = (ReorderableList l) => {  
			UpdateNormalizedWeightings();
		};

		// set header
		list.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Weightings");
		};

		// override element adding to set default values
		list.onAddCallback = (ReorderableList l) => {  
			var index = l.serializedProperty.arraySize;
			l.serializedProperty.arraySize++;
			l.index = index;
			var element = l.serializedProperty.GetArrayElementAtIndex(index);
			element.FindPropertyRelative("weighting").floatValue = 1f;
		};
	}

	// calculate and normalize the weightings of all the floats
	void UpdateNormalizedWeightings() {
		int prefabCount = targetScript.objectInfos.Count;
		if (prefabCount > 0) {
			float[] results = new float[(prefabCount)];
			// get sum of all weights
			float weightSum = targetScript.WeightSum();

			// scale everything down by that
			int i = 0;
			foreach (ObjectInfo prefabInfo in targetScript.objectInfos) {
				results[i] = prefabInfo.weighting / weightSum;
				i++;
			}
			// return
			normalizedWeightings = results;
		}
	}
	
	public override void OnInspectorGUI() {
		// draw the list etc.
		serializedObject.Update();
		EditorGUILayout.Space(); // make some space
		list.DoLayoutList();
		serializedObject.ApplyModifiedProperties();

		// update weightings if something changed
		if (GUI.changed) {
			ClampWeightings();
			UpdateNormalizedWeightings();
		}
	}

	// keep all weightings within a certain range
	void ClampWeightings() {
		foreach (ObjectInfo prefabInfo in targetScript.objectInfos) {
			prefabInfo.weighting = Mathf.Clamp(prefabInfo.weighting, 0f, float.MaxValue);
		}
	}
}
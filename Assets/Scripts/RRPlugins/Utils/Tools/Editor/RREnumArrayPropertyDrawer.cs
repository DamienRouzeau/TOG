

using UnityEngine;
using UnityEditor;

//! @class RREnumArrayPropertyDrawer
//!
//! @brief Property drawer for a RREnumArray class
[CustomPropertyDrawer( typeof( RREnumArrayBaseClass ), true )]
public class RREnumArrayPropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		EditorGUI.BeginProperty( position, label, property );

		property.isExpanded = EditorGUI.Foldout( new Rect( position.x, position.y, position.width, 16.0f ), property.isExpanded, label );
		if( property.isExpanded )
		{
			++EditorGUI.indentLevel;

			SerializedProperty valuesArrayProperty = property.FindPropertyRelative( "m_internalArray" );
			SerializedProperty namesArrayProperty = property.FindPropertyRelative( "m_enumNames" );

			if (valuesArrayProperty != null && namesArrayProperty != null)
			{
				float fTop = position.y+16.0f;
				for (int nIndex = 0; nIndex < namesArrayProperty.arraySize; ++nIndex)
				{
					SerializedProperty elementProperty = valuesArrayProperty.GetArrayElementAtIndex( nIndex );
					SerializedProperty nameProperty = namesArrayProperty.GetArrayElementAtIndex( nIndex );

					if (elementProperty != null && nameProperty != null)
					{
						GUIContent elementLabel = new GUIContent( nameProperty.stringValue );

						float fPropertyHeight = EditorGUI.GetPropertyHeight( elementProperty, elementLabel );
						Rect elementRect = new Rect( position.x, fTop, position.width, fPropertyHeight );
						EditorGUI.PropertyField(elementRect, elementProperty, elementLabel, true);

						fTop += fPropertyHeight;
					}
				}
			}

			--EditorGUI.indentLevel;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		float fHeight = 16.0f;
		if( property.isExpanded )
		{
			SerializedProperty valuesArrayProperty = property.FindPropertyRelative( "m_internalArray" );
			SerializedProperty namesArrayProperty = property.FindPropertyRelative( "m_enumNames" );

			if (valuesArrayProperty != null && namesArrayProperty != null)
			{
				for (int nIndex = 0; nIndex < namesArrayProperty.arraySize; ++nIndex)
				{
					SerializedProperty value = valuesArrayProperty.GetArrayElementAtIndex( nIndex );
					SerializedProperty name = namesArrayProperty.GetArrayElementAtIndex( nIndex );
					if (value != null && name != null)
						fHeight += EditorGUI.GetPropertyHeight(value, new GUIContent(name.stringValue));
				}
			}
		}

		return fHeight;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditorInternal;
using IMGUI = UnityEngine.GUI;

[CustomPropertyDrawer(typeof(UnityEventBase), true)]
public class UnityEventDrawerFoldable : UnityEventDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (property.isExpanded)
		{
			return base.GetPropertyHeight(property, label);
		}
		else
		{
			return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		}
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var header = position;
		header.yMax = header.yMin + EditorGUIUtility.singleLineHeight;



		var propertyLabel = EditorGUI.BeginProperty(position, label, property);
		if (property.isExpanded)
		{
			base.OnGUI(position, property, label);
		}
		else
		{
			IMGUI.Box(header, "", ReorderableList.defaultBehaviours.headerBackground);

			var headerText = new Rect(header);
			headerText.y += 1;
			headerText.xMin += 6;
			IMGUI.Label(headerText, propertyLabel);
		}

		SerializedProperty elements = property.FindPropertyRelative("m_PersistentCalls.m_Calls");

		var typeStyle = EditorStyles.miniLabel;
		typeStyle.alignment = TextAnchor.MiddleRight;
		typeStyle.padding = new RectOffset(6, 6, 1, 1);
		IMGUI.Label(header, $"{elements.arraySize} Callbacks", typeStyle);

		property.isExpanded = EditorGUI.Foldout(header, property.isExpanded, "", true);
		EditorGUI.EndProperty();


	}
}



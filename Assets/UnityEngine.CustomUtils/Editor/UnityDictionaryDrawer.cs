using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(UnityDictionaryAttribute), false)]
[CustomPropertyDrawer(typeof(UnityDictionary<,>), false)]
public class UnityDictionaryDrawer : PropertyDrawer
{
	protected readonly UnityKeyValuePairDrawer _elementDrawer = new UnityKeyValuePairDrawer();
	protected readonly string _arrayPropName = "_serializedKeyValuePairs";

	protected string _activeArray = string.Empty;
	protected int    _activeIndex = -1;

	public UnityDictionaryDrawer()
	{
		_elementDrawer = new UnityKeyValuePairDrawer();
		_arrayPropName = "_serializedKeyValuePairs";
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float defaultHeight = EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
		float baseHeight = base.GetPropertyHeight(property, label);
		float foldoutHeight = 0;
		if (property.isExpanded)
		{
			SerializedProperty arrayProp = property.FindPropertyRelative(_arrayPropName);
			ReorderableList reorderableList = CreateReorderableList(arrayProp);
			foldoutHeight += reorderableList.GetHeight();
		}
		
		return baseHeight + foldoutHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		//Rect subPosition = EditorGUI.PrefixLabel(position, new GUIContent("Dictionary"));



		if (false)
		{
			EditorGUI.BeginProperty(position, label, property);
			//EditorGUI.DelayedTextField(position, property, label);
			//int controlID = GUIUtility.GetControlID(s_TextFieldHash, FocusType.Keyboard, position);
			//return DelayedTextFieldInternal(position, controlID, label, text, null, style);
			
			SerializedProperty arrayProp2 = property.Copy();
			arrayProp2.Next(true);
			
			EditorGUI.PropertyField(position, arrayProp2, label, true);
			EditorGUI.EndProperty();

			//property.serializedObject.ApplyModifiedProperties();
			return;
		}

		EditorGUI.BeginProperty(position, label, property);
		{
			Rect header = new Rect(position);
			header.height = base.GetPropertyHeight(property, label);

			bool wasExpanded = property.isExpanded;
			property.isExpanded = EditorGUI.Foldout(header, property.isExpanded, label, true);
			if (property.isExpanded)
			{
				Rect area = new Rect(position);
				area.yMin = header.yMax;

				SerializedProperty arrayProp = property.FindPropertyRelative(_arrayPropName);
				ReorderableList reorderableList = CreateReorderableList(arrayProp);


				//int oldIndex = reorderableList.index;
				reorderableList.DoList(area);
				//if (oldIndex != reorderableList.index)
				//{
				//	OnActive(arrayProp, reorderableList.index);
				//}
			}

			//EditorGUI.PropertyField(position, arrayProp, label, true);
		}
		EditorGUI.EndProperty();
	}


	protected virtual void OnActive(SerializedProperty arrayProp, int index)
	{
		_activeArray = arrayProp.propertyPath;
		_activeIndex = index;
	}

	protected virtual ReorderableList CreateReorderableList(SerializedProperty arrayProp)
	{
		int[] dummyArray = new int[arrayProp.arraySize];

		ReorderableList reorderableList = new ReorderableList(arrayProp.serializedObject, arrayProp, true, false, true, true);

		//reorderableList.elementHeight = GetElementHeight(arrayProp, 0);



		reorderableList.elementHeightCallback = (index) => GetElementHeight(arrayProp, index);

		reorderableList.drawElementCallback           = (rect, index, isActive, isFocused) => OnElementGUI       (arrayProp, rect, index, isActive, isFocused);
		//reorderableList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) => OnElementBackground(arrayProp, rect, index, isActive, isFocused);

		reorderableList.onAddDropdownCallback   = (buttonRect, list) => OnAddElementDropdown(arrayProp, buttonRect, list);

		//reorderableList.onAddCallback           = (list) => OnAddElement(arrayProp, list);
		//reorderableList.onRemoveCallback        = (list) => arrayProp.DeleteArrayElementAtIndex(reorderableList.index);
		reorderableList.drawNoneElementCallback = (rect) => OnListEmptyGUI(arrayProp, rect);
		reorderableList.onCanAddCallback        = (list) => true;
		//reorderableList.onCanRemoveCallback     = (list) => list.count != 0;
		reorderableList.onSelectCallback        = (list) => OnActive(arrayProp, list.index);

		if (arrayProp.propertyPath == _activeArray)
		{
			//reorderableList.GrabKeyboardFocus();
			if (_activeIndex >= 0 && _activeIndex < reorderableList.count)
			{
				reorderableList.index = _activeIndex;
			}
		}

		return reorderableList;
	}

	protected virtual void OnListEmptyGUI(SerializedProperty arrayProp, Rect rect)
	{
		Rect position = new Rect(rect);
		EditorGUI.LabelField(position, "Dictionary is empty");
	}

	protected virtual GUIContent GetElementLabel(SerializedProperty arrayProp, int index)
	{
		return new GUIContent($"Element {index}");
	}

	protected virtual float GetElementHeight(SerializedProperty arrayProp, int index)
	{
		SerializedProperty elementProp = arrayProp.GetArrayElementAtIndex(index);
		//elementProp.isExpanded = isActive;

		GUIContent label = GetElementLabel(arrayProp, index);

		return _elementDrawer.GetPropertyHeight(elementProp, label);
	}

	protected virtual void OnElementGUI(SerializedProperty arrayProp, Rect rect, int index, bool isActive, bool isFocused)
	{
		Rect position = new Rect(rect);
		position.xMin += ReorderableList.Defaults.dragHandleWidth;

		SerializedProperty elementProp = arrayProp.GetArrayElementAtIndex(index);
		//elementProp.isExpanded = isActive;

		GUIContent label = GetElementLabel(arrayProp, index);

		UnityDictionaryAttribute attr = attribute as UnityDictionaryAttribute;
		string keyText   = attr.key  ;
		string valueText = attr.value;

		_elementDrawer.OnGUI(position, elementProp, label, keyText, valueText);
	}

	protected virtual void OnElementBackground(SerializedProperty arrayProp, Rect rect, int index, bool isActive, bool isFocused)
	{
		//GUIStyle elementBackground = UnityEditor.UI
		//elementBackground.Draw(rect, isHover: false, selected, selected, focused);
	}

	protected virtual void OnAddElement(SerializedProperty arrayProp, ReorderableList reorderableList)
	{
		arrayProp.InsertArrayElementAtIndex(reorderableList.index);
		arrayProp.serializedObject.ApplyModifiedProperties();
	}

	protected virtual void OnAddElementDropdown(SerializedProperty arrayProp, Rect buttonRect, ReorderableList reorderableList)
	{
		// Create a copy of the object to edit so that validation does not purge the new property.
		UnityEngine.Object objectCopy = UnityEngine.Object.Instantiate(arrayProp.serializedObject.targetObject);
		SerializedObject serializedObjectCopy = new SerializedObject(objectCopy);
		SerializedProperty arrayPropCopy = serializedObjectCopy.FindProperty(arrayProp.propertyPath);


		int index = arrayPropCopy.arraySize++;
		Debug.Log($"{index} of {arrayPropCopy.arraySize}");
		SerializedProperty elementPropCopy = arrayPropCopy.GetArrayElementAtIndex(index);

		void OnFinishAdd()
		{
			arrayProp.arraySize = arrayPropCopy.arraySize;
			arrayProp.serializedObject.CopyFromSerializedProperty(elementPropCopy);
			arrayProp.serializedObject.ApplyModifiedProperties();

			UnityEngine.Object toDestroy = objectCopy;
			if (typeof(Component).IsAssignableFrom(objectCopy.GetType()))
			{
				toDestroy = (objectCopy as Component).gameObject;
			}
			Debug.Log($"OnFinishAdd: Destroy {toDestroy}");
			UnityEngine.Object.DestroyImmediate(toDestroy);
		}
		
		_elementDrawer.OnAddDropdown(buttonRect, elementPropCopy, OnFinishAdd);
	}
}

public class UnityKeyValuePairDrawer : PropertyDrawer
{
	public class AddPopupWindowContent : PopupWindowContent
	{
		public delegate void FinishAddCallback();

		public readonly UnityKeyValuePairDrawer Drawer;
		public readonly SerializedProperty Property;

		public FinishAddCallback OnFinishAddCallback;

		public AddPopupWindowContent(UnityKeyValuePairDrawer drawer, SerializedProperty property)
		{
			Drawer = drawer;
			Property = property;
		}

		public override void OnGUI(Rect rect)
		{
			Rect position = new Rect(rect);
			position.yMin += EditorGUIUtility.standardVerticalSpacing;
			position.yMax -= EditorGUIUtility.standardVerticalSpacing;
			position.xMin += EditorGUIUtility.standardVerticalSpacing;
			position.xMax -= EditorGUIUtility.standardVerticalSpacing;

			if (position.width < EditorGUIUtility.fieldWidth)
			{
				position.width = EditorGUIUtility.fieldWidth;
			}
			

			SerializedProperty keyProp = Property.FindPropertyRelative(Drawer._keyPropName);

			Rect line = new Rect(position);
			line.height = EditorGUI.GetPropertyHeight(keyProp, keyProp.isExpanded);

			float oldWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 30;
			EditorGUI.PropertyField(line, keyProp, keyProp.isExpanded);
			EditorGUIUtility.labelWidth = oldWidth;
		}

		public override void OnClose()
		{
			OnFinishAddCallback?.Invoke();
			base.OnClose();
		}
	}

	public readonly Type PairGenericType;

	protected readonly string _keyPropName;
	protected readonly string _valuePropName;

	public PropertyAttribute ContainerAttribute;
	public FieldInfo         ContainerFieldInfo;
	public Type              PairType;

	public UnityKeyValuePairDrawer()
	{
		PairGenericType = typeof(UnityKeyValuePair<,>);
		_keyPropName = "_key";
		_valuePropName = "_value";
	}



	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float defaultHeight = EditorGUI.GetPropertyHeight(property, label, true);
		float baseHeight = base.GetPropertyHeight(property, label);

		SerializedProperty keyProp   = property.FindPropertyRelative(_keyPropName);
		SerializedProperty valueProp = property.FindPropertyRelative(_valuePropName);
		SerializedProperty[] childProps = { keyProp, valueProp };

		float childrenHeight = 0;
		foreach (var childProp in childProps)
		{
			childrenHeight += EditorGUI.GetPropertyHeight(childProp);
		}

		return childrenHeight; // defaultHeight * (2.0f/3.0f); // - baseHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		GUIContent keyLabel   = new GUIContent("Key"  );
		GUIContent valueLabel = new GUIContent("Value");
		OnGUI(position, property, label, keyLabel, valueLabel);
	}

	public virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label, string keyText, string valueText)
	{
		GUIContent keyLabel   = new GUIContent(keyText  );
		GUIContent valueLabel = new GUIContent(valueText);
		OnGUI(position, property, label, keyLabel, valueLabel);
	}

	public virtual void OnGUI(Rect position, SerializedProperty property, GUIContent label, GUIContent keyLabel, GUIContent valueLabel)
	{
		Rect childrenRect = new Rect(position);

		SerializedProperty keyProp   = property.FindPropertyRelative(_keyPropName  );
		SerializedProperty valueProp = property.FindPropertyRelative(_valuePropName);
		SerializedProperty[] childProps = { keyProp, valueProp };
		GUIContent[] childLabels = { keyLabel, valueLabel };

		float y = childrenRect.yMin;
		for (int i=0; i < childProps.Length; i++)
		{
			var childProp = childProps[i];
			var childLabel = childLabels[i];

			Rect line = new Rect(childrenRect);
			line.y = y;
			line.height = EditorGUI.GetPropertyHeight(childProp);
			EditorGUI.PropertyField(line, childProp, childLabel, true);
			//EditorGUILayout.PropertyField(prop);
			y = line.yMax;
		}

		//EditorGUI.PropertyField(position, property, label, true);
	}

	public virtual void OnAddDropdown(Rect position, SerializedProperty property, AddPopupWindowContent.FinishAddCallback onFinishAddCallback)
	{
		AddPopupWindowContent content = new AddPopupWindowContent(this, property);
		content.OnFinishAddCallback = onFinishAddCallback;
		PopupWindow.Show(position, content);
	}

	

	protected Type GetPairType()
	{
		if (PairType != null)
		{
			return PairType;
		}
		
		Type pairType = null;

		//Type containerType = ContainerFieldInfo.FieldType.BaseType;
		//Type[] genericArgs = containerType.GetGenericArguments();
		//pairType = PairGenericType.MakeGenericType(genericArgs);

		return pairType;
	}

	protected Type GetKeyType()
	{
		return GetPairType().GetGenericArguments()[0];
	}
}
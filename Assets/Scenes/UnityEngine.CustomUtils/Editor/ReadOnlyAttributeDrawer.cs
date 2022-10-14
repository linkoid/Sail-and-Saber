using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyGroupDrawer : DecoratorDrawer
{
    public override float GetHeight()
    {
        return 0;
    }

    public override void OnGUI(Rect position)
    {
        // Make the field grayed out in the inspector
        EditorGUI.BeginDisabledGroup(true);
    }
}
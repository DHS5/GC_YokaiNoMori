using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class YokaiMovementGrid
{
    [SerializeField] private bool north;
    [SerializeField] private bool northEast;
    [SerializeField] private bool northWest;

    [SerializeField] private bool east;
    [SerializeField] private bool west;
    
    [SerializeField] private bool south;
    [SerializeField] private bool southEast;
    [SerializeField] private bool southWest;

    [SerializeField] private List<Vector2Int> validDeltas = new();

    public void ComputeValidDeltas(bool debug = false)
    {
        validDeltas.Clear();

        if (north) validDeltas.Add(new Vector2Int(0, 1));
        if (northEast) validDeltas.Add(new Vector2Int(1, 1));
        if (northWest) validDeltas.Add(new Vector2Int(-1, 1));

        if (east) validDeltas.Add(new Vector2Int(1, 0));
        if (west) validDeltas.Add(new Vector2Int(-1, 0));

        if (south) validDeltas.Add(new Vector2Int(0, -1));
        if (southEast) validDeltas.Add(new Vector2Int(1, -1));
        if (southWest) validDeltas.Add(new Vector2Int(-1, -1));

        if (debug)
        {
            if (validDeltas.Count == 0) Debug.Log("No deltas");
            else
            {
                StringBuilder sb = new();
                foreach (var delta in validDeltas)
                {
                    sb.AppendLine(delta.ToString());
                }
                Debug.Log(sb.ToString());
            }
        }
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(YokaiMovementGrid))]
public class YokaiMovementGridDrawer : PropertyDrawer
{
    SerializedProperty p_northEast;
    SerializedProperty p_north;
    SerializedProperty p_northWest;

    SerializedProperty p_west;
    SerializedProperty p_east;

    SerializedProperty p_southEast;
    SerializedProperty p_south;
    SerializedProperty p_southWest;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        p_northEast = property.FindPropertyRelative("northEast");
        p_north = property.FindPropertyRelative("north");
        p_northWest = property.FindPropertyRelative("northWest");

        p_west = property.FindPropertyRelative("west");
        p_east = property.FindPropertyRelative("east");

        p_southEast = property.FindPropertyRelative("southEast");
        p_south = property.FindPropertyRelative("south");
        p_southWest = property.FindPropertyRelative("southWest");

        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.LabelField(position, label);

        float baseX = position.x + EditorGUIUtility.labelWidth + 10f;
        Rect rect = new Rect(baseX, position.y, 20f, 20f);

        p_northWest.boolValue = EditorGUI.Toggle(rect, p_northWest.boolValue);
        rect.x += 20f;
        p_north.boolValue = EditorGUI.Toggle(rect, p_north.boolValue);
        rect.x += 20f;
        p_northEast.boolValue = EditorGUI.Toggle(rect, p_northEast.boolValue);

        rect.x = baseX;
        rect.y += 20f;

        p_west.boolValue = EditorGUI.Toggle(rect, p_west.boolValue);
        rect.x += 40f;
        p_east.boolValue = EditorGUI.Toggle(rect, p_east.boolValue);

        rect.x = baseX;
        rect.y += 20f;

        p_southWest.boolValue = EditorGUI.Toggle(rect, p_southWest.boolValue);
        rect.x += 20f;
        p_south.boolValue = EditorGUI.Toggle(rect, p_south.boolValue);
        rect.x += 20f;
        p_southEast.boolValue = EditorGUI.Toggle(rect, p_southEast.boolValue);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 60f;
    }
}
#endif

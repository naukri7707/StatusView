using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatusView), true)]
public class StatusViewDrawer : Editor
{
    private StatusView self;

    private Property<StatusView, float> Scale { get; set; }

    private Property<StatusView, int> Offset { get; set; }

    private void OnEnable()
    {
        self = target as StatusView;
        Scale = new Property<StatusView, float>("Radius", this);
        Offset = new Property<StatusView, int>("Offset", this);
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (LazyField(() => EditorGUILayout.FloatField("Radius", Scale.Value), out var scale))
        {
            Scale.Value = scale;
        }
        if (LazyField(() => EditorGUILayout.IntField("Offset", Offset.Value), out var offset))
        {
            Offset.Value = offset;
        }
        if (self.UnityEditorOnly_Abilities != null)
        {
            foreach (var (name, value, setter) in self.UnityEditorOnly_Abilities)
            {
                LazyField(
                    () => EditorGUILayout.IntSlider(name, value, self.Range.x, self.Range.y),
                    setter
                    );
            }
            self.UpdateStatus();
        }
    }

    bool LazyField<T>(Func<T> drawer, out T value)
    {
        EditorGUI.BeginChangeCheck();
        value = drawer();
        return EditorGUI.EndChangeCheck();
    }

    bool LazyField<T>(Func<T> drawer, Action<T> setter)
    {
        EditorGUI.BeginChangeCheck();
        var value = drawer();
        if (EditorGUI.EndChangeCheck())
        {
            setter(value);
            return true;
        }
        return false;
    }
}
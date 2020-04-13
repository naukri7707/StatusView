using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatusView), true)]
public class StatusViewDrawer : Editor
{
    private StatusView self;

    private void OnEnable()
    {
        self = target as StatusView;
    }

    private float spot;


    Rect[] SplitRect(Rect rectToSplit, int n)
    {
        Rect[] rects = new Rect[n];

        for (int i = 0; i < n; i++)
        {
            rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y, rectToSplit.width / n, rectToSplit.height);
        }

        int padding = (int)rects[0].width - 40;
        int space = 5;

        rects[0].width -= padding + space;
        rects[2].width -= padding + space;

        rects[1].x -= padding;
        rects[1].width += padding * 2;

        rects[2].x += padding + space;

        return rects;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (self.UnityEditorOnly_Abilities != null)
        {
            foreach (var (name, value, setter) in self.UnityEditorOnly_Abilities)
            {
                setter(EditorGUILayout.IntSlider(name, value, self.Range.x, self.Range.y));
            }
            self.UpdateStatus();
        }
    }
}

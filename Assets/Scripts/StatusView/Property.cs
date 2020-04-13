using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// fast reflaction

public class Property
{
    public static string FieldPrefix = "_";
}

public class Property<TTarget, TResult> : Property
    where TTarget : MonoBehaviour
{
    public event Func<TResult> OnGet;

    public event Action<TResult> OnSet;

    public TResult Value { get => OnGet(); set => OnSet(value); }

    public Property(string propertyName, Editor targetEditor)
        : this(propertyName, null, targetEditor) { }

    public Property(string propertyName, string fieldName, Editor targetEditor)
    {
        var target = targetEditor.target as TTarget;

        var type = target.GetType();

        var property = type.GetProperty(propertyName);

        var getter = Delegate.CreateDelegate(typeof(Func<TTarget, TResult>), property.GetGetMethod(true)) as Func<TTarget, TResult>;
        var setter = Delegate.CreateDelegate(typeof(Action<TTarget, TResult>), property.GetSetMethod(true)) as Action<TTarget, TResult>;
        // 
        OnGet += () => getter(target);
        OnSet += (v) => setter(target, v);

        var value = getter(target);

        fieldName = fieldName ?? $"{FieldPrefix}{char.ToLower(propertyName[0])}{propertyName.Substring(1)}";
        var field = targetEditor.serializedObject.FindProperty(fieldName);
        switch (value)
        {
            case int v:
                OnSet += _ => field.intValue = (int)(object)Value; // 避免 property.setter() 重設 value
                break;
            case float v:
                OnSet += _ => field.floatValue = (float)(object)Value;
                break;
            case string v:
                OnSet += _ => field.stringValue = (string)(object)Value;
                break;
            // TODO other type
            default:
                throw new Exception("不支援的型態");
        }
        OnSet += _ => targetEditor.serializedObject.ApplyModifiedProperties();
    }
}
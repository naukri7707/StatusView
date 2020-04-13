using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasRenderer))]
public class StatusView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text AbilityText;

    [SerializeField]
    private CanvasRenderer backgroundRenderer;

    [SerializeField]
    private CanvasRenderer chartRenderer;

    [SerializeField]
    private Material backgroundMaterial;

    [SerializeField]
    private Material chartMaterial;

    [SerializeField, HideInInspector]
    private float _radius = 10;

    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            OnResize?.Invoke();
        }
    }

    [SerializeField, HideInInspector]
    private int _offset = 5;

    public int Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            OnResize?.Invoke();
        }
    }

    [SerializeField, MinMaxSlider(0, 1000)]
    public Vector2Int Range = new Vector2Int(10, 100);

    private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private IEnumerable<int> AbilityValues;

#if UNITY_EDITOR
    public IEnumerable<(string name, int value, Action<int> setter)> UnityEditorOnly_Abilities;
#endif

    // [field: SerializeField]
    private int Count { get; set; }

    private float Angle { get; set; }

    private RectTransform[] textTransforms;

    public event Action OnResize;

    public void InitView<T>(T target)
    {
        // 初始化欄位資訊
        var sortedAbilities = new SortedList<long, (string name, PropertyInfo property)>();
        var type = target.GetType();
        var props = type.GetProperties(BINDING_FLAGS);
        long autoincrement = 0;
        foreach (var prop in props)
        {
            var ability = prop.GetCustomAttribute<AbilityAttribute>();
            if (ability != null)
            {
                if (prop.PropertyType != typeof(int))
                {
                    throw new Exception($"能力值變數必須為 int");
                }
                sortedAbilities.Add(
                    (long)ability.Order * 2147483646 + autoincrement++,
                    (ability.Name, prop)
                    );
            }
        }
        Count = sortedAbilities.Count;
        Angle = 360 / Count;
        var abilityNames = new string[Count];
        var abilityValueGetters = new Func<T, int>[Count];

        if (Count < 3)
        {
            throw new Exception($"能力值須超過 3 項才能正確顯示");
        }
        foreach (var (i, (name, prop)) in sortedAbilities.Values.WithIndex())
        {
            abilityNames[i] = name;
            abilityValueGetters[i] = Delegate.CreateDelegate(typeof(Func<T, int>), prop.GetGetMethod(true)) as Func<T, int>;
        }
        OnResize += SetBackgroundMesh;
        OnResize += UpdateStatus;
        OnResize += SetAbilityText;
        OnResize();
#if UNITY_EDITOR
        var abilityValueSetters = new Action<T, int>[Count];
        foreach (var (i, (name, prop)) in sortedAbilities.Values.WithIndex())
        {
            abilityValueSetters[i] = Delegate.CreateDelegate(typeof(Action<T, int>), prop.GetSetMethod(true)) as Action<T, int>;
        }
        //
        UnityEditorOnly_Abilities = unityEditorOnly_Abilities();
        IEnumerable<(string name, int value, Action<int> setter)> unityEditorOnly_Abilities()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return (
                    abilityNames[i],
                    abilityValueGetters[i](target),
                    (value) => abilityValueSetters[i](target, value)
                    );
            }
        }
#endif
        void SetAbilityText()
        {
            if (textTransforms is null)
            {
                var tmp = new List<RectTransform>();
                foreach (var (i, name) in abilityNames.WithIndex())
                {
                    var text = Instantiate(AbilityText, transform);
                    text.text = name;
                    var trans = text.GetComponent<RectTransform>();
                    trans.anchoredPosition = GetVeticesPos(Radius + Offset, i * Angle);
                    tmp.Add(trans);
                }
                textTransforms = tmp.ToArray();
            }
            else
            {
                foreach (var (i, trans) in textTransforms.WithIndex())
                {
                    trans.anchoredPosition = GetVeticesPos(Radius + Offset, i * Angle);
                }
            }
        }
        void SetBackgroundMesh()
        {
            var mesh = new Mesh();
            var vertices = new Vector3[361];
            for (int i = 1; i <= 360; i++)
            {
                vertices[i] = GetVeticesPos(Radius, i);
            }
            mesh.vertices = vertices;

            var triangles = new int[360 * 3];
            for (int i = 0; i < 360; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            triangles[360 * 3 - 1] = 1;
            mesh.triangles = triangles;
            backgroundRenderer.SetMesh(mesh);
            backgroundRenderer.SetMaterial(backgroundMaterial, null);
            //
            AbilityValues = abilityValue();
            IEnumerable<int> abilityValue()
            {
                foreach (var getter in abilityValueGetters)
                {
                    yield return getter(target);
                }
            }
        }
    }

    public void UpdateStatus()
    {
        var mesh = new Mesh();
        // set vertex
        var vertices = new Vector3[Count + 1];
        foreach (var (i, v) in AbilityValues.WithIndex())
        {
            vertices[i + 1] = GetVeticesPos(Radius * v / Range.y, i * Angle);
        }
        mesh.vertices = vertices;

        // set triangle
        var triangles = new int[Count * 3];
        for (int i = 0; i < Count; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }
        triangles[Count * 3 - 1] = 1;
        mesh.triangles = triangles;
        // set mesh
        chartRenderer.SetMesh(mesh);
        chartRenderer.SetMaterial(chartMaterial, null);
    }

    Vector2 GetVeticesPos(float value, float angle)
    {
        value *= Radius;
        if (angle == 0)
        {
            return new Vector2(0, value);
        }
        else
        {
            float radix = angle * Mathf.Deg2Rad;
            var res = new Vector2(
                Mathf.Sin(radix) * value,
                Mathf.Cos(radix) * value
                );
            return res;
        }
    }
}

public static class ExtensionMethod
{
    public static IEnumerable<(int index, T value)> WithIndex<T>(this IEnumerable<T> self)
    {
        int i = -1;
        foreach (var e in self)
        {
            yield return (++i, e);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Ability("STR")]
    public int Str { get; set; } = 57;

    [Ability("VIT", 1)]
    public int Vit { get; set; } = 63;

    [Ability("LUK", 9999)]
    public int Luk { get; set; } = 45;

    [Ability("INT", 3)]
    public int Int { get; set; } = 71;

    [Ability("DEX")]
    public int Dex { get; set; } = 31;

    [SerializeField]
    private StatusView statusView;

    //[Ability("AGI")]
    //public int Agi { get; set; } = 5;

    private void Start()
    {
        statusView.InitView(this);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Ability("STR")]
    public int Str { get; set; }

    [Ability("VIT", 1)]
    public int Vit { get; set; }

    [Ability("LUK", 9999)]
    public int Luk { get; set; }

    [Ability("INT", 3)]
    public int Int { get; set; }

    [Ability("DEX")]
    public int Dex { get; set; }

    [SerializeField]
    private StatusView statusView;

    //[Ability("AGI")]
    //public int Agi { get; set; } = 5;

    private void Start()
    {
        statusView.InitView(this);
    }
}
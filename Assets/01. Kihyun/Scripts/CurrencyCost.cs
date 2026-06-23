using System;
using UnityEngine;

[Serializable]
public struct CurrencyCost
{
    [Min(0)] public int Gear;
    [Min(0)] public int Scrap;

    public bool IsFree => Gear <= 0 && Scrap <= 0;
}
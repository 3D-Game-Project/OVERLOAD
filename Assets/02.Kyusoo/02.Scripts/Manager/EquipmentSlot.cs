using System;
using UnityEngine;

[Serializable]
public struct EquipmentSlot
{
    public string SlotName;       
    public PartsType EquipedType; 
    public Transform Anchor;      
}
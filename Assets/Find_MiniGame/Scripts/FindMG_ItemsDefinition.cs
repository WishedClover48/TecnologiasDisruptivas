using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Find Minigame/Items Definition")]
public class FindMG_ItemsDefinition : ScriptableObject
{
    public List<FindMG_ItemDefinition> items;
}

[Serializable] public struct FindMG_ItemDefinition
{
    public string itemName;
    public GameObject modelPrefab;
}

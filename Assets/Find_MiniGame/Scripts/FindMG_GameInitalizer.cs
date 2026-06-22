using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FindMG_GameInitalizer : MonoBehaviour
{
    [Serializable] private struct FindMG_StartingConfiguration
    {
        public List<FindMG_DropZone> dropZones;
        public List<FindMG_Item> possibleSpawnPoints;
        [Space]
        public FindMG_ItemsDefinition possibleItems;
        public int toDoLength;
        [Space] public FindMG_UI ui;
    }
    [SerializeField] private FindMG_StartingConfiguration config;
    [SerializeField] private bool startOnAwake = true;

    private List<FindMG_Item> toDoList;

    private void Start()
    {
        if (startOnAwake) BeginGame();
    }

    public void BeginGame()
    {
        var itemPool = new List<FindMG_ItemDefinition>(config.possibleItems.items);
        var spawnPoints = new List<FindMG_Item>(config.possibleSpawnPoints);
        
        var toDoLength = Mathf.Min(config.toDoLength, itemPool.Count, config.possibleSpawnPoints.Count);

        Shuffle(itemPool);
        Shuffle(spawnPoints);

        toDoList = new List<FindMG_Item>();

        for (var i = 0; i < toDoLength; i++)
        {
            spawnPoints[i].Init(itemPool[i], true);
            toDoList.Add(spawnPoints[i]);
        }

        for (var i = toDoLength; i < spawnPoints.Count; i++)
        {
            var filler = itemPool[Random.Range(0, itemPool.Count)];
            spawnPoints[i].Init(filler, false);
        }

        var gameManager = new FindMG_GameManager(toDoList, config.dropZones, config.ui);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}



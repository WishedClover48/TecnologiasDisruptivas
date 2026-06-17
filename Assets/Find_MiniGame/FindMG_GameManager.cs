using System;
using System.Collections.Generic;
using UnityEngine;

public class FindMG_GameManager
{
    public List<FindMG_Item> ToDoList { get; private set; }
    public List<FindMG_DropZone> DropZone { get; private set; }
    
    public event Action<FindMG_Item> OnItemCompleted;
    public event Action OnListCompleted;

    public FindMG_GameManager(List<FindMG_Item> items, List<FindMG_DropZone> zones, FindMG_UI ui)
    {
        ToDoList = items;
        DropZone = zones;

        ui.Init(this);

        foreach (var zone in DropZone)
        {
            zone.OnItemDropped += TestItem;
        }
    }

    private void TestItem(FindMG_Item item)
    {
        if (!item.ToDo) return;
        if (!ToDoList.Contains(item)) return;

        CorrectItem(item);
    }
    private void CorrectItem(FindMG_Item item)
    {
        ToDoList.Remove(item);
        OnItemCompleted?.Invoke(item);

        if (ToDoList.Count == 0)
            ListComplete();
    }
    private void ListComplete()
    {
        foreach (var zone in DropZone)
        {
            zone.OnItemDropped -= TestItem;
        }

        OnListCompleted?.Invoke();
    }
}

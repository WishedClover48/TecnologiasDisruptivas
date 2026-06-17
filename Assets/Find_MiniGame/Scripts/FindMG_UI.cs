using System.Collections.Generic;
using UnityEngine;

public class FindMG_UI : MonoBehaviour
{
    private FindMG_GameManager gameManager;

    public void Init(FindMG_GameManager manager)
    {
        gameManager = manager;
        FillList(gameManager.ToDoList);
        
        gameManager.OnItemCompleted += ItemCompleted;
        gameManager.OnListCompleted += ListCompleted;
    }

    private void FillList(List<FindMG_Item> items)
    {
        //Writes the list in the UI
    }

    private void ItemCompleted(FindMG_Item item)
    {
        //Crosses item of the list UI wise (Probably change color)
        Debug.Log("ItemCompleted");
    }

    private void ListCompleted()
    {
        //Hides List and shows Resume
        
        Debug.Log("ListCompleted");
        
        gameManager.OnItemCompleted -= ItemCompleted;
        gameManager.OnListCompleted -= ListCompleted;
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FindMG_UI : MonoBehaviour
{
    [SerializeField] private List<TMP_Text> _Text;
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
        for (int i = 0; i < items.Count; i++)
        {
            if (i < _Text.Count)
            {
                _Text[i].text = items[i].gameObject.name;
            }
        }
    }

    private void ItemCompleted(FindMG_Item item)
    {
        //Crosses item of the list UI wise (Probably change color)
        Debug.Log("ItemCompleted");
        var textElement = _Text.Find(x => x.text == item.gameObject.name);
        if (textElement != null)
        {
            textElement.color = Color.green;
        }
    }

    private void ListCompleted()
    {
        //Hides List and shows Resume
        Debug.Log("ListCompleted");
        
        gameManager.OnItemCompleted -= ItemCompleted;
        gameManager.OnListCompleted -= ListCompleted;
    }
}

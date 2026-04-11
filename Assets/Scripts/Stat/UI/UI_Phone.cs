using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Phone : MonoBehaviour
{
    [SerializeField] private TMP_Text day;
    [SerializeField] private TMP_Text time;
    [SerializeField] private TMP_Text money;
    [SerializeField] private TMP_Text stress;
    [SerializeField] private TMP_Text health;
    
    //The DAY should update everyTime a day finishes for the player, depending on how many days this "run" has.
    //"{currentDay} + "/" + {maxDays} days"
    
    //After each action the player takes he will waste X amount of hours doing it. Each day has an Y amount of hours.
    //Based on how many hours is left the time should update to represent a day from 9:00 to 00:00
}

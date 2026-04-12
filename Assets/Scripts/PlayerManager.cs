using System;
using UnityEngine;
using System.Collections.Generic;

namespace DefaultNamespace
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player Stats")]
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int currentStress = 0;
        [SerializeField] private int currentFinance = 100;
        [SerializeField] private int currentMoney = 1000;

        [Header("Selected Activities")]
        [SerializeField] private List<ActionData_SO> selectedActivities = new List<ActionData_SO>();
        GlobalEventSO<ActionData_SO> onActivityApplied;
        
        private DayTimeManager dayTimeManager;
        
        public int Health => currentHealth;
        public int Stress => currentStress;
        public int Finance => currentFinance;
        public int Money => currentMoney;
        
        public List<ActionData_SO> SelectedActivities => selectedActivities;

        #region  Singelton

        public static PlayerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        private void Start()
        {
            dayTimeManager=DayTimeManager.Instance;
        }


        public bool CanAffordActivity(ActionData_SO actionData)
        {
            return dayTimeManager.RemainingHours >= actionData.TimeCost && currentMoney >= actionData.MoneyCost;
        }

        public void ApplyActivity(ActionData_SO actionData)
        {
            if (!CanAffordActivity(actionData))
            {
                Debug.LogWarning($"Cannot afford activity: {actionData.ActivityName}");
                return;
            }

            // Subtract costs
            dayTimeManager.SpendTime(actionData.TimeCost);
            currentMoney -= actionData.MoneyCost;

            // Apply effects
            currentFinance += actionData.Finance;
            currentHealth += actionData.Health;
            currentStress += actionData.Stress;

            // Clamp values to reasonable ranges
            currentHealth = Mathf.Clamp(currentHealth, 0, 100);
            currentStress = Mathf.Clamp(currentStress, 0, 100);
            currentFinance = Mathf.Clamp(currentFinance, 0, 100);

            selectedActivities.Add(actionData);

            onActivityApplied.RaiseEvent(actionData );
            Debug.Log($"Applied activity: {actionData.ActivityName}. Stats - Finance: {currentFinance}, Health: {currentHealth}, Stress: {currentStress}");
        }

        public void RemoveActivity(ActionData_SO actionData)
        {
            if (!selectedActivities.Contains(actionData))
            {
                Debug.LogWarning($"Activity {actionData.ActivityName} was not selected");
                return;
            }
            selectedActivities.Remove(actionData);

            Debug.Log($"Removed activity: {actionData.ActivityName}");
        }

        public void SaveSelectedActivities()//copado pero no se si lo usamos
        {
            // Save to PlayerPrefs (simple saving solution)
            PlayerPrefs.SetInt("SelectedActivitiesCount", selectedActivities.Count);
            
            for (int i = 0; i < selectedActivities.Count; i++)
            {
                if (selectedActivities[i] != null)
                {
                    PlayerPrefs.SetString($"SelectedActivity_{i}", selectedActivities[i].name);
                }
            }

            // Save current stats
            PlayerPrefs.SetInt("PlayerFinance", currentFinance);
            PlayerPrefs.SetInt("PlayerHealth", currentHealth);
            PlayerPrefs.SetInt("PlayerStress", currentStress);
            PlayerPrefs.SetInt("PlayerMoney", currentMoney);

            PlayerPrefs.Save();
            Debug.Log("Selected activities and stats saved!");
        }

        public void LoadSelectedActivities()//copado pero no se si lo usamos
        {
            int count = PlayerPrefs.GetInt("SelectedActivitiesCount", 0);
            selectedActivities.Clear();

            for (int i = 0; i < count; i++)
            {
                string activityName = PlayerPrefs.GetString($"SelectedActivity_{i}", "");
                if (!string.IsNullOrEmpty(activityName))
                {
                    // You'll need to load the ActivitySO by name from Resources or another method
                    ActionData_SO actionData = Resources.Load<ActionData_SO>($"Activities/{activityName}");
                    if (actionData != null)
                    {
                        selectedActivities.Add(actionData);
                    }
                }
            }

            // Load stats
            currentFinance = PlayerPrefs.GetInt("PlayerFinance", 100);
            currentHealth = PlayerPrefs.GetInt("PlayerHealth", 100);
            currentStress = PlayerPrefs.GetInt("PlayerStress", 0);
            currentMoney = PlayerPrefs.GetInt("PlayerMoney", 1000);

            Debug.Log("Selected activities and stats loaded!");
        }
    }
}
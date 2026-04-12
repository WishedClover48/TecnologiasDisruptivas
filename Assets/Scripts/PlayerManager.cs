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
        [SerializeField] private int currentTime = 12;

        [Header("Selected Activities")]
        [SerializeField] private List<ActivitySO> selectedActivities = new List<ActivitySO>();

        // Properties for accessing stats
        public int Health => currentHealth;
        public int Stress => currentStress;
        public int Finance => currentFinance;
        public int Money => currentMoney;
        public int Time => currentTime;
        public List<ActivitySO> SelectedActivities => selectedActivities;

        // Singleton pattern for easy access
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

        public bool CanAffordActivity(ActivitySO activity)
        {
            return currentTime >= activity.TimeCost && currentMoney >= activity.MoneyCost;
        }

        public void ApplyActivity(ActivitySO activity)
        {
            if (!CanAffordActivity(activity))
            {
                Debug.LogWarning($"Cannot afford activity: {activity.ActivityName}");
                return;
            }

            // Subtract costs
            currentTime -= activity.TimeCost;
            currentMoney -= activity.MoneyCost;

            // Apply effects
            currentFinance += activity.Finance;
            currentHealth += activity.Health;
            currentStress += activity.Stress;

            // Clamp values to reasonable ranges
            currentHealth = Mathf.Clamp(currentHealth, 0, 100);
            currentStress = Mathf.Clamp(currentStress, 0, 100);
            currentFinance = Mathf.Clamp(currentFinance, 0, 100);

            selectedActivities.Add(activity);

            Debug.Log($"Applied activity: {activity.ActivityName}. Stats - Finance: {currentFinance}, Health: {currentHealth}, Stress: {currentStress}");
        }

        public void RemoveActivity(ActivitySO activity)
        {
            if (!selectedActivities.Contains(activity))
            {
                Debug.LogWarning($"Activity {activity.ActivityName} was not selected");
                return;
            }
            selectedActivities.Remove(activity);

            Debug.Log($"Removed activity: {activity.ActivityName}. Stats - Finance: {currentFinance}, Health: {currentHealth}, Stress: {currentStress}");
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
            PlayerPrefs.SetInt("PlayerTime", currentTime);
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
                    ActivitySO activity = Resources.Load<ActivitySO>($"Activities/{activityName}");
                    if (activity != null)
                    {
                        selectedActivities.Add(activity);
                    }
                }
            }

            // Load stats
            currentFinance = PlayerPrefs.GetInt("PlayerFinance", 100);
            currentHealth = PlayerPrefs.GetInt("PlayerHealth", 100);
            currentStress = PlayerPrefs.GetInt("PlayerStress", 0);
            currentTime = PlayerPrefs.GetInt("PlayerTime", 480);
            currentMoney = PlayerPrefs.GetInt("PlayerMoney", 1000);

            Debug.Log("Selected activities and stats loaded!");
        }
    }
}
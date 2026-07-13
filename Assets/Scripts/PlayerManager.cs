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
        [SerializeField] private int currentMoney = 1000;

        [Header("Selected Activities")]
        [SerializeField] private List<ActionData_SO> selectedActivities = new List<ActionData_SO>();
        [SerializeField] private GlobalEventSO<ActionData_SO> onActivityApplied;

        [Space]
        [SerializeField] private SceneTransition sceneTransition;

        public bool tutorialCompleted;
        
        private DayTimeManager dayTimeManager;
        private ActionData_SO currentActivity;

        private int initialHealth, initialStress, initialMoney;

        public int Health => currentHealth;
        public int Stress => currentStress;
        public int Money => currentMoney;
        public List<ActionData_SO> SelectedActivities => selectedActivities;
        public SceneTransition SceneTransition => sceneTransition;

        #region  Singelton

        public static PlayerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                initialHealth  = currentHealth;
                initialStress  = currentStress;
                initialMoney   = currentMoney;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        public void ResetState()
        {
            currentHealth  = initialHealth;
            currentStress  = initialStress;
            currentMoney   = initialMoney;
            currentActivity = null;
            selectedActivities.Clear();
        }

        private void Start()
        {
            dayTimeManager=DayTimeManager.Instance;
            tutorialCompleted = true;
        }
        public ActionData_SO CurrentActivity {  get => currentActivity; set => currentActivity = value; }

        public bool CanAffordActivity(ActionData_SO actionData)
        {
            if (!(dayTimeManager.RemainingHours >= actionData.TimeCost))
            {
                Debug.Log($"Cannot afford activity: TIME");
            }
            if (!(currentMoney >= actionData.MoneyCost))
            {
                Debug.Log($"Cannot afford activity: MONEY");
            }
            return dayTimeManager.RemainingHours >= actionData.TimeCost && currentMoney >= actionData.MoneyCost;
        }

        public void ApplyActivity(ActionData_SO actionData)
        {
            if (!CanAffordActivity(actionData))
            {
                Debug.LogWarning($"Cannot afford activity: {actionData.ActivityName}");
                return;
            }

            currentMoney -= actionData.MoneyCost;

            currentHealth += actionData.Health;
            currentStress += actionData.Stress;

            currentHealth = Mathf.Clamp(currentHealth, 0, 100);
            currentStress = Mathf.Clamp(currentStress, 0, 100);

            selectedActivities.Add(actionData);

            onActivityApplied.RaiseEvent(actionData);
            Debug.Log($"Applied activity: {actionData.ActivityName}. Stats - Health: {currentHealth}, Stress: {currentStress}, Money: {currentMoney}");
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
            currentHealth = PlayerPrefs.GetInt("PlayerHealth", 100);
            currentStress = PlayerPrefs.GetInt("PlayerStress", 0);
            currentMoney = PlayerPrefs.GetInt("PlayerMoney", 1000);

            Debug.Log("Selected activities and stats loaded!");
        }
    }
}
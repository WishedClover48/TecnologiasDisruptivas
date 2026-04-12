using UnityEngine;
using DefaultNamespace;

public class GameManager : MonoBehaviour
{
    [Header("Auto Save Settings")]
    [SerializeField] private bool autoSave = true;
    [SerializeField] private float autoSaveInterval = 30f; // Save every 30 seconds
    
    private float autoSaveTimer = 0f;

    private void Start()
    {
        // Load activities when the game starts
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.LoadSelectedActivities();
        }
    }

    private void Update()
    {
        if (autoSave)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                SaveGame();
                autoSaveTimer = 0f;
            }
        }
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SaveSelectedActivities();
        }
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.LoadSelectedActivities();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGame();
    }
}

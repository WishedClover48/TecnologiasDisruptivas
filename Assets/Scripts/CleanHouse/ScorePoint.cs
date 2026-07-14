using DefaultNamespace;
using System.Collections.Generic;
using UnityEngine;

public class ScorePoint : MonoBehaviour 
{
    [SerializeField] private List<Transform> _objectsToScore;
    [SerializeField] private int _scoreObjective;

    private int _currentScore = 0;

    private void Awake()
    {
        _currentScore = 0;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_objectsToScore.Contains(other.transform))
        {
            Score(other.gameObject);
        }
    }

    private void Score(GameObject obj)
    {
        _currentScore++;
        _objectsToScore.Remove(obj.transform);
        obj.SetActive(false);
        if (_currentScore >= _scoreObjective)
        {
            PlayerManager.Instance.SceneTransition.TransitionFromActivity("SampleScene");
        }
    }
}
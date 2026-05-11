using UnityEngine;

public class TutorialFlow : MonoBehaviour
{
    [SerializeField] private WelcomeTutorial welcomeTutorial;
    [SerializeField] private ActivityTutorial activityTutorial;
    [SerializeField] private PhoneTutorial phoneTutorial;

    private void Start()
    {
        welcomeTutorial.onComplete = OnWelcomeComplete;
        welcomeTutorial.Activate();
    }

    [ContextMenu("Next")]
    private void OnWelcomeComplete()
    {
        activityTutorial.Activate();
        phoneTutorial.Activate();
    }
}

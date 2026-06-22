using UnityEngine;

public class TutorialFlow : MonoBehaviour
{
    [SerializeField] private WelcomeTutorial welcomeTutorial;
    [SerializeField] private ActivityTutorial activityTutorial;
    [SerializeField] private PhoneTutorial phoneTutorial;

    private static bool hasShownWelcome;

    private void Start()
    {
        if (hasShownWelcome)
        {
            OnWelcomeComplete();
            return;
        }

        hasShownWelcome = true;
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

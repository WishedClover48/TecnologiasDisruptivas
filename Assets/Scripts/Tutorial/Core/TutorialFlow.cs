using DefaultNamespace;
using UnityEngine;

public class TutorialFlow : MonoBehaviour
{
    [SerializeField] private WelcomeTutorial welcomeTutorial;
    [SerializeField] private ActivityTutorial activityTutorial;
    [SerializeField] private PhoneTutorial phoneTutorial;

    public static bool hasShownWelcome;

    private void Awake()
    {
        if(PlayerManager.Instance.tutorialCompleted)
            return;
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

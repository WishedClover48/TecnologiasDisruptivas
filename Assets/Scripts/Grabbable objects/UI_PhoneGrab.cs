using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// Diegetic phone behaviour: the screen canvas is only visible while the
/// player is actively holding the phone. Attach to the phone root GameObject
/// (the one with a GrabInteractable component).
/// </summary>
[RequireComponent(typeof(GrabInteractable))]
public class UI_PhoneGrab : MonoBehaviour
{
    [Tooltip("The World-Space canvas that renders the phone screen.")]
    [SerializeField] private Canvas phoneScreen;

    [Tooltip("If true, screen is hidden until the player grabs the phone.")]
    [SerializeField] private bool hideWhenNotGrabbed = true;

    private GrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<GrabInteractable>();
    }

    private void Start()
    {
        grab.WhenStateChanged += HandleStateChange;
        SetScreenVisible(!hideWhenNotGrabbed);
    }

    private void OnDestroy()
    {
        if (grab != null)
            grab.WhenStateChanged -= HandleStateChange;
    }

    private void HandleStateChange(InteractableStateChangeArgs args)
    {
        bool grabbed = args.NewState == InteractableState.Select;
        SetScreenVisible(!hideWhenNotGrabbed || grabbed);
    }

    private void SetScreenVisible(bool visible)
    {
        if (phoneScreen != null)
            phoneScreen.gameObject.SetActive(visible);
    }
}

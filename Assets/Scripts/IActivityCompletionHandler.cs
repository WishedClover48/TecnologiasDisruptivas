public interface IActivityCompletionHandler
{
    bool IsHandlingActivity { get; }
    bool HandleActivityCompleted(Activity activity);
}

namespace EasyGamesStore.Models
{
    // This class is used to represent error details in the application.
    // It is mainly passed to the Error view to display information about what went wrong.
    public class ErrorViewModel
    {
        // The unique ID of the request that caused the error.
        // This is useful for debugging and tracing issues in logs or monitoring systems.
        public string? RequestId { get; set; }

        // This property determines whether the RequestId should be shown in the view.
        // It returns true only if RequestId is not null or empty.
        // This way, the view can decide whether to display the request ID to the user.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

namespace FIlmPicker.Models
{
    public class StatusMessage
    {
        public StatusMessageType Type { get; private set; }
        public string Message { get; private set; }

        public StatusMessage(StatusMessageType type, string message)
        {
            Type = type;
            Message = message;
        }
    }
}

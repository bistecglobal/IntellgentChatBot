using System;
namespace QnABot.Models
{
    public enum ChatStep
    {
        UserInformation = 1,
        ChatInformation = 2,
        UserFeedback = 3,
        SupportTicket = 4,
        None
    }

    public enum InputType
    {
        Text = 1,
        Value = 2
    }

    public class StepInformation
    {

        public StepInformation()
        {
            Step = ChatStep.None;
            UserInputType = InputType.Value;
        }

        public ChatStep Step { get; set; }

        public InputType UserInputType { get; set; }
    }

}

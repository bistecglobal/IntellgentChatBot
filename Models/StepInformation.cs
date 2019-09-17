using System;
namespace QnABot.Models
{
    public enum ChatStep
    {
        UserInformation = 1,
        ChatInformation = 2,
        SupportTicket = 3,
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

using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models
{
    public class UpdateResult 
    {
        public Guid ID { get; set; }
        public Guid UpdateInstructionID { get; set; }
        public bool IsSuccess { get; set; }
        public List<UpdateResultMessage> Messages { get; set; }

        public long TimeSpentMilliseconds { get; set; }

        public UpdateResult()
        {
            this.Messages = new List<UpdateResultMessage>();
        }

        public void AddMessage(string message, UpdateResultMessage.eMessageType type, DateTime? dateTime = null)
        {
            this.Messages.Add(new UpdateResultMessage()
            {
                Type = type,
                Message = message,
                DateTime = dateTime.HasValue ? dateTime.Value : DateTime.Now
            });
        }
    }

    public class UpdateResultMessage
    {
        public enum eMessageType
        {
            SUCCESS,
            ERROR,
            INFORMATION
        };

        public eMessageType Type {get;set;}

        public DateTime DateTime { get; set; }

        public string Message { get; set; }
    }
}
 
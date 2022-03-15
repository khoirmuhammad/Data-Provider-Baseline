using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Messages
{
    public class MessageResult<T> : MessageStatus
    {
        public MessageResult()
        {
        }

        public MessageResult(bool isSuccess, string message, string additionalInformation = "")
            : base(isSuccess, message, additionalInformation)
        {
        }

        public MessageResult(MessageStatus messageStatus)
        {
            this.IsSuccess = messageStatus.IsSuccess;
            this.Message = messageStatus.Message;
            this.AdditionalInformation = messageStatus.AdditionalInformation;
        }

        public T Result { get; set; }
    }
}

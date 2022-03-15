using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Messages
{
    public class MessageStatus
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string AdditionalInformation { get; set; }
        public int RowsAffected { get; set; }
        public IDictionary<string, object> OutputParameters { get; set; }

        public MessageStatus()
        {
        }

        public MessageStatus(bool isSuccess, string message, string additionalInformation = "")
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
            this.AdditionalInformation = additionalInformation;
        }

        public override string ToString()
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(this.AdditionalInformation))
                result = string.Format("{1} -- {2}", this.Message, this.AdditionalInformation);
            else
                result = string.Format("{1}", this.Message);

            return result;
        }
    }
}

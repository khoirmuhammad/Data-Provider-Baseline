using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Messages
{
    public class BaseParameterStatus
    {
        public string IsSuccessParameter { get; set; }
        public string MessageParameter { get; set; }
        public string AdditionalInformationParameter { get; set; }

        public BaseParameterStatus()
        {

        }

        public BaseParameterStatus(string isSuccessParameter, string messageParameter, string additionalInformationParameter)
        {
            IsSuccessParameter = isSuccessParameter;
            MessageParameter = messageParameter;
            AdditionalInformationParameter = additionalInformationParameter;
        }
    }
}

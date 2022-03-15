using DataProvider.Messages;
using DataProvider.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Command
{
    public class GenericCommand
    {
        public string CommandText { get; set; }
        public bool DoesUseTransaction { get; set; }
        public BaseParameterStatus ParameterStatus { get; set; }
        public IList<InputParameter> InputParameters { get; set; }
        public IList<OutputParameter> OutputParameters { get; set; }

        public GenericCommand()
        {

        }

        public GenericCommand(string commandText, bool doesUseTransaction = true, BaseParameterStatus parameterStatus = null, IList<InputParameter> inputParameters = null, IList<OutputParameter> outputParameters = null)
        {
            CommandText = commandText;
            DoesUseTransaction = doesUseTransaction;
            ParameterStatus = parameterStatus;
            InputParameters = inputParameters;
            OutputParameters = outputParameters;
        }
    }
}

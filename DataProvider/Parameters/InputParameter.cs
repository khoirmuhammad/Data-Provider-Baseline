using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Parameters
{
    public class InputParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public InputParameter()
        {
        }

        public InputParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}

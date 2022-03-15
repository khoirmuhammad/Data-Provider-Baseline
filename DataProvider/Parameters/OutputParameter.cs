using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Parameters
{
    public class OutputParameter
    {
        public string Name { get; set; }
        public SqlDbType SqlDbType { get; set; }
        public int Size { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public object Value { get; set; }

        public OutputParameter()
        {
        }

        public OutputParameter(string name, SqlDbType sqlDbType, int size, byte precision, byte scale, object value)
        {
            this.Name = name;
            this.SqlDbType = sqlDbType;
            this.Size = size;
            this.Precision = precision;
            this.Scale = scale;
            this.Value = value;
        }

        public OutputParameter(string name, SqlDbType sqlDbType)
            : this(name, sqlDbType, 0, 0, 0, (object)DBNull.Value)
        {
        }

        public OutputParameter(string name, SqlDbType sqlDbType, int size)
            : this(name, sqlDbType, size, 0, 0, (object)DBNull.Value)
        {
        }

        public OutputParameter(string name, SqlDbType sqlDbType, int size, object value)
            : this(name, sqlDbType, size, 0, 0, value)
        {
        }

        public OutputParameter(string parameterName, SqlDbType sqlDbType, byte precision, byte scale, object value)
            : this(parameterName, sqlDbType, 0, precision, scale, value)
        {
        }
    }
}

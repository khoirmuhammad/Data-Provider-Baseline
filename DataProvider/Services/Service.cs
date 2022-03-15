using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProvider.Command;
using DataProvider.Data;
using DataProvider.Messages;

namespace DataProvider.Services
{
    public class Service
    {
        DatabaseManager db = new DatabaseManager();
        public object Get(GenericCommand command)
        {
            return db.Get(command.CommandText, command.InputParameters);
        }

        public DataTable Select(GenericCommand command)
        {
            return db.Select(command.CommandText, command.InputParameters);
        }

        public MessageStatus Save(GenericCommand command)
        {
            return db.Save(command.CommandText, command.InputParameters, command.OutputParameters, command.ParameterStatus, command.DoesUseTransaction);
        }

        public MessageStatus Delete(GenericCommand command)
        {
            return db.Save(command.CommandText, command.InputParameters, command.OutputParameters, command.ParameterStatus, command.DoesUseTransaction);
        }

        public MessageResult<DataTable> ReturnDataTable(GenericCommand command)
        {
            return db.ReturnDataTable(command.CommandText, command.InputParameters, command.OutputParameters, command.ParameterStatus, command.DoesUseTransaction);
        }

        
    }
}

using DataProvider.Command;
using DataProvider.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProvider.Services;
using System.Data;
using DataProvider.Messages;

namespace DataProvider
{
    public class Client
    {
        Service service = new Service();

        /*
         * Service Get : 
         * 1. It will take single data either string, integer, boolean etc. It's according to data we select
         * 2. we simply put a convert mechanism to get apropriate value
         */
        public string GetUserNameById(Guid id)
        {
            try
            {
                string result = string.Empty;

                var inputParameters = new List<InputParameter>();
                inputParameters.Add(new InputParameter("@id", id));

                GenericCommand command = new GenericCommand("GetUserNameById", inputParameters: inputParameters);

                result = Convert.ToString(service.Get(command));

                return result;
            }
            catch(Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /*
         * Service Select :
         * 1. It will take several data from column. It can be single row or more again.
         * 2. Here we give an example how to access store procedure without input parameter
         * 3. The result will be datatable
         */
        public string SelectUserData()
        {
            try
            {
                string result = string.Empty;

                GenericCommand command = new GenericCommand("GetUserData");

                DataTable dt = service.Select(command);

                foreach (DataRow dr in dt.Rows)
                {
                    result = $"{Convert.ToString(dr["Name"])} - {Convert.ToString(dr["Address"])}";
                }

                return result;
            }
            catch(Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /*
         * Service Save :
         * 1. Only pass explicit commandText and inputParameter
         * 2. By defualt we have activated transaction SQL feature, since the doesUseTransaction is set to true
         */
        public string SaveUser(Guid? id, string name, string address)
        {
            try
            {
                var inputParameters = new List<InputParameter>();

                inputParameters.Add(new InputParameter("@id", id));
                inputParameters.Add(new InputParameter("@name", name));
                inputParameters.Add(new InputParameter("@address", address));

                var command = new GenericCommand("SaveUser", inputParameters: inputParameters);

                MessageStatus msg = service.Save(command);

                return $"{msg.Message} - {msg.AdditionalInformation}";
            }
            catch(Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /*
         * Service Save :
         * 1. Pass explicit commandText, useTransaction, inputParameter and outputParameter
         */
        public string InsertUser(string name, string address)
        {
            try
            {
                var inputParameters = new List<InputParameter>();

                inputParameters.Add(new InputParameter("@name", name));
                inputParameters.Add(new InputParameter("@address", address));

                var outputParameters = new List<OutputParameter>();
                outputParameters.Add(new OutputParameter("@id", SqlDbType.UniqueIdentifier));

                var command = new GenericCommand("InsertUser", doesUseTransaction: false, inputParameters: inputParameters, outputParameters: outputParameters);

                MessageStatus msg = service.Save(command);

                return msg.IsSuccess ? Convert.ToString(msg.OutputParameters["@id"]) : string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        /*
         * Return Data Table :
         * 1. We can use to query only or execute non query such as insert and update data which is the result contains datatable
         */
        public string SelectUserInfo()
        {
            try
            {
                string result = string.Empty;

                GenericCommand command = new GenericCommand("SelectUserData");

                var msg = service.ReturnDataTable(command);

                DataTable dt = msg.Result;

                foreach(DataRow dr in dt.Rows)
                {
                    result = $"{Convert.ToString(dr["id"])} - {Convert.ToString(dr["name"])} - {Convert.ToString(dr["address"])}";
                }

                return result;
            }
            catch(Exception ex)
            {
                return ex.Message.ToString();
            }
        }
    }
}

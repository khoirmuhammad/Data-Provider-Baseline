using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProvider.Parameters;
using DataProvider.Messages;

namespace DataProvider.Data
{
    public class DatabaseManager
    {
        private string _connectionString = Common.CommonSetting.ConnectionString;

        #region SQL Base
        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private void CloseConnection(IDbConnection connection)
        {
            var sqlConnection = (SqlConnection)connection;
            sqlConnection.Close();
            sqlConnection.Dispose();
        }

        private IDbCommand CreateCommand(string commandText, CommandType commandType, IDbConnection connection)
        {
            return new SqlCommand
            {
                CommandText = commandText,
                Connection = (SqlConnection)connection,
                CommandType = commandType
            };
        }

        private IDbCommand CreateCommand(string commandText, CommandType commandType, IDbConnection connection, IDbTransaction transaction)
        {
            return new SqlCommand
            {
                CommandText = commandText,
                CommandType = commandType,
                Connection = (SqlConnection)connection,
                Transaction = (SqlTransaction)transaction
            };
        }

        private IDataAdapter CreateAdapter(IDbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
        }
        #endregion

        #region SQL Base DML & Query

        private DataTable GetDataTable(string commandText, CommandType commandType, IList<IDbDataParameter> parameters = null)
        {
            using (var connection = this.CreateConnection())
            {
                connection.Open();

                using (var command = this.CreateCommand(commandText, commandType, connection))
                {
                    if (parameters != null)
                        foreach (var parameter in parameters)
                            command.Parameters.Add(parameter);

                    var dataset = new DataSet();
                    var dataAdaper = this.CreateAdapter(command);
                    dataAdaper.Fill(dataset);

                    return dataset.Tables.Count > 0 ? dataset.Tables[0] : null;
                }
            }
        }

        private DataSet GetDataSet(string commandText, IList<IDbDataParameter> parameters = null)
        {
            using (var connection = this.CreateConnection())
            {
                connection.Open();

                using (var command = this.CreateCommand(commandText, CommandType.StoredProcedure, connection))
                {
                    if (parameters != null)
                        foreach (var parameter in parameters)
                            command.Parameters.Add(parameter);

                    var dataset = new DataSet();
                    var dataAdaper = this.CreateAdapter(command);
                    dataAdaper.Fill(dataset);

                    return dataset;
                }
            }
        }

        private IDataReader GetDataReader(string commandText, IList<IDbDataParameter> parameters, out IDbConnection connection)
        {
            IDataReader reader = null;
            connection = this.CreateConnection();
            connection.Open();

            var command = this.CreateCommand(commandText, CommandType.StoredProcedure, connection);
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            reader = command.ExecuteReader();

            return reader;
        }

        private object GetScalarValue(string commandText, CommandType commandType, IList<IDbDataParameter> parameters = null)
        {
            try
            {
                using (var connection = this.CreateConnection())
                {
                    connection.Open();

                    using (var command = this.CreateCommand(commandText, commandType, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }

                        return command.ExecuteScalar();
                    }
                }
            }
            catch(SqlException ex)
            {
                throw ex;
            }
        }

        private MessageStatus ExecuteNonQuery(string commandText, IList<IDbDataParameter> parameters, bool doesUseTransaction = true, BaseParameterStatus parameterStatus = null)
        {
            var messageStatus = new MessageStatus();
            IDbTransaction transactionScope = null;
            SqlParameter param = null;
            Dictionary<string, object> outputParams = new Dictionary<string, object>();

            using (var connection = this.CreateConnection())
            {
                connection.Open();
                transactionScope = doesUseTransaction ? connection.BeginTransaction() : null;

                using (var command = this.CreateCommand(commandText, CommandType.StoredProcedure, connection, transactionScope))
                {
                    try
                    {
                        if (parameters != null)
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);

                        // Output params
                        if (parameterStatus == null)
                            parameterStatus = new BaseParameterStatus("@isSuccess", "@message", "@additionalInformation");

                        if (!string.IsNullOrEmpty(parameterStatus.IsSuccessParameter))
                        {
                            param = new SqlParameter(parameterStatus.IsSuccessParameter, SqlDbType.Bit);
                            param.Direction = ParameterDirection.Output;
                            command.Parameters.Add(param);
                        }

                        if (!string.IsNullOrEmpty(parameterStatus.MessageParameter))
                        {
                            param = new SqlParameter(parameterStatus.MessageParameter, SqlDbType.NVarChar, 1000);
                            param.Direction = ParameterDirection.Output;
                            command.Parameters.Add(param);
                        }

                        if (!string.IsNullOrEmpty(parameterStatus.AdditionalInformationParameter))
                        {
                            param = new SqlParameter(parameterStatus.AdditionalInformationParameter, SqlDbType.NVarChar, 1000);
                            param.Direction = ParameterDirection.Output;
                            command.Parameters.Add(param);
                        }

                        messageStatus.RowsAffected = command.ExecuteNonQuery();

                        foreach (var item in command.Parameters)
                        {
                            var parameter = (IDataParameter)item;
                            if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output) &&
                                (!parameter.ParameterName.Equals(parameterStatus.IsSuccessParameter) || !parameter.ParameterName.Equals(parameterStatus.MessageParameter) ||
                                !parameter.ParameterName.Equals(parameterStatus.AdditionalInformationParameter)))
                                outputParams.Add(parameter.ParameterName, parameter.Value);
                        }

                        messageStatus.IsSuccess = command.Parameters.Contains(parameterStatus.IsSuccessParameter) ? (bool)((IDataParameter)command.Parameters[parameterStatus.IsSuccessParameter]).Value : true;

                        messageStatus.Message = command.Parameters.Contains(parameterStatus.MessageParameter) && ((IDataParameter)command.Parameters[parameterStatus.MessageParameter]).Value != DBNull.Value ? ((IDataParameter)command.Parameters[parameterStatus.MessageParameter]).Value.ToString() : string.Empty;

                        messageStatus.AdditionalInformation = command.Parameters.Contains(parameterStatus.AdditionalInformationParameter) && ((IDataParameter)command.Parameters[parameterStatus.AdditionalInformationParameter]).Value != DBNull.Value ? ((IDataParameter)command.Parameters[parameterStatus.AdditionalInformationParameter]).Value.ToString() : string.Empty;
                        messageStatus.OutputParameters = outputParams;

                        if (transactionScope != null)
                        {
                            if (messageStatus.IsSuccess)
                                transactionScope.Commit();
                            else
                                transactionScope.Rollback();
                        }

                    }
                    catch (Exception ex)
                    {
                        if (transactionScope != null)
                            transactionScope.Rollback();

                        messageStatus.IsSuccess = false;
                        messageStatus.Message = ex.Message;
                        messageStatus.AdditionalInformation = ex.ToString();
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return messageStatus;
        }

        private MessageResult<DataTable> ExecuteDataTable(string commandText, IList<IDbDataParameter> parameters, bool doesUseTransaction = true, BaseParameterStatus parameterStatus = null)
        {
            var messageResult = new MessageResult<DataTable>();
            DataTable dataTable = null;
            IDbTransaction transactionScope = null;
            SqlParameter param = null;
            Dictionary<string, object> outputParams = new Dictionary<string, object>();

            using (var connection = this.CreateConnection())
            {
                connection.Open();
                transactionScope = doesUseTransaction ? connection.BeginTransaction() : null;

                using (var command = this.CreateCommand(commandText, CommandType.StoredProcedure, connection, transactionScope))
                {
                    try
                    {
                        if (parameters != null)
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);

                        // Output params
                        if (parameterStatus == null)
                            parameterStatus = new BaseParameterStatus("@isSuccess", "@message", "@additionalInformation");

                        if (!string.IsNullOrEmpty(parameterStatus.IsSuccessParameter))
                        {
                            param = new SqlParameter(parameterStatus.IsSuccessParameter, SqlDbType.Bit);
                            param.Direction = ParameterDirection.Output;
                            command.Parameters.Add(param);
                        }

                        if (!string.IsNullOrEmpty(parameterStatus.MessageParameter))
                        {
                            param = new SqlParameter(parameterStatus.MessageParameter, SqlDbType.NVarChar, 1000);
                            param.Direction = ParameterDirection.Output;
                            command.Parameters.Add(param);
                        }

                        if (!string.IsNullOrEmpty(parameterStatus.AdditionalInformationParameter))
                        {
                            param = new SqlParameter(parameterStatus.AdditionalInformationParameter, SqlDbType.NVarChar, 1000);
                            param.Direction = ParameterDirection.Output;
                            command.Parameters.Add(param);
                        }

                        var dataset = new DataSet();
                        var dataAdaper = this.CreateAdapter(command);
                        dataAdaper.Fill(dataset);

                        dataTable = dataset.Tables.Count > 0 ? dataset.Tables[0] : null;

                        foreach (var item in command.Parameters)
                        {
                            var parameter = (IDataParameter)item;
                            if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output ||
                                !parameter.ParameterName.Equals(parameterStatus.IsSuccessParameter) || !parameter.ParameterName.Equals(parameterStatus.MessageParameter) ||
                                !parameter.ParameterName.Equals(parameterStatus.AdditionalInformationParameter))
                                outputParams.Add(parameter.ParameterName, parameter.Value);
                        }

                        messageResult.IsSuccess = command.Parameters.Contains(parameterStatus.IsSuccessParameter) ? (bool)((IDataParameter)command.Parameters[parameterStatus.IsSuccessParameter]).Value : true;

                        messageResult.Message = command.Parameters.Contains(parameterStatus.MessageParameter) && ((IDataParameter)command.Parameters[parameterStatus.MessageParameter]).Value != DBNull.Value ? ((IDataParameter)command.Parameters[parameterStatus.MessageParameter]).Value.ToString() : string.Empty;
                        
                        messageResult.OutputParameters = outputParams;
                        messageResult.Result = dataTable;

                        if (transactionScope != null)
                        {
                            if (messageResult.IsSuccess)
                                transactionScope.Commit();
                            else
                                transactionScope.Rollback();
                        }

                    }
                    catch (Exception ex)
                    {
                        if (transactionScope != null)
                            transactionScope.Rollback();

                        messageResult.IsSuccess = false;
                        messageResult.Message = ex.Message;
                        messageResult.AdditionalInformation = ex.ToString();
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return messageResult;
        }
        #endregion

        #region Private Methods

        private IList<IDbDataParameter> GenerateParameters(IList<InputParameter> inputParameters, IList<OutputParameter> outputParameters = null)
        {
            IList<IDbDataParameter> parameters = new List<IDbDataParameter>();
            object value = null;

            if (inputParameters != null)
            {
                foreach (var parameter in inputParameters)
                {
                    if (parameter.Value != null)
                    {
                        if (parameter.Value.GetType().Equals(typeof(string)))
                        {
                            value = parameter.Value.ToString().Trim();
                        }
                        else if (parameter.Value.GetType().Equals(typeof(DataTable)))
                        {
                            var dataTable = (DataTable)parameter.Value;

                            if (dataTable.Rows.Count > 0)
                                value = dataTable;
                            else
                                continue;
                        }
                        else
                        {
                            value = parameter.Value;
                        }
                    }
                    else
                    {
                        value = DBNull.Value;
                    }

                    parameters.Add(this.SetInputParameter(parameter.Name, value));
                }
            }

            if (outputParameters != null)
            {
                foreach (var parameter in outputParameters)
                {
                    if (parameter.Value != null)
                    {
                        if (parameter.Value.GetType().Equals(typeof(string)))
                            value = parameter.Value.ToString().Trim();
                        else
                            value = parameter.Value;
                    }
                    else
                    {
                        value = (object)DBNull.Value;
                    }

                    parameters.Add(this.SetOutputParameter(parameter.Name, parameter.SqlDbType, parameter.Size, parameter.Precision, parameter.Scale, value));
                }
            }
                

            return parameters;
        }

        private IDbDataParameter SetInputParameter(string name, object value)
        {
            return new SqlParameter
            {
                ParameterName = name,
                Value = value
            };
        }
        private IDbDataParameter SetOutputParameter(string name, SqlDbType sqlDbType)
        {
            return this.SetOutputParameter(name, sqlDbType, 1);
        }
        private IDbDataParameter SetOutputParameter(string name, SqlDbType sqlDbType, int size)
        {
            return this.SetOutputParameter(name, sqlDbType, size, 0, 0, (object)DBNull.Value);
        }
        private IDbDataParameter SetOutputParameter(string name, SqlDbType sqlDbType, byte precision, byte scale)
        {
            return this.SetOutputParameter(name, sqlDbType, 0, precision, scale, (object)DBNull.Value);
        }
        private IDbDataParameter SetOutputParameter(string name, SqlDbType sqlDbType, int size, byte precision, byte scale, object value)
        {
            return new SqlParameter
            {
                ParameterName = name,
                SqlDbType = sqlDbType,
                Size = size,
                Precision = precision,
                Scale = scale,
                Value = value,
                Direction = ParameterDirection.InputOutput
            };
        }
        #endregion

        #region Operation
        public object Get(string commandText, IList<InputParameter> inputParameters)
        {
            var parameters = this.GenerateParameters(inputParameters);
            return GetScalarValue(commandText, CommandType.StoredProcedure, parameters);
        }

        public DataTable Select(string commandText, IList<InputParameter> inputParameters)
        {
            var parameters = this.GenerateParameters(inputParameters);
            return GetDataTable(commandText, CommandType.StoredProcedure, parameters);
        }

        public MessageStatus Save(string commandText, IList<InputParameter> inputParameters, IList<OutputParameter> outputParameters = null, BaseParameterStatus parameterStatus = null, bool useTransaction = true)
        {
            var parameters = this.GenerateParameters(inputParameters, outputParameters);
            return ExecuteNonQuery(commandText, parameters, useTransaction, parameterStatus);
        }

        public MessageStatus Delete(string commandText, IList<InputParameter> inputParameters, IList<OutputParameter> outputParameters = null, BaseParameterStatus parameterStatus = null, bool useTransaction = true)
        {
            var parameters = this.GenerateParameters(inputParameters, outputParameters);
            return ExecuteNonQuery(commandText, parameters, useTransaction, parameterStatus);
        }

        public MessageResult<DataTable> ReturnDataTable(string commandText, IList<InputParameter> inputParameters, IList<OutputParameter> outputParameters = null, BaseParameterStatus parameterStatus = null, bool useTransaction = true)
        {
            var parameters = this.GenerateParameters(inputParameters, outputParameters);
            return ExecuteDataTable(commandText, parameters, useTransaction, parameterStatus);
        }
        #endregion
    }
}

using PortiaNet.HealthCheck.Reporter;
using System.Data;
using System.Data.SqlClient;

namespace PortiaNet.HealthCheck.Writer.SQLServer
{
    internal class HealthCheckReportService : IHealthCheckReportService
    {
        private readonly SQLServerConfigurationWriterConfiguration _configuration;
        private SqlConnection? _connection;

        public HealthCheckReportService(SQLServerConfigurationWriterConfiguration configuration)
        {
            _configuration = configuration;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder(_configuration.ConnectionString);

                // Creating the Database if not exist. It needs to have access to the master database and database creation privilage
                var masterConnectionStringBuilder = new SqlConnectionStringBuilder(_configuration.ConnectionString);
                masterConnectionStringBuilder.InitialCatalog = "master";
                using (var connection = new SqlConnection(masterConnectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    using var dbCommand = connection.CreateCommand();
                    dbCommand.CommandText = $@"IF NOT EXISTS (
  SELECT [name]
    FROM sys.databases
    WHERE [name] = N'{connectionStringBuilder.InitialCatalog}'
)
CREATE DATABASE {connectionStringBuilder.InitialCatalog}";
                    dbCommand.ExecuteNonQuery();
                }

                _connection = new SqlConnection(connectionStringBuilder.ConnectionString);
                _connection.Open();
                using var tableCommand = _connection.CreateCommand();
                tableCommand.CommandText = $@"if object_id('{_configuration.TableName}') is null
begin
	CREATE TABLE {_configuration.TableName}(
		Id				                        BIGINT			NOT NULL		IDENTITY(1, 1),
		{nameof(RequestDetail.IpAddress)}		VARCHAR(200)	NOT NULL,
        {nameof(RequestDetail.Username)}		VARCHAR(500)	NULL,
        {nameof(RequestDetail.Host)}			VARCHAR(200)	NULL,
        {nameof(RequestDetail.Method)}			VARCHAR(1000)	NULL,
        {nameof(RequestDetail.Path)}			VARCHAR(1000)	NULL,
        {nameof(RequestDetail.QueryString)}		VARCHAR(MAX)	NULL,
        {nameof(RequestDetail.UserAgent)}		VARCHAR(MAX)	NULL,
        {nameof(RequestDetail.Duration)}		FLOAT			NULL,
        {nameof(RequestDetail.HadError)}		BIT				NOT NULL		DEFAULT 0,
        {nameof(RequestDetail.NodeName)}		VARCHAR(200)	NULL,
        {nameof(RequestDetail.EventDateTime)}   DateTime        NULL
	);
end";
                tableCommand.ExecuteNonQuery();
            }
            catch
            {
                if (!_configuration.MuteOnError)
                    throw;
            }
        }

        public Task SaveAPICallInformationAsync(RequestDetail requestDetail)
        {
            if (_connection == null)
                return Task.CompletedTask;

            try
            {
                var query = $@"INSERT INTO {_configuration.TableName}(
    {nameof(RequestDetail.IpAddress)},
    {nameof(RequestDetail.Username)},
    {nameof(RequestDetail.Host)},
    {nameof(RequestDetail.Method)},
    {nameof(RequestDetail.Path)},
    {nameof(RequestDetail.QueryString)},
    {nameof(RequestDetail.UserAgent)},
    {nameof(RequestDetail.Duration)},
    {nameof(RequestDetail.HadError)},
    {nameof(RequestDetail.NodeName)},
    {nameof(RequestDetail.EventDateTime)}
)
VALUES (
    @{nameof(RequestDetail.IpAddress)},
    @{nameof(RequestDetail.Username)},
    @{nameof(RequestDetail.Host)},
    @{nameof(RequestDetail.Method)},
    @{nameof(RequestDetail.Path)},
    @{nameof(RequestDetail.QueryString)},
    @{nameof(RequestDetail.UserAgent)},
    @{nameof(RequestDetail.Duration)},
    @{nameof(RequestDetail.HadError)},
    @{nameof(RequestDetail.NodeName)},
    @{nameof(RequestDetail.EventDateTime)}
)";
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = query;
                cmd.Parameters.Add("@" + nameof(RequestDetail.IpAddress), SqlDbType.VarChar, 200).Value = requestDetail.IpAddress?? string.Empty;
                cmd.Parameters.Add("@" + nameof(RequestDetail.Username), SqlDbType.VarChar, 500).Value = requestDetail.Username ?? string.Empty;
                cmd.Parameters.Add("@" + nameof(RequestDetail.Host), SqlDbType.VarChar, 200).Value = requestDetail.Host ?? string.Empty;
                cmd.Parameters.Add("@" + nameof(RequestDetail.Method), SqlDbType.VarChar, 1000).Value = requestDetail.Method ?? string.Empty;
                cmd.Parameters.Add("@" + nameof(RequestDetail.Path), SqlDbType.VarChar, 1000).Value = requestDetail.Path ?? string.Empty;
                cmd.Parameters.Add("@" + nameof(RequestDetail.QueryString), SqlDbType.VarChar, 4000).Value = requestDetail.QueryString ?? string.Empty;
                cmd.Parameters.Add("@" + nameof(RequestDetail.UserAgent), SqlDbType.VarChar, 4000).Value = requestDetail.UserAgent ?? string.Empty;
                cmd.Parameters.Add("@" + nameof(RequestDetail.Duration), SqlDbType.Float, 0).Value = requestDetail.Duration;
                cmd.Parameters.Add("@" + nameof(RequestDetail.HadError), SqlDbType.Bit, 0).Value = requestDetail.HadError;
                cmd.Parameters.Add("@" + nameof(RequestDetail.NodeName), SqlDbType.VarChar, 200).Value = requestDetail.NodeName ?? string.Empty;
                cmd.Parameters.Add("@" + nameof(RequestDetail.EventDateTime), SqlDbType.DateTime, 0).Value = requestDetail.EventDateTime;

                cmd.ExecuteNonQuery();
                return Task.CompletedTask;
            }
            catch
            {
                if (!_configuration.MuteOnError)
                    throw;
                else
                    return Task.CompletedTask;
            }
        }

        ~HealthCheckReportService()
        {
            if(_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
    }
}

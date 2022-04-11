namespace PortiaNet.HealthCheck.Writer
{
    public class SQLServerConfigurationWriterConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Default name is <b>RequestTracks</b>
        /// </summary>
        public string TableName { get; set; } = "RequestTracks";

        public string NodeName { get; set; } = string.Empty;

        public bool MuteOnError { get; set; } = false;
    }
}

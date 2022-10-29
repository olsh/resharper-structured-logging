namespace ReSharper.Structured.Logging.Settings
{
    public enum PropertyNamingType
    {
        PascalCase,

        CamelCase,

        SnakeCase,

        /// <summary>
        /// The elastic naming convention.
        /// </summary>
        /// <remarks>
        /// https://www.elastic.co/guide/en/beats/devguide/current/event-conventions.html
        /// </remarks>
        ElasticNaming
    }
}

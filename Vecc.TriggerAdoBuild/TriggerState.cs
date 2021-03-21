namespace Vecc.TriggerAdoBuild
{
    public class TriggerState
    {
        public string AccessToken { get; set; }
        public string BuildDefinitionId { get; set; }
        public string Organization { get; set; }
        public string Project { get; set; }
        public Request Request { get; set; }
    }
}

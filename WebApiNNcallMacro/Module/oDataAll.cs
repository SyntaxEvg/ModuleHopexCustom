namespace Mega.Api.Models
{
    public class oDataAll
{
    public string MacrosID { get; set; }
    public string Class { get; set; }
    public string Verb { get; set; }
    public string TypeIntegration { get; set; }
    public string un { get; set; }
    public string ip { get; set; }
    public int? DaysInterval { get; set; }
    public dynamic Data
    {
        get;
        set;
    }
    public dynamic Data1
    {
        get;
        set;
    }
}
}
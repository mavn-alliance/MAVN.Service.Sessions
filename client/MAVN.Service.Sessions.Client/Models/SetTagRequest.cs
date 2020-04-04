namespace MAVN.Service.Sessions.Client.Models
{
    public class SetTagRequest
    {
        public string SessionToken { get; set; }

        public string Tag { get; set; }
    }
}

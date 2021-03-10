namespace Findx.Configuration
{
    public class FindxConfigurationOptions
    {
        public string AppId { set; get; }
        public string AppSercet { set; get; }
        public string Group { set; get; }
        public string Namespace { set; get; }
        public string Address { set; get; }
        public int RefreshInteval { set; get; } = 60000;
    }
}

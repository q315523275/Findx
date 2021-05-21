using System.Collections.Generic;

namespace Findx.Swagger
{
    public class SwaggerOptions
    {
        public ICollection<SwaggerEndpoint> Endpoints { get; set; } = new List<SwaggerEndpoint>();

        public bool Enabled { get; set; }
    }

    public class SwaggerEndpoint
    {
        public string Title { get; set; }

        public string Version { get; set; }

        public string Url { get; set; }
    }
}

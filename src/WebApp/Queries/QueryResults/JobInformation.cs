using System;
using System.Collections.Generic;

namespace WebApp.Queries.QueryResults
{
    public class JobInformation
    {
        public string Input { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Output { get; set; }
        public string State { get; set; }
        public string ElapsedTime { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
    }
}

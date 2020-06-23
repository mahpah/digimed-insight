using System;

namespace WebApp.Data.Entities
{
    public class MediaItem
    {
        public MediaItem(string input, string output, string jobName)
        {
            Input = input;
            Output = output;
            JobName = jobName;
        }

        public Guid Id { get; private set; }
        public string Input { get; private set; }
        public string Output { get; private set; }
        public string JobName { get; private set; }
    }

    public enum MediaItemStatus
    {
        Undefined = 0,
        Processing = 1,
        Completed = 2,
        Failed
    }
}

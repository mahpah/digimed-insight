namespace Insights.VideoPackager
{
    public class StreamDescriptor
    {
        public string Input { get; private set; }
        public string Output { get; private set; }
        public string Label { get; private set; }
        public StreamType Type { get; private set; }

        public StreamDescriptor FromInput(string input)
        {
            Input = input;
            return this;
        }
        
        public StreamDescriptor ToOutput(string output)
        {
            Output = output;
            return this;
        }

        public StreamDescriptor WithLabel(string label)
        {
            Label = label;
            return this;
        }
        
        public StreamDescriptor StreamType(StreamType type)
        {
            Type = type;
            return this;
        }
    }
}
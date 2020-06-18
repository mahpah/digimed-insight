using System;
using System.Collections.Generic;

namespace Insights.VideoPackager
{
    public class ShakaCommand
    {
        private List<StreamDescriptor> _streamDescriptors = new List<StreamDescriptor>();
        private List<KeyDescriptor> _keyDescriptors = new List<KeyDescriptor>();
        public string Pssh { get; private set; }
        public bool EnableRawKeyEncryption { get; private set; }
        public string[] ProtectionSystems { get; private set; }
        public string OutputPath { get; private set; }
        public string OutputType { get; private set; }

        public ShakaCommand AddStream(Action<StreamDescriptor> action)
        {
            var descriptor = new StreamDescriptor();
            action.Invoke(descriptor);
            _streamDescriptors.Add(descriptor);
            return this;
        }

        public ShakaCommand WithRawKeyEncryption(bool isEnable = true)
        {
            EnableRawKeyEncryption = true;
            return this;
        }

        public ShakaCommand AddKey(Action<KeyDescriptor> action)
        {
            var key = new KeyDescriptor();
            action.Invoke(key);
            _keyDescriptors.Add(key);
            return this;
        }

        public ShakaCommand SetPssh(string pssh)
        {
            Pssh = pssh;
            return this;
        }

        public ShakaCommand ProtectedBy(params string[] protectionSystems)
        {
            ProtectionSystems = protectionSystems;
            return this;
        }

        public ShakaCommand SetOutput(string type, string path)
        {
            OutputType = type;
            OutputPath = path;
            return this;
        }
    }
}

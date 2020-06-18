using System;
using Xunit;

namespace Insights.VideoPackager.Tests
{
    public class ShakaPackagerTests
    {
        [Fact]
        public void Test1()
        {
            var command = new ShakaCommand()
                .AddStream(x =>
                {
                    x.FromInput("input.mp4")
                        .StreamType(StreamType.Video)
                        .ToOutput("h264_360p.mp4")
                        .WithLabel("sd");
                })
                .AddKey(k =>
                {
                    k.Label = "sd";
                    k.KeyId = "b1972791420653068785e6e7491e1613";
                    k.Key = "bfd08f2a4799ae5cd2704a6cc23562c4";
                })
                .AddStream(x =>
                {
                    x.FromInput("input.mp4")
                        .StreamType(StreamType.Audio)
                        .ToOutput("audio.mp4")
                        .WithLabel("audio");
                })
                .AddKey(k =>
                {
                    k.Label = "audio";
                    k.KeyId = "b1972791420653068785e6e7491e1613";
                    k.Key = "bfd08f2a4799ae5cd2704a6cc23562c4";
                })
                .SetPssh("000000547073736800000000EDEF8BA979D64ACEA3C827DCD51D21ED0000003408011210b1972791420653068785e6e7491e16131a086d6f7669646f6e65221053023592112f87459fb9d0276e79ef662a025344")
                .SetOutput("mpd", "h264_drm.mpd")
                .ProtectedBy("Widevine", "PlayReady");
        }
    }
}

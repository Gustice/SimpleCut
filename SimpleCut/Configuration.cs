using SimpleCut.Utils;
using System;

namespace SimpleCut
{
    [Serializable]
    public class Configuration
    {
        public string DefaultIpAddress { get; set; }
        public double CrossFadeDuration { get; set; }

        public StringObjectPair<long> Cam1Source { get; set; }
        public StringObjectPair<long> Cam2Source { get; set; }
        public StringObjectPair<long> NoteBookSource { get; set; }
        public StringObjectPair<long> LogoSource { get; set; }
    }
}


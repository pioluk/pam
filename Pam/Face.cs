using System;
using System.Drawing;

namespace Pam
{
    internal class Face : IDisposable
    {
        public const int HIST_CH_BITS = 4;
        public const int HIST_BITS = HIST_CH_BITS * 3;
        public const int HIST_LEN = 1 << HIST_BITS;
        public int TimesUnused { get; set; }
        public IArtifact Artifact { get; set; }
        public bool InUse { get; set; } = false;
        public float[] histogram;
        public RectFilter RectFilter = new RectFilter();

        public void Dispose()
        {
        }
    }
}

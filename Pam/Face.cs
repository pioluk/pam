using System;
using System.Drawing;

namespace Pam
{
    internal class Face : IDisposable
    {
        public const int HIST_CH_BITS = 4;
        public const int HIST_BITS = HIST_CH_BITS * 3;
        public const int HIST_LEN = 1 << HIST_BITS;

        public int Id = 0;
        public int TimesUnused = 0;
        public Bitmap Bitmap = null;
        public IArtifact Artifact = null;
        public bool InUse = false;
        public float[] histogram;
        public RectFilter RectFilter = new RectFilter();

        public void Dispose()
        {
            if(Bitmap != null)
                Bitmap.Dispose();
        }
    }
}

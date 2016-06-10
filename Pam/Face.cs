using System;
using System.Drawing;

namespace Pam
{
    internal class Face : IDisposable
    {
        public int Id = 0;
        public int TimesUnused = 0;
        public IArtifact Artifact = null;
        public bool InUse = false;
        public double bestFactor;
        public int bestRectIdx;
        public Bitmap Mini = null;
        public RectFilter RectFilter = new RectFilter();

        public void Dispose()
        {
            if(Mini != null)
                Mini.Dispose();
        }
    }
}

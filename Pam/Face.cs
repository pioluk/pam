using System;
using System.Drawing;

namespace Pam
{
    internal class Face : IDisposable
    {
        public int Id = 0;
        public int TimesUnused = 0;
        public Bitmap Bitmap = null;
        public IArtifact Artifact = null;
        public bool InUse = false;
        public RectFilter RectFilter = new RectFilter();

        public void Dispose()
        {
            if(Bitmap != null)
                Bitmap.Dispose();
        }
    }
}

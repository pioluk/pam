using System;
using System.Drawing;

namespace Pam
{
    internal class Face : IDisposable
    {
        public int TimesUnused { get; set; }
        public Bitmap Bitmap { get; set; }
        public IArtifact Artifact { get; set; }
        public bool InUse { get; set; } = false;

        public void Dispose()
        {
            Bitmap.Dispose();
        }
    }
}
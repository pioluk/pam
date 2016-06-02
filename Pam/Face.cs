using System;
using System.Drawing;

namespace Pam
{
    internal class Face : IDisposable
    {
        public int TimesUnused { get; set; }
        public IArtifact Artifact { get; set; }
        public bool InUse { get; set; } = false;
        public RectFilter RectFilter = new RectFilter();

        public void Dispose()
        {
        }
    }
}

using System;
using System.Drawing;

namespace Pam
{
    internal class Face
    {
        public int Id = 0;
        public int TimesUnused = 0;
        public int TimesUndetected = 0;
        public int Age = 0;
        public IArtifact Artifact = null;
        public bool InUse = false;
        public ushort[] Mini = null;
        public RectFilter RectFilter = new RectFilter();
    }
}

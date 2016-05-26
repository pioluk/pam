using System;
using System.Drawing;

namespace Pam
{
    internal class Face
    {
        public int TimesUnused { get; set; }
        public Bitmap Bitmap { get; set; }
        public IArtifact Artifact { get; set; }
        public bool InUse { get; set; } = false;
    }
}
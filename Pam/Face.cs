﻿using System;
using System.Drawing;

namespace Pam
{
    internal class Face : IDisposable
    {
        public int Id = 0;
        public int TimesUnused = 0;
        public IArtifact Artifact = null;
        public bool InUse = false;
        public Bitmap Mini = null;
        public float[] histogram = null;
        public RectFilter RectFilter = new RectFilter();

        public void Dispose()
        {
            if(Mini != null)
                Mini.Dispose();
        }
    }
}

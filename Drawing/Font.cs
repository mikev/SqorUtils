using System;

namespace Sqor.Utils.Drawing
{
    public partial class Font
    {
#pragma warning disable 649
        private string name;
        private float points;    
#pragma warning restore 649
    
        public Font()
        {
        }
        
        public string Name
        {
            get { return name; }
        }
        
        public float Points
        {
            get { return points; }
        }
    }
}


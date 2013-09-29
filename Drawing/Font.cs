using System;

namespace Sqor.Utils.Drawing
{
    public partial class Font
    {
        private string name;
        private float points;    
    
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


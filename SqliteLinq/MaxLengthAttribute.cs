using System;

namespace Sqor.Utils.SqliteLinq
{
    public class MaxLengthAttribute : Attribute
    {
        private int maxLength;

        public MaxLengthAttribute(int maxLength)
        {
            this.maxLength = maxLength;
        }
        
        public int MaxLength
        {
            get { return this.maxLength; }
        }
    }
}


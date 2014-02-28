using System;

namespace Sqor.Utils.Models
{
    public struct ImplicitValue
    {
        private readonly object value;

        public ImplicitValue(object value)
        {
            this.value = value;
        }

        public static implicit operator string(ImplicitValue value)
        {
            return (string)value.value;
        } 

        public static implicit operator bool(ImplicitValue value)
        {
            return (bool)value.value;
        }

        public static implicit operator int(ImplicitValue value)
        {
            return (int)value.value;
        } 

        public static implicit operator short(ImplicitValue value)
        {
            return (short)value.value;
        } 

        public static implicit operator byte(ImplicitValue value)
        {
            return (byte)value.value;
        } 

        public static implicit operator long(ImplicitValue value)
        {
            return (long)value.value;
        } 

        public static implicit operator uint(ImplicitValue value)
        {
            return (uint)value.value;
        } 

        public static implicit operator ushort(ImplicitValue value)
        {
            return (ushort)value.value;
        } 

        public static implicit operator sbyte(ImplicitValue value)
        {
            return (sbyte)value.value;
        } 

        public static implicit operator ulong(ImplicitValue value)
        {
            return (ulong)value.value;
        } 

        public static implicit operator float(ImplicitValue value)
        {
            return (float)value.value;
        } 

        public static implicit operator double(ImplicitValue value)
        {
            return (double)value.value;
        } 

        public static implicit operator decimal(ImplicitValue value)
        {
            return (decimal)value.value;
        } 

        public static implicit operator DateTime(ImplicitValue value)
        {
            return (DateTime)value.value;
        } 

        public static implicit operator int?(ImplicitValue value)
        {
            return (int?)value.value;
        } 

        public static implicit operator bool?(ImplicitValue value)
        {
            return (bool?)value.value;
        } 

        public static implicit operator short?(ImplicitValue value)
        {
            return (short?)value.value;
        } 

        public static implicit operator byte?(ImplicitValue value)
        {
            return (byte?)value.value;
        } 

        public static implicit operator long?(ImplicitValue value)
        {
            return (long?)value.value;
        } 

        public static implicit operator uint?(ImplicitValue value)
        {
            return (uint?)value.value;
        } 

        public static implicit operator ushort?(ImplicitValue value)
        {
            return (ushort?)value.value;
        } 

        public static implicit operator sbyte?(ImplicitValue value)
        {
            return (sbyte?)value.value;
        } 

        public static implicit operator ulong?(ImplicitValue value)
        {
            return (ulong?)value.value;
        } 

        public static implicit operator float?(ImplicitValue value)
        {
            return (float?)value.value;
        } 

        public static implicit operator double?(ImplicitValue value)
        {
            return (double?)value.value;
        } 

        public static implicit operator decimal?(ImplicitValue value)
        {
            return (decimal?)value.value;
        } 

        public static implicit operator DateTime?(ImplicitValue value)
        {
            return (DateTime?)value.value;
        } 
    }
}
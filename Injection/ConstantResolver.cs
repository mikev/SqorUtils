﻿using System;

namespace Sqor.Utils.Injection
{
    public class ConstantResolver : ReflectionResolver
    {
        private object constant;

        public ConstantResolver(Type type, object constant) : base(type)
        {
            this.constant = constant;
        }

        public override object Instantiate(Request request)
        {
            return constant;
        }
    }
}

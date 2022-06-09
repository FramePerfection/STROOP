﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace STROOP.Utilities
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class InitializerAttribute : Attribute { }

    public class InitializeSpecialAttribute : InitializerAttribute { }
    public class InitializeBaseAddressAttribute : InitializerAttribute { }
}

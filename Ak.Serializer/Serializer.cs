﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ak.Serializer
{
    interface ISerializer
    {
        string Serialize(object value);
        T Deserialize<T>(string value);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SockGet.Core.Enums
{
    [Flags]
    internal enum Token 
    {
        Message,
        Auth,
        Tag,
        Heartbeat,
        Sync
    }
}

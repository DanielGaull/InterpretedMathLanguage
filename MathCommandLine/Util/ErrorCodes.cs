﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Util
{
    public enum ErrorCodes
    {
        FATAL_UNKNOWN = 0,
        NOT_COMPOSITE = 1,
        INVALID_TYPE = 2,
        KEY_DOES_NOT_EXIST = 3,
        NOT_A_STRING = 4,
        I_OUT_OF_RANGE = 5,
        WRONG_ARG_COUNT = 6,
    }
}

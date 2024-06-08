using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Util
{
    public enum ErrorCodes
    {
        FATAL_UNKNOWN = 0,
        NO_PROPERTIES = 1,
        INVALID_TYPE = 2,
        KEY_DOES_NOT_EXIST = 3,
        NOT_A_STRING = 4,
        I_OUT_OF_RANGE = 5,
        WRONG_ARG_COUNT = 6,
        DIV_BY_ZERO = 7,
        FAILS_RESTRICTION = 8,
        TYPE_DOES_NOT_EXIST = 9,
        NOT_CALLABLE = 10,
        VAR_DOES_NOT_EXIST = 11,
        CANNOT_ASSIGN = 12,
        CANNOT_DECLARE = 13,
        INVALID_ARGUMENT = 14,
        ILLEGAL_LAMBDA = 15,
        WRONG_GENERIC_COUNT = 16,

    }
}

// Copyright (c) Leonardo Brugnara
// Full copyright and license information in LICENSE file
using System;

namespace CmdOpt.Options
{
    [Flags]
    public enum OptionAttributes
    {
        None = 0,
        Optional = 1,
        OptionalValue = 2,
        MultiValue = 4,
        SubModule = 8
    }
}

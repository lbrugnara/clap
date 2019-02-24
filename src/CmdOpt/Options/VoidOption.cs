// Copyright (c) Leonardo Brugnara
// Full copyright and license information in LICENSE file
using CmdOpt.Environment;
using System;

namespace CmdOpt.Options
{
    public class VoidOption<TEnvironment> : Option<TEnvironment> where TEnvironment : Environment<TEnvironment>
    {
        public override OptionHandler<TEnvironment> Handler { get; }

        public VoidOption(string shortopt, string longopt, string description, OptionAttributes attributes, Action<TEnvironment> handler)
            : base (shortopt, longopt, description, attributes)
        {
            this.Handler = (env, args) => handler(env);
        }
    }
}

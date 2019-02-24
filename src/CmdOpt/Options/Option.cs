// Copyright (c) Leonardo Brugnara
// Full copyright and license information in LICENSE file
using CmdOpt.Environment;

namespace CmdOpt.Options
{
    public delegate void OptionHandler<TEnvironment>(TEnvironment env, params string[] arguments) where TEnvironment : Environment<TEnvironment>;

    public abstract class Option<TEnvironment> where TEnvironment : Environment<TEnvironment>
    {
        public Option(string shortopt, string longopt, string description, OptionAttributes attributes)
        {
            ShortName = shortopt;
            LongName = longopt;
            Description = description;
            Attributes = attributes;
        }

        /// <summary>
        /// Option's short name
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Option's long name
        /// </summary>
        public string LongName { get; }

        /// <summary>
        /// Option description
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Option's attributes that determine the behavior
        /// </summary>
        public OptionAttributes Attributes { get; }

        /// <summary>
        /// Returns true if the option is required by checking if the <see cref="OptionAttributes.Optional"/> is missing and
        /// the option is not a <see cref="OptionAttributes.SubModule"/>
        /// </summary>
        /// <returns>True if the option is required</returns>
        public bool IsRequired => !Attributes.HasFlag(OptionAttributes.Optional) && !Attributes.HasFlag(OptionAttributes.SubModule);

        /// <summary>
        /// The delegate handler that will be called once the option has 
        /// been parsed in order to update the TEnvironment
        /// </summary>
        public abstract OptionHandler<TEnvironment> Handler { get; }

        /// <summary>
        /// Returns the option description formatted to show in the
        /// help message
        /// </summary>
        /// <param name="neededpad"></param>
        /// <returns></returns>
        public string GetFormattedDescription(int neededpad)
        {
            if (Description == null)
                return "\t";
            string desc = "\t";
            bool needsBreak = false;
            for (int i=0; i < Description.Length; i++)
            {
                if (i > 0 && i % 120 == 0)
                {
                    if (Description[i] == ' ')
                    {
                        desc += "\n\t".PadRight(neededpad, ' ') + "\t";
                    }
                    else
                    {
                        needsBreak = true;
                    }
                }
                else if (needsBreak && Description[i-1] == ' ')
                {
                    needsBreak = false;
                    desc += "\n\t".PadRight(neededpad, ' ') + "\t";
                }
                desc += Description[i];
            }
            return desc;
        }
    }
}

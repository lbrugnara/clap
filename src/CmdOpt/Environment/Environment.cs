// Copyright (c) Leonardo Brugnara
// Full copyright and license information in LICENSE file
using CmdOpt.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CmdOpt.Environment
{
    /// <summary>
    /// The Command Line Argument Parser environment provides basic functionality
    /// and expose hooks in the parsing's life cycle
    /// </summary>
    /// <typeparam name="TEnvironment"></typeparam>
    public abstract class Environment<TEnvironment> : IEnumerable<Option<TEnvironment>> where TEnvironment : Environment<TEnvironment>
    {
        /// <summary>
        /// Flag to track if the user has requested to see the help message
        /// </summary>
        protected bool ShowHelp { get; set; }

        public Environment()
        {
            this.Options = new List<Option<TEnvironment>>();
            this.Errors = new List<string>();
        }

        /// <summary>
        /// Options this environment expects as valid options
        /// </summary>
        private List<Option<TEnvironment>> Options { get; set; }

        /// <summary>
        /// If true, the environment has an invalid state that resulted from
        /// the argument parsing
        /// </summary>
        public bool Error => this.Errors != null && this.Errors.Any();

        /// <summary>
        /// List of errors generated in the parsing process
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Parse the array of strings as the environment's arguments
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>True if the parsing succeeded</returns>
        public bool Parse(string[] args)
        {
            bool parsingSucceeded = this.Parse(this as TEnvironment, args);
            if (parsingSucceeded)
            {
                this.ValidateOptions();
            }
            return parsingSucceeded;
        }

        /// <summary>
        /// Checks if the help message has been requested or an invalid parsing resulted
        /// in it being shown
        /// </summary>
        /// <returns></returns>
        public bool IsHelpMessageRequest()
        {
            return ShowHelp;
        }

        /// <summary>
        /// Sets the <see cref="ShowHelp"/> as true to mark that the user has
        /// requested the help message, or the parsing resulted in it being shown
        /// </summary>
        public void RequestHelpMessage()
        {
            ShowHelp = true;
        }

        /// <summary>
        /// Process the different hooks to get the errors and then
        /// send them out to the <see cref="OnShowErrorMessage"/>
        /// </summary>
        public void ShowErrorMessage()
        {
            List<string> errors = new List<string>();

            string beforeerrors = this.OnBeforeErrorMessages();

            foreach (string err in Errors)
            {
                string beforeerror = this.OnBeforeErrorMessage();
                string error = this.OnErrorMessage(err);
                string aftererror = this.OnAfterErrorMessage();

                errors.Add(beforeerror + error + aftererror);
            }

            string aftererrors = this.OnAfterErrorMessages();

            this.OnShowErrorMessage(beforeerrors + string.Join("\n", errors) + aftererrors);
        }

        /// <summary>
        /// Called before processing all the errors
        /// </summary>
        /// <returns></returns>
        protected virtual string OnBeforeErrorMessages()
        {
            return "";
        }

        /// <summary>
        /// Called before each error
        /// </summary>
        /// <returns></returns>
        protected virtual string OnBeforeErrorMessage()
        {
            return "";
        }

        /// <summary>
        /// Called to get each error
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        protected virtual string OnErrorMessage(string error)
        {
            return error;
        }

        /// <summary>
        /// Called after each error
        /// </summary>
        /// <returns></returns>
        protected virtual string OnAfterErrorMessage()
        {
            return "";
        }

        /// <summary>
        /// Called after processing all errors
        /// </summary>
        /// <returns></returns>
        protected virtual string OnAfterErrorMessages()
        {
            return "";
        }

        /// <summary>
        /// Outputs the error message to the console's output
        /// </summary>
        /// <param name="error"></param>
        protected virtual void OnShowErrorMessage(string error)
        {
            Console.WriteLine(error);
        }

        /// <summary>
        /// Processes and shows the help message
        /// </summary>
        public void ShowHelpMessage()
        {
            string beforehelp = this.OnBeforeHelpMessage();
            string help = this.OnHelpMessage(this.GetHelpMessage());
            string afterhelp = this.OnAfterHelpMessage();

            this.OnShowHelpMessage(beforehelp + help + afterhelp);
        }

        /// <summary>
        /// Called before processing the help message
        /// </summary>
        /// <returns></returns>
        protected virtual string OnBeforeHelpMessage()
        {
            return "";
        }

        /// <summary>
        /// Returns the help message
        /// </summary>
        /// <param name="help"></param>
        /// <returns></returns>
        protected virtual string OnHelpMessage(string help)
        {
            return help;
        }

        /// <summary>
        /// Called after processing the help message
        /// </summary>
        /// <returns></returns>
        protected virtual string OnAfterHelpMessage()
        {
            return "";
        }

        /// <summary>
        /// Outputs the help message to the console's output
        /// </summary>
        /// <param name="help"></param>
        protected virtual void OnShowHelpMessage(string help)
        {
            Console.WriteLine(help);
        }

        public abstract void ValidateOptions();

        /// <summary>
        /// Returns the formatted help message
        /// </summary>
        /// <returns></returns>
        public string GetHelpMessage()
        {
            return string.Join("\n", Options.Select(p => {
                string name = string.Format("  {0}{1}{2}", p.ShortName, p.LongName != null ? "|" : "", p.LongName);
                string desc = string.Format("\t{0}", p.GetFormattedDescription(name.Length), p.Attributes.ToString());
                string attrs = p.Attributes != OptionAttributes.None ? $" ({p.Attributes.ToString()})" : "";
                return name + desc + attrs + "\n";
            }));
        }

        /// <summary>
        /// Creates and adds a new void parameter to the environment
        /// </summary>
        /// <param name="shortopt">Parameter's short name</param>
        /// <param name="longopt">Parameter's long name</param>
        /// <param name="description">Parameter's description</param>
        /// <param name="handler"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public Option<TEnvironment> Add(string shortopt, string longopt, string description, Action<TEnvironment> handler, OptionAttributes attributes)
        {
            var p = new VoidOption<TEnvironment>(shortopt, longopt, description, attributes, handler);
            Options.Add(p);
            return p;
        }

        public Option<TEnvironment> Add(string shortopt, string longopt, string description, Action<TEnvironment, string> handler, OptionAttributes attributes)
        {
            var p = new SingleValueOption<TEnvironment>(shortopt, longopt, description, attributes, handler);
            Options.Add(p);
            return p;
        }

        public Option<TEnvironment> Add(string shortopt, string longopt, string description, Action<TEnvironment, string[]> action, OptionAttributes attributes)
        {
            var p = new MultiValueOption<TEnvironment>(shortopt, longopt, description, attributes, action);
            Options.Add(p);
            return p;
        }

        public bool Parse(TEnvironment env, string[] args)
        {
            // Create a copy of the list to track already parsed options
            var options = this.Options.ToList();

            int index = 0;
            while (index < args.Length)
            {
                string argument = args.ElementAtOrDefault(index);

                // Get the parameter matching this argument
                var parameter = options.FirstOrDefault(p => p.ShortName == argument || p.LongName == argument);

                if (parameter == null)
                {
                    index++;
                    continue;
                }

                // Remove the parameter we found so we won't process it again
                options.Remove(parameter);

                switch (parameter)
                {
                    case VoidOption<TEnvironment> sp:
                        this.ParseVoidParamater(sp, env);
                        index++;
                        break;

                    case SingleValueOption<TEnvironment> svp:
                        index = this.ParseSingleValueParamater(svp, env, args, index);
                        break;

                    case MultiValueOption<TEnvironment> mvp:
                        index = this.ParseMultiValueParameter(mvp, env, args, index);
                        break;

                    default:
                        throw new Exception("ClapEnv is misconfigured");
                }
            }

            // Set the error message on every required parameter not processed
            options.Where(p => p.IsRequired).ToList().ForEach(p => env.Errors.Add(string.Format("Parameter {0} is required", p.LongName ?? p.ShortName)));

            return !env.Error;
        }

        /// <summary>
        /// Invokes the <see cref="VoidOption{TEnvironment}.Handler"/> to process the parameter
        /// </summary>
        /// <param name="parameter">Void parameter</param>
        /// <param name="env">Environment</param>
        private void ParseVoidParamater(VoidOption<TEnvironment> parameter, TEnvironment env)
        {
            parameter.Handler.Invoke(env);
        }

        /// <summary>
        /// Invokes the <see cref="SingleValueOption{TEnvironment}.Handler"/> to process the parameter
        /// and the provided value
        /// </summary>
        /// <param name="parameter">Single-value parameter</param>
        /// <param name="env">Environment</param>
        /// <param name="args">Arguments to lookup the value</param>
        /// <param name="paramIndex">Pointer to the first possible value in the arguments list</param>
        /// <returns>Pointer to the value-past position (or parameter-past position if there is no value and the parameter is optional value)</returns>
        private int ParseSingleValueParamater(SingleValueOption<TEnvironment> parameter, TEnvironment env, string[] args, int paramIndex)
        {
            var value = this.GetParameterValue(parameter, args, paramIndex);

            // Leave on error
            if (value == null && parameter.IsRequired)
            {
                env.Errors.Add(string.Format("Parameter {0} is required", parameter.LongName ?? parameter.ShortName));
                // Move 1 position past to the parameter
                return paramIndex + 1;
            }

            parameter.Handler.Invoke(env, value);

            // If there is a value, increment the pointer
            if (value != null)
                return paramIndex + 2;

            return paramIndex + 1;
        }

        private int ParseMultiValueParameter(MultiValueOption<TEnvironment> parameter, TEnvironment env, string[] args, int parameterIndex)
        {
            if (parameter.Attributes.HasFlag(OptionAttributes.SubModule))
            {
                this.ParseMultiValueAsSubModule(parameter, env, args.Skip(parameterIndex + 1).ToArray());

                // The submodule process the rest of the arguments, so we need to leave
                return args.Length;
            }
            else if (parameter.Attributes.HasFlag(OptionAttributes.MultiValue))
            {
                parameterIndex = this.ParseMultiValueAsParameter(parameter, env, args, parameterIndex);
            }

            return parameterIndex;
        }

        private void ParseMultiValueAsSubModule(MultiValueOption<TEnvironment> parameter, TEnvironment env, string[] args)
        {
            parameter.Handler.Invoke(env, args);
        }

        private int ParseMultiValueAsParameter(MultiValueOption<TEnvironment> parameter, TEnvironment env, string[] args, int parameterIndex)
        {
            var arguments = new List<string>();

            string value = null;
            do
            {
                value = this.GetParameterValue(parameter, args, parameterIndex);

                if (value != null)
                {
                    parameterIndex++;
                    arguments.Add(value);
                }
            } while (value != null);

            // Leave on error
            if (parameter.IsRequired && arguments.Count == 0)
            {
                env.Errors.Add(string.Format("Parameter {0} is required", parameter.LongName ?? parameter.ShortName));
                // Move 1 position past to the parameter
                return parameterIndex + 1;
            }

            parameter.Handler.Invoke(env, arguments.ToArray());

            // Move 1 position past to the last value
            return parameterIndex + 1;
        }

        private string GetParameterValue(Option<TEnvironment> parameter, string[] args, int paramIndex)
        {
            // Get the possible value, next to the parameter
            var value = args.ElementAtOrDefault(paramIndex + 1);

            // If null, the value is missing
            var isEnd = string.IsNullOrEmpty(value);
            // Check if it is another parameter instead of a value
            var isOtherParam = !isEnd && value.StartsWith("-") && this.Options.Any(p => p.ShortName == value || p.LongName == value);
            // If it is empty or is another parameter, there is no value for this parameter
            var noValue = isEnd || isOtherParam;

            // Leave on error
            if (noValue && parameter.IsRequired)
                return null;

            // If there is no value, set it as null
            if (noValue)
                value = null;

            return value;
        }

        public IEnumerator<Option<TEnvironment>> GetEnumerator()
        {
            return this.Options.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Options.GetEnumerator();
        }
    }
}
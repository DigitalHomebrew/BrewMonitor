﻿namespace Caliburn.Micro
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if WinRT && !WinRT81
    using Windows.UI.Xaml;
    using Windows.UI.Interactivity;
    using TriggerBase = Windows.UI.Interactivity.TriggerBase;
    using EventTrigger = Windows.UI.Interactivity.EventTrigger;
    using TriggerAction = Windows.UI.Interactivity.TriggerAction;
    using System.Text;
    using System.Text.RegularExpressions;
    using Windows.UI.Xaml.Data;
#elif WinRT81
    using System.Reflection;
    using Windows.UI.Xaml;
    using Microsoft.Xaml.Interactivity;
    using TriggerBase = Microsoft.Xaml.Interactivity.IBehavior;
    using EventTrigger = Microsoft.Xaml.Interactions.Core.EventTriggerBehavior;
    using TriggerAction = Microsoft.Xaml.Interactivity.IAction;
    using System.Text;
    using System.Text.RegularExpressions;
    using Windows.UI.Xaml.Data;
#else
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Data;
    using EventTrigger = System.Windows.Interactivity.EventTrigger;
    using TriggerBase = System.Windows.Interactivity.TriggerBase;
    using TriggerAction = System.Windows.Interactivity.TriggerAction;
    using System.Text;
#endif

    /// <summary>
    /// Parses text into a fully functional set of <see cref="TriggerBase"/> instances with <see cref="ActionMessage"/>.
    /// </summary>
    public static class Parser
    {
        static readonly Regex LongFormatRegularExpression = new Regex(@"^[\s]*\[[^\]]*\][\s]*=[\s]*\[[^\]]*\][\s]*$");
        static readonly ILog Log = LogManager.GetLog(typeof(Parser));

        /// <summary>
        /// Parses the specified message text.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="text">The message text.</param>
        /// <returns>The triggers parsed from the text.</returns>
        public static IEnumerable<TriggerBase> Parse(DependencyObject target, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new TriggerBase[0];
            }

            var triggers = new List<TriggerBase>();
            var messageTexts = Split(text, ';');

            foreach (var messageText in messageTexts)
            {
                var triggerPlusMessage = LongFormatRegularExpression.IsMatch(messageText)
                                             ? Split(messageText, '=')
                                             : new[] { null, messageText };

                var messageDetail = triggerPlusMessage.Last()
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Trim();

                var trigger = CreateTrigger(target, triggerPlusMessage.Length == 1 ? null : triggerPlusMessage[0]);
                var message = CreateMessage(target, messageDetail);

#if WinRT81
                AddActionToTrigger(target, message, trigger);
#else
                trigger.Actions.Add(message);
#endif

                triggers.Add(trigger);
            }

            return triggers;
        }

#if WinRT81

        private static void AddActionToTrigger(DependencyObject target, TriggerAction message, TriggerBase trigger)
        {
            // This is stupid, but there a number of limitiations in the 8.1 Behaviours SDK

            // The first is that there is no base class for a Trigger, just IBehaviour. Which
            // means there's no strongly typed way to add an action to a trigger. Every trigger
            // in the SDK implements the same pattern but no interface, we're going to have to
            // use reflection to set it.

            // More stupidity, ActionCollection doesn't care about IAction, but DependencyObject
            // and there's no actual implementation of 

            var messageDependencyObject = message as DependencyObject;

            if (messageDependencyObject == null)
            {
                Log.Warn("{0} doesn't inherit DependencyObject and can't be added to ActionCollection", trigger.GetType().FullName);
                return;
            }

            // 95% of the time the trigger will be an EventTrigger, let's optimise for that case

            if (trigger is EventTrigger)
            {
                var eventTrigger = (EventTrigger) trigger;

                eventTrigger.Actions.Add(messageDependencyObject);
            }
            else
            {
                var actionsProperty = trigger.GetType().GetRuntimeProperty("Actions");

                if (actionsProperty == null)
                {
                    Log.Warn("Could not find Actions collection on trigger {0}.", trigger.GetType().FullName);
                    return;
                }

                var actionCollection = actionsProperty.GetValue(trigger) as ActionCollection;

                if (actionCollection == null)
                {
                    Log.Warn("{0}.Actions is either not an ActionCollection or is null.", trigger.GetType().FullName);
                    return;
                }

                actionCollection.Add(messageDependencyObject);
            }

            // The second is the IAction doesn't have an associated object property so we have
            // to create it ourselves, could be potential issues here with leaking the associated 
            // object and not correctly detaching, this may depend if the trigger implements it's
            // AssociatedObject as a DependencyProperty.

            // Turns out trying to a binding won't work because the trigger doesn't notify the 
            // binding of changes, so we just need to set it, yay.

            var actionMessage = message as ActionMessage;
            var targetElement = target as FrameworkElement;

            if (actionMessage != null && targetElement != null)
            {
                //var binding = new Binding { Source = trigger, Path = new PropertyPath("AssociatedObject") };

                //BindingOperations.SetBinding(actionMessage, ActionMessage.AssociatedObjectProperty, binding);

                actionMessage.AssociatedObject = targetElement;
            }
            
        }

#endif

        /// <summary>
        /// The function used to generate a trigger.
        /// </summary>
        /// <remarks>The parameters passed to the method are the the target of the trigger and string representing the trigger.</remarks>
        public static Func<DependencyObject, string, TriggerBase> CreateTrigger = (target, triggerText) =>
        {
            if (triggerText == null)
            {
                var defaults = ConventionManager.GetElementConvention(target.GetType());
                return defaults.CreateTrigger();
            }

            var triggerDetail = triggerText
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("Event", string.Empty)
                .Trim();

            return new EventTrigger { EventName = triggerDetail };
        };

        /// <summary>
        /// Creates an instance of <see cref="ActionMessage"/> by parsing out the textual dsl.
        /// </summary>
        /// <param name="target">The target of the message.</param>
        /// <param name="messageText">The textual message dsl.</param>
        /// <returns>The created message.</returns>
        public static TriggerAction CreateMessage(DependencyObject target, string messageText)
        {
            var openingParenthesisIndex = messageText.IndexOf('(');
            if (openingParenthesisIndex < 0)
            {
                openingParenthesisIndex = messageText.Length;
            }

            var closingParenthesisIndex = messageText.LastIndexOf(')');
            if (closingParenthesisIndex < 0)
            {
                closingParenthesisIndex = messageText.Length;
            }

            var core = messageText.Substring(0, openingParenthesisIndex).Trim();
            var message = InterpretMessageText(target, core);
            var withParameters = message as IHaveParameters;
            if (withParameters != null)
            {
                if (closingParenthesisIndex - openingParenthesisIndex > 1)
                {
                    var paramString = messageText.Substring(openingParenthesisIndex + 1, closingParenthesisIndex - openingParenthesisIndex - 1);
                    var parameters = SplitParameters(paramString);

                    foreach (var parameter in parameters)
                        withParameters.Parameters.Add(CreateParameter(target, parameter.Trim()));
                }
            }

            return message;
        }

        /// <summary>
        /// Function used to parse a string identified as a message.
        /// </summary>
        public static Func<DependencyObject, string, TriggerAction> InterpretMessageText = (target, text) =>
        {
            return new ActionMessage { MethodName = Regex.Replace(text, "^Action", string.Empty).Trim() };
        };

        /// <summary>
        /// Function used to parse a string identified as a message parameter.
        /// </summary>
        public static Func<DependencyObject, string, Parameter> CreateParameter = (target, parameterText) =>
        {
            var actualParameter = new Parameter();

            if (parameterText.StartsWith("'") && parameterText.EndsWith("'"))
            {
                actualParameter.Value = parameterText.Substring(1, parameterText.Length - 2);
            }
            else if (MessageBinder.SpecialValues.ContainsKey(parameterText.ToLower()) || char.IsNumber(parameterText[0]))
            {
                actualParameter.Value = parameterText;
            }
            else if (target is FrameworkElement)
            {
                var fe = (FrameworkElement)target;
                var nameAndBindingMode = parameterText.Split(':').Select(x => x.Trim()).ToArray();
                var index = nameAndBindingMode[0].IndexOf('.');

                View.ExecuteOnLoad(fe, delegate
                {
                    BindParameter(
                        fe,
                        actualParameter,
                        index == -1 ? nameAndBindingMode[0] : nameAndBindingMode[0].Substring(0, index),
                        index == -1 ? null : nameAndBindingMode[0].Substring(index + 1),
                        nameAndBindingMode.Length == 2 ? (BindingMode)Enum.Parse(typeof(BindingMode), nameAndBindingMode[1], true) : BindingMode.OneWay
                        );
                });
            }

            return actualParameter;
        };

        /// <summary>
        /// Creates a binding on a <see cref="Parameter"/>.
        /// </summary>
        /// <param name="target">The target to which the message is applied.</param>
        /// <param name="parameter">The parameter object.</param>
        /// <param name="elementName">The name of the element to bind to.</param>
        /// <param name="path">The path of the element to bind to.</param>
        /// <param name="bindingMode">The binding mode to use.</param>
        public static void BindParameter(FrameworkElement target, Parameter parameter, string elementName, string path, BindingMode bindingMode)
        {
            var element = elementName == "$this"
                ? target
                : BindingScope.GetNamedElements(target).FindName(elementName);
            if (element == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                path = ConventionManager.GetElementConvention(element.GetType()).ParameterProperty;
            }
#if WinRT
            var binding = new Binding
            {
                Path = new PropertyPath(path),
                Source = element,
                Mode = bindingMode
            };
#else
            var binding = new Binding(path) {
                Source = element,
                Mode = bindingMode
            };
#endif

#if (SILVERLIGHT && !SL5)
            var expression = (BindingExpression)BindingOperations.SetBinding(parameter, Parameter.ValueProperty, binding);

            var field = element.GetType().GetField(path + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field == null) {
                return;
            }

            ConventionManager.ApplySilverlightTriggers(element, (DependencyProperty)field.GetValue(null), x => expression, null, null);
#else
#if !WinRT
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
#endif
            BindingOperations.SetBinding(parameter, Parameter.ValueProperty, binding);
#endif
        }

        static string[] Split(string message, char separator)
        {
            //Splits a string using the specified separator, if it is found outside of relevant places
            //delimited by [ and ]
            string str;
            var list = new List<string>();
            var builder = new StringBuilder();

            int squareBrackets = 0;
#if WinRT
            foreach (var current in message.ToCharArray())
            {
#else
            foreach(var current in message) {
#endif
                //Square brackets are used as delimiters, so only separators outside them count...
                if (current == '[')
                {
                    squareBrackets++;
                }
                else if (current == ']')
                {
                    squareBrackets--;
                }
                else if (current == separator)
                {
                    if (squareBrackets == 0)
                    {
                        str = builder.ToString();
                        if (!string.IsNullOrEmpty(str))
                            list.Add(builder.ToString());
                        builder.Length = 0;
                        continue;
                    }
                }

                builder.Append(current);
            }

            str = builder.ToString();
            if (!string.IsNullOrEmpty(str))
            {
                list.Add(builder.ToString());
            }

            return list.ToArray();
        }

        static string[] SplitParameters(string parameters)
        {
            //Splits parameter string taking into account brackets...
            var list = new List<string>();
            var builder = new StringBuilder();

            bool isInString = false;

            int curlyBrackets = 0;
            int squareBrackets = 0;
            int roundBrackets = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                var current = parameters[i];

                if (current == '"')
                {
                    if (i == 0 || parameters[i - 1] != '\\')
                    {
                        isInString = !isInString;
                    }
                }

                if (!isInString)
                {
                    switch (current)
                    {
                        case '{':
                            curlyBrackets++;
                            break;
                        case '}':
                            curlyBrackets--;
                            break;
                        case '[':
                            squareBrackets++;
                            break;
                        case ']':
                            squareBrackets--;
                            break;
                        case '(':
                            roundBrackets++;
                            break;
                        case ')':
                            roundBrackets--;
                            break;
                        default:
                            if (current == ',' && roundBrackets == 0 && squareBrackets == 0 && curlyBrackets == 0)
                            {
                                //The only commas to be considered as parameter separators are outside:
                                //- Strings
                                //- Square brackets (to ignore indexers)
                                //- Parantheses (to ignore method invocations)
                                //- Curly brackets (to ignore initializers and Bindings)
                                list.Add(builder.ToString());
                                builder.Length = 0;
                                continue;
                            }
                            break;
                    }
                }

                builder.Append(current);
            }

            list.Add(builder.ToString());

            return list.ToArray();
        }
    }
}
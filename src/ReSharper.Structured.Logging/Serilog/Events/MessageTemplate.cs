// Copyright 2013-2015 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using ReSharper.Structured.Logging.Serilog.Parsing;

namespace ReSharper.Structured.Logging.Serilog.Events
{
    /// <summary>
    /// Represents a message template passed to a log method. The template
    /// can subsequently render the template in textual form given the list
    /// of properties.
    /// </summary>
    public class MessageTemplate
    {
        /// <summary>
        /// Represents the empty message template.
        /// </summary>
        public static MessageTemplate Empty { get; } = new MessageTemplate(Enumerable.Empty<MessageTemplateToken>());

        readonly MessageTemplateToken[] _tokens;

        /// <summary>
        /// Construct a message template using manually-defined text and property tokens.
        /// </summary>
        /// <param name="tokens">The text and property tokens defining the template.</param>
        public MessageTemplate(IEnumerable<MessageTemplateToken> tokens)
            // ReSharper disable PossibleMultipleEnumeration
            : this(string.Join("", tokens), tokens)
            // ReSharper enable PossibleMultipleEnumeration
        {
        }

        /// <summary>
        /// Construct a message template using manually-defined text and property tokens.
        /// </summary>
        /// <param name="text">The full text of the template; used by Serilog internally to avoid unneeded
        /// string concatenation.</param>
        /// <param name="tokens">The text and property tokens defining the template.</param>
        public MessageTemplate(string text, IEnumerable<MessageTemplateToken> tokens)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            Text = text;
            _tokens = tokens.ToArray();

            var propertyTokens = GetElementsOfTypeToArray<PropertyToken>(_tokens);
            if (propertyTokens.Length != 0)
            {
                var allPositional = true;
                var anyPositional = false;
                foreach (var propertyToken in propertyTokens)
                {
                    if (propertyToken.IsPositional)
                        anyPositional = true;
                    else
                        allPositional = false;
                }

                if (allPositional)
                {
                    PositionalProperties = propertyTokens;
                }
                else
                {
                    if (anyPositional)
                        IsMixedTemplate = true;

                    NamedProperties = propertyTokens;
                }
            }
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.OfType{TResult}"/>, but faster.
        /// </summary>
        static TResult[] GetElementsOfTypeToArray<TResult>(MessageTemplateToken[] tokens)
            where TResult: class
        {
            var result = new List<TResult>(tokens.Length / 2);
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i] as TResult;
                if (token != null)
                {
                    result.Add(token);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// The raw text describing the template.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Render the template as a string.
        /// </summary>
        /// <returns>The string representation of the template.</returns>
        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        /// The tokens parsed from the template.
        /// </summary>
        public IEnumerable<MessageTemplateToken> Tokens => _tokens;

        internal MessageTemplateToken[] TokenArray => _tokens;

        internal PropertyToken[] NamedProperties { get; }

        internal PropertyToken[] PositionalProperties { get; }

        public bool IsMixedTemplate { get; }
    }
}

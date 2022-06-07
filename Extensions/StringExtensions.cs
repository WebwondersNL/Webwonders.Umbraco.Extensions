﻿using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Cms.Core.Strings;

namespace Webwonders.Extensions.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///  An extension method that returns a new IHtmlEncodedString in which all occurrences of a paragraph
        ///   in the current instance are replaced with another a br element.
        /// </summary>
        /// <param name="value">Current instance of the IHtmlEncodedString</param>
        /// <returns>Updated IHtmlEncodedString</returns>
        public static IHtmlEncodedString ReplaceParagraphs(this IHtmlEncodedString value)
        {
            var newString = "";

            if (value != null)
            {
                var inputString = value.ToString();

                if (!string.IsNullOrEmpty(inputString))
                {
                    newString = inputString.Trim().Replace("<p>", "").Replace("</p>", "<br />");
                }
            }

            return new HtmlEncodedString(newString);
        }

        /// <summary>
        /// An extension method that returns a new IHtmlEncodedString in which all occurrences of a paragraph in the current instance are removed.
        /// </summary>
        /// <param name="value">Current instance of the IHtmlEncodedString</param>
        /// <returns>Updated IHtmlEncodedString</returns>
        public static IHtmlEncodedString RemoveParagraphs(this IHtmlEncodedString value)
        {
            var newString = "";

            if (value != null)
            {
                var inputString = value.ToString();

                if (!string.IsNullOrEmpty(inputString))
                {
                    newString = inputString.Trim().Replace("<p>", "").Replace("</p>", "");
                }
            }

            return new HtmlEncodedString(newString);
        }

        ///<summary>
        /// Indicates whether a specified IHtmlEncodedString is null, empty, or consists only of white-space characters.
        ///</summary>
        ///<param name="value">The value to check.</param>
        ///<returns>Returns true if the value is null, empty, or consists only of white-space characters, otherwise returns false.</returns>
        public static bool IsNullOrWhiteSpace(this IHtmlEncodedString value)
        {
            return string.IsNullOrWhiteSpace(Regex.Replace(HttpUtility.HtmlDecode(value.ToString()), "<.*?>", string.Empty));
        }

    }
}
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DynamicExpresso.Helpers
{
    internal static class TranslateExtension
    {
        /// <summary>
        /// The resource identifier.
        /// </summary>
        const string ResourceId = "DynamicExpresso.Resources.ErrorMessages";

        /// <summary>
        /// The resmgr.
        /// </summary>
        static readonly Lazy<ResourceManager> resmgr =
            new Lazy<ResourceManager>(() => new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        /// <summary>
        /// Translate the text if it fouded in any Resource file
        /// </summary>
        /// <param name="text">The text to be translate</param>
        /// <returns>The translated text</returns>
        public static string TranslateText(string text, CultureInfo cultureInfo)
        {
            if (text == null)
                return "";

            var ci = cultureInfo ?? CultureInfo.InvariantCulture;
            var translation = resmgr.Value.GetString(text, ci);

            if (translation == null)
                translation = resmgr.Value.GetString(text, CultureInfo.InvariantCulture); // returns the key, which GETS DISPLAYED TO THE USER

            return translation;
        }
    }
}
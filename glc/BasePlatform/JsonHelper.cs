using Logger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BasePlatformExtension
{
	/// <summary>
	/// Helper class for dealing with JSON files
	/// </summary>
	public static class CJsonHelper
	{
		/// <summary>
		/// Retrieve string value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as a string or empty string if not found</returns>
		public static string GetStringProperty(JsonElement jElement, string strPropertyName)
		{
			try
			{
				if(jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
				{
					return jValue.GetString();
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e);
			}
			return "";
		}

        /// <summary>
        /// Retrieve unsigned long value from the JSON element
        /// </summary>
        /// <param name="strPropertyName">Name of the property</param>
        /// <param name="jElement">Source JSON element</param>
        /// <returns>Value of the property as a ulong or 0 if not found</returns>
        public static ulong GetULongProperty(JsonElement jElement, string strPropertyName)
        {
            try
            {
                if(jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
                {
                    if(jValue.TryGetUInt64(out ulong nOut)) return nOut;
                }
            }
            catch(Exception e)
            {
                CLogger.LogError(e);
            }
            return 0;
        }
    }
}

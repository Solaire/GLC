using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BasePlatformExtension
{
    public static class CRegistryHelper
    {
		private static readonly string[] ARTICLES =
		{
			"The ",								// English definite
			"A ", "An ",						// English indefinite
			/*
			"El ", "La ", "Los ", "Las ",		// Spanish definite
			"Un ", "Una ", "Unos ", "Unas ",	// Spanish indefinite
			"Le ", "Les ", "L\'",				//, "La" [Spanish] // French definite
			"Une ", "De ", "Des ",				//, "Un" [Spanish] // French indefinite [many French sort with indefinite article]
			"Der", "Das",						//, "Die" [English word] // German definite
			"Ein", "Eine"						// German indefinite
			*/
		};

		/// <summary>
		/// Get a value from the registry if it exists
		/// </summary>
		/// <param name="key">The registry key</param>
		/// <param name="valName">The registry value name</param>
		/// <returns>the value's string data</returns>
		public static string GetRegStrVal(RegistryKey key, string valName)
		{
			try
			{
				object valData = key.GetValue(valName);
				if(valData != null)
				{
					return valData.ToString();
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e);
			}
			return String.Empty;
		}

		/// <summary>
		/// Simplify a string for use as a default alias
		/// </summary>
		/// <param name="title">The game's title</param>
		/// <returns>simplified string</returns>
		public static string GetAlias(string title)
		{
			string alias = title.ToLower();
			/*
			foreach (string prep in new List<string> { "for", "of", "to" })
			{
				if (alias.StartsWith(prep + " "))
					alias = alias.Substring(prep.Length + 1);
			}
			*/
			foreach(string art in ARTICLES)
			{
				if(alias.StartsWith(art + " "))
				{
					alias = alias.Substring(art.Length + 1);
				}
			}
			alias = new string(alias.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !char.IsSymbol(c)).ToArray());
			return alias;
		}

		/// <summary>
		/// Find game folders in the registry.
		/// This method will look for and return folders that match the input string.
		/// </summary>
		/// <param name="root">Root directory that will be scanned</param>
		/// <param name="strFolder">Target game folder</param>
		/// <returns>List of Reg keys with game folders</returns>
		public static List<RegistryKey> FindGameFolders(RegistryKey root, string strFolder)
		{
			LinkedList<RegistryKey> toCheck = new LinkedList<RegistryKey>();
			List<RegistryKey> gameKeys = new List<RegistryKey>();

			toCheck.AddLast(root);

			while(toCheck.Count > 0)
			{
				root = toCheck.First.Value;
				toCheck.RemoveFirst();

				if(root != null)
				{
					foreach(var sub in root.GetSubKeyNames())
					{
						if(!(sub.Equals("Microsoft")))
						{
							if(string.IsNullOrEmpty(strFolder) || sub.IndexOf(strFolder, StringComparison.OrdinalIgnoreCase) >= 0)
							{
								gameKeys.Add(root.OpenSubKey(sub, RegistryKeyPermissionCheck.ReadSubTree));
							}
						}
					}
				}
			}
			return gameKeys;
		}

		/// <summary>
		/// Find game keys in specified root
		/// Looks for a key-value pair inside the specified root.
		/// </summary>
		/// <param name="root">Root folder that will be scanned</param>
		/// <param name="strKey">The target key that should contain the target value</param>
		/// <param name="strValue">The target value in the key</param>
		/// <param name="ignore">Function will ignore these folders (used to ignore things like launchers)</param>
		/// <returns>List of game registry keys</returns>
		public static List<RegistryKey> FindGameKeys(RegistryKey root, string strKey, string strValue, string[] ignore)
		{
			LinkedList<RegistryKey> toCheck = new LinkedList<RegistryKey>();
			List<RegistryKey> gameKeys = new List<RegistryKey>();

			toCheck.AddLast(root);

			while(toCheck.Count > 0)
			{
				root = toCheck.First.Value;
				toCheck.RemoveFirst();

				if(root != null)
				{
					foreach(var name in root.GetValueNames())
					{
						if(root.GetValueKind(name) == RegistryValueKind.String && name == strValue)
						{
							if(((string)root.GetValue(name)).Contains(strKey))
							{
								gameKeys.Add(root);
								break;
							}
						}
					}

					foreach(var sub in root.GetSubKeyNames()) // Add subkeys to search list
					{
						if(!(sub.Equals("Microsoft"))) // Microsoft folder only contains system stuff and it doesn't need searching
						{
							foreach(var entry in ignore)
							{
								if(!(sub.Equals(entry.ToString())))
								{
									try
									{
										toCheck.AddLast(root.OpenSubKey(sub, RegistryKeyPermissionCheck.ReadSubTree));
									}
									catch(Exception e)
									{
										CLogger.LogError(e);
									}
								}
							}
						}
					}
				}
			}
			return gameKeys;
		}

		/// <summary>
		/// Get a value from the registry if it exists
		/// </summary>
		/// <param name="key">The registry key</param>
		/// <param name="valName">The registry value name</param>
		/// <returns>the value's DWORD (UInt32) data</returns>
		public static int? GetRegDWORDVal(RegistryKey key, string valName)
		{
			try
			{
				object valData = key.GetValue(valName);
				/*
				Type valType = valData.GetType();
				if (valData != null && valType == typeof(int))
				{
				*/
				if(int.TryParse(valData.ToString(), out int result))
				{
					return result;
				}
				//}
			}
			catch(Exception e)
			{
				CLogger.LogError(e);
			}
			return null;
		}
	}
}

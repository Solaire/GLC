using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace IniParser
{
	//https://stackoverflow.com/questions/217902/reading-writing-an-ini-file
	/// <summary>
	/// This class is used to read / write a .ini configuration file. The file will contain the following:
	///		Fronend config: Size and location sof controls. Color and image settings for custom images.
	///		Backend config: Detected platforms will be checked and updated each time the program is executed.
	/// </summary>
	public class CIniParser
	{
		private string m_filePath; // file path
		private string m_EXE = Assembly.GetExecutingAssembly().GetName().Name; // file name

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern int GetPrivateProfileString(string section, string key, string _default, StringBuilder output, int size, string filePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		private static extern int GetPrivateProfileSection(string section, byte[] returnBuffe, int size, string fileName);

		/// <summary>
		/// Constructor: Open .ini file. If file doesn't exist, create new and set default settings. Update installed platforms
		/// </summary>
		/// <param name="iniPath">File name/path. If null, create in .exe directory with name of .exe file</param>
		public CIniParser(string iniPath = null)
		{
			if (!File.Exists(iniPath ?? m_EXE + ".ini"))
			{
				m_filePath = new FileInfo(iniPath ?? m_EXE + ".ini").FullName.ToString();
				return;
			}
			m_filePath = new FileInfo(iniPath ?? m_EXE + ".ini").FullName.ToString();
		}

		/// <summary>
		/// Read key and return value
		/// </summary>
		/// <param name="key">Target key</param>
		/// <param name="section">Section to look in. If null, it will look in default section [GLC]</param>
		/// <returns>Value as a string</returns>
		public string Read(string key, string section = null)
		{
			var output = new StringBuilder(255);
			GetPrivateProfileString(section ?? m_EXE, key, "", output, 255, m_filePath);
			return output.ToString();
		}

		/// <summary>
		/// Read key and return value
		/// </summary>
		/// <param name="key">Target key</param>
		/// <param name="section">ection to look in. If null, it will look in default section [GLC]</param>
		/// <returns>Value as an int</returns>
		public int ReadAsNumber(string key, string section = null)
		{
			var output = new StringBuilder(255);
			GetPrivateProfileString(section ?? m_EXE, key, "", output, 255, m_filePath);
			return Int32.Parse(output.ToString());
		}

		/// <summary>
		/// Read key and return values.
		/// </summary>
		/// <param name="key">Target key</param>
		/// <param name="section">Section to look in. If null, it will look in default section [GLC]</param>
		/// <returns>Values an an array of strings</returns>
		public int[] ReadAsArray(string key, string section = null)
		{
			var output = new StringBuilder(255);
			GetPrivateProfileString(section ?? m_EXE, key, "", output, 255, m_filePath);

			string[] strings = output.ToString().Split('x');
			return new int[] { Int32.Parse(strings[0]), Int32.Parse(strings[1]) };
		}

		/// <summary>
		/// Write to .ini file
		/// </summary>
		/// <param name="key">Key to add/append</param>
		/// <param name="value">Value to add/append</param>
		/// <param name="section">Section to write in. If null, it will write in the default section [GLC]</param>
		public void Write(string key, string value, string section = null)
		{
			WritePrivateProfileString(section ?? m_EXE, key, value, m_filePath);
		}

		/// <summary>
		/// Delete key from .ini file
		/// </summary>
		/// <param name="key">Key to delete</param>
		/// <param name="section">Section to look in. If null, it will look in default section [GLC]</param>
		public void DeleteKey(string key, string section = null)
		{
			Write(key, null, section ?? m_EXE);
		}

		/// <summary>
		/// Delete section from .ini file
		/// </summary>
		/// <param name="section">Section to delete. If null, it will delete the default section [GLC]</param>
		public void DeleteSection(string section = null) // TO update icon sizes to match any resolution
		{
			Write(null, null, section ?? m_EXE);
		}

		/// <summary>
		/// Check if key exists
		/// </summary>
		/// <param name="key">Target key</param>
		/// <param name="section">Section to look in. If null, it will look in default section [GLC]</param>
		/// <returns>Boolean. True if key exists</returns>
		public bool KeyExists(string key, string section = null)
		{
			return Read(key, section).Length > 0;
		}

		/// <summary>
		/// Insert a comment into the specified file section
		/// </summary>
		/// <param name="message">Comment message</param>
		/// <param name="key">Key associated with the comment</param>
		/// <param name="section">Section associated with the comment</param>
		public void WriteComment(string message, string key = null, string section = null)
		{
			WritePrivateProfileString(section ?? m_EXE, ";" + key ?? "", message, m_filePath);
		}

		/// <summary>
		/// Read and return section block
		/// </summary>
		/// <param name="section">Section to read. If null, default section will be read [GLC]</param>
		/// <returns>List of values in the section</returns>
		public List<string> ReadSection(string section = null)
		{
			byte[] buffer = new byte[2048];
			GetPrivateProfileSection(section ?? m_EXE, buffer, 2048, m_filePath);
			string[] temp = Encoding.Unicode.GetString(buffer).Trim('\0').Split('\0');

			List<string> output = new List<string>();

			foreach (string s in temp)
			{
				output.Add(s);
			}
			return output;
		}
	}
}
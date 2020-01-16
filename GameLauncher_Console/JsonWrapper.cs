using System.Collections.Generic;
using System.IO;

namespace GameLauncher_Console
{
	/// <summary>
	/// Class for serializing and deserializing JSON data. 
	/// JSON data is stored and handled in a dynamically sized dictionary structure.
	/// </summary>
	class CJsonWrapper
	{
		class TestObject
		{
			public int		intItem { get; set; }
			public string	strItem { get; set; }
			public bool		boolItem { get; set; }
		}
		
		/* TO IMPLEMENT:
		 *  Load JSON file;
		 *  Save File to a JSON format;
		 *  Deserialize JSON data into a dictionary
		 *  Serialize JSON data from the dictionary into the JSON file
		 *  Create JSON Data [Add(), Remove(), Replace()/Edit(), Read()]
		 *  Access JSON Data [MemberAsX(), IsMember(), IsTypeOfX()]
		 */ 

		/* NOTES:
		 * Look into different data structures (list, linkedList, Dictionary, Map, array)
		 * Look into meta-programming (templates\generics)
		 * Look into dynamic object generation
		 * Essentially, the data must be broken down and stored into one of the following types(int, float/double, string, char(treat as string), bool)
		 *	Should be easy to distinguish:
		 *		* string/char has quotation mark
		 *		* int is a whole number
		 *		* float/double has a decimal point
		 *		* bool is true false (no quotation marks)
		 *	Not sure about support for binary data (0xFFF and such)
		 */
	}
}

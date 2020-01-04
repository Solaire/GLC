using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameLauncherDock.Logic
{
	class CXML
	{
		protected XDocument m_xDoc;
		protected string m_strDocumentPath;
		protected string m_strRootNode;

		/// <summary>
		/// Constructor
		/// Set the document path to the application's directory
		/// </summary>
		protected CXML(string strRootNode)
		{
			m_strRootNode = strRootNode;
			m_strDocumentPath = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".xml";
		}

		/// <summary>
		/// Check if the XML document exists in the selected path
		/// </summary>
		/// <returns>True if document exists, otherwise false</returns>
		protected virtual bool Exists(string strDocumentPath)
		{
			return System.IO.File.Exists(strDocumentPath);
		}

		/// <summary>
		/// Create empty XML document
		/// </summary>
		/// <returns>True is document is created successfully, otherwise false</returns>
		protected virtual bool CreateEmptyXML()
		{
			m_xDoc = new XDocument(new XDeclaration("1.0", "UTF-8", null), new XElement(m_strRootNode));
			return Exists(m_strDocumentPath);
		}

		/// <summary>
		/// Check if the XML document is empty
		/// </summary>
		/// <returns>True if document is empty, otherwise false</returns>
		protected virtual bool IsEmpty()
		{
			// Looks like a hack, but no performance hit here.
			// If there is at least one element, the function will return false after one loop iteration
			// If there are no elements, the loop will not iterate at all (there will be 0 iterators)
			foreach(XElement element in m_xDoc.Descendants(m_strRootNode))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Edit a node in the XML document
		/// </summary>
		/// <param name="strSearchKey">Key that contains the search value</param>
		/// <param name="strSearchValue">Value to be edited</param>
		/// <param name="strEditKey">Key that holds the valus to be edited</param>
		/// <param name="strEditValue">New key value</param>
		public virtual void EditValue(string strSearchKey, string strSearchValue, string strEditKey, string strEditValue)
		{
			foreach(XElement element in m_xDoc.Element(m_strRootNode).Nodes())
			{
				string s = element.Element(strSearchKey).Value;

				if(s == strSearchValue)
				{
					element.Element(strEditKey).Value = strEditValue;
					m_xDoc.Save(m_strDocumentPath);
					return;
				}
			}
		}

		/// <summary>
		/// Delete node from the XML document
		/// </summary>
		/// <param name="strKey">Key that will be depeted</param>
		/// <param name="strValue">Search value that will be deleted</param>
		public virtual void RemoveNode(string strKey, string strValue)
		{
			foreach(XElement element in m_xDoc.Element(m_strRootNode).Nodes())
			{
				string s = element.Element(strKey).Value;
				if(s == strValue)
				{
					element.Remove();
					m_xDoc.Save(m_strDocumentPath);
					return;
				}
			}
		}
	}
}

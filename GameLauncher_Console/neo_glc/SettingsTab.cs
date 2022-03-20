using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NStack;
using Terminal.Gui;
using core;

namespace glc
{
	public class CSettingsTab : TabView.Tab
	{
		private static View m_container;

		private static CSettingsCategoriesPanel m_settingCategories;
		private static CSettingsValuesPanel m_settingValues;

		public CSettingsTab()
			: base()
		{
			Text = "Settings";
			m_settingCategories = new CSettingsCategoriesPanel("Categories", 0, 0, Dim.Percent(40), Dim.Fill(), true, Key.CtrlMask | Key.C);
			m_settingValues = new CSettingsValuesPanel(SettingCategory.cGeneral, Pos.Percent(40), 0, Dim.Fill(), Dim.Fill(), true, Key.CtrlMask | Key.C);

			// Hook up the triggers
			// Event triggers for the list view
			m_settingCategories.ContainerView.OpenSelectedItem += Categories_OpenSelectedItem;
			m_settingCategories.ContainerView.SelectedItemChanged += Categories_SelectedChanged;

			m_settingValues.ContainerView.OpenSelectedItem += Values_OpenSelectedItem;
			m_settingValues.ContainerView.SelectedItemChanged += Values_SelectedChanged;

			// Container to store all frames
			m_container = new View()
			{
				X = 0,
				Y = 0, // for menu
				Width = Dim.Fill(),
				Height = Dim.Fill(),
				CanFocus = false,
			};
			m_container.Add(m_settingCategories.FrameView);
			m_container.Add(m_settingValues.FrameView);

			View = m_container;
		}

		/// <summary>
		/// Handle game selection event
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void Categories_OpenSelectedItem(ListViewItemEventArgs e)
		{
			m_settingValues = new CSettingsValuesPanel(m_settingCategories.ContentList[m_settingCategories.ContainerView.SelectedItem], Pos.Percent(40), 0, Dim.Fill(), Dim.Fill(), true, Key.CtrlMask | Key.C);
		}

		private static void Categories_SelectedChanged(ListViewItemEventArgs e)
		{
			m_settingValues = new CSettingsValuesPanel(m_settingCategories.ContentList[m_settingCategories.ContainerView.SelectedItem], Pos.Percent(40), 0, Dim.Fill(), Dim.Fill(), true, Key.CtrlMask | Key.C);
		}

		/// <summary>
		/// Handle game selection event
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void Values_OpenSelectedItem(ListViewItemEventArgs e)
		{

		}

		private static void Values_SelectedChanged(ListViewItemEventArgs e)
		{

		}
	}
}
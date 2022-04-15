using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NStack;
using Terminal.Gui;
using core;
using glc.Settings;

namespace glc
{
	public class CSettingsTab : TabView.Tab
	{
		private static View m_container;

		private static CSettingsCategoriesPanel m_settingCategoryPanel;
		private static CSettingsEditPanel m_settingEditPanel;

		public CSettingsTab()
			: base()
		{
			Text = "Settings";
			m_settingCategoryPanel = new CSettingsCategoriesPanel("Categories", 0, 0, Dim.Percent(40), Dim.Fill(), true);
			m_settingEditPanel = new CSettingsEditPanel("SystemAttribute", Pos.Percent(40), 0, Dim.Fill(), Dim.Fill(), true);

			// Hook up the triggers
			// Event triggers for the list view
			m_settingCategoryPanel.ContainerView.OpenSelectedItem += Categories_OpenSelectedItem;
			m_settingCategoryPanel.ContainerView.SelectedItemChanged += Categories_SelectedChanged;

			m_settingEditPanel.ContainerView.OpenSelectedItem += Values_OpenSelectedItem;

			// Container to store all frames
			m_container = new View()
			{
				X = 0,
				Y = 0, // for menu
				Width = Dim.Fill(),
				Height = Dim.Fill(),
				CanFocus = false,
			};
			m_container.Add(m_settingCategoryPanel.FrameView);
			m_container.Add(m_settingEditPanel.FrameView);

			View = m_container;
		}

		/// <summary>
		/// Handle game selection event
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void Categories_OpenSelectedItem(ListViewItemEventArgs e)
		{
			m_settingEditPanel.FrameView.SetFocus();
		}

		private static void Categories_SelectedChanged(ListViewItemEventArgs e)
		{
			m_settingEditPanel.LoadCategory(m_settingCategoryPanel.ContentList[m_settingCategoryPanel.ContainerView.SelectedItem]);
		}

		/// <summary>
		/// Handle game selection event
		/// </summary>
		/// <param name="e">The event argument</param>
		private static void Values_OpenSelectedItem(ListViewItemEventArgs e)
		{
			m_settingEditPanel.EditValue();
		}
	}
}
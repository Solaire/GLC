using System.Collections.Generic;
using System.Linq;

using core;
using core.Platform;
using core.Tag;

namespace glc.Settings
{
    public class CPlatformSettingsContainer : CGenericSettingContainer<CBasicPlatform>
    {
        public CPlatformSettingsContainer()
        {
            DataList = CPlatformSQL.ListPlatforms();
            DataSource = new CDataNodeDataSource<CBasicPlatform>(DataList);
        }

        public override void EditNode(int selectionIndex)
        {
			CBasicPlatform selected = DataList[selectionIndex];

			List<TagObject> tempTagList = CTagSQL.GetTagsforPlatform(selected.PrimaryKey);
			List<IDataNode> currentTags = new List<IDataNode>(tempTagList.Cast<IDataNode>());

			CEditSelectionDlg<CBasicPlatform> dlg = new CEditSelectionDlg<CBasicPlatform>(selected, currentTags);

			if(dlg.Run(ref selected))
            {
                CPlatformSQL.ToggleActive(selected.PrimaryKey, selected.IsEnabled);

                DataList[selectionIndex] = selected;
                DataSource.ToList()[selectionIndex] = selected;
            }

			if(dlg.IsOkayPressed() && dlg.IsSelectionDirty())
            {
				List<int> enabledTags = new List<int>();
				foreach(IDataNode tag in currentTags)
				{
					if(tag.IsEnabled)
					{
						enabledTags.Add(tag.PrimaryKey);
					}
				}

				CPlatformSQL.SetTags(selected.PrimaryKey, enabledTags);
			}
        }

        public override void SelectNode(int selectionIndex)
        {
            EditNode(selectionIndex);
        }
    }
}

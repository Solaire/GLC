using System;
using System.Collections.Generic;

using core;

namespace glc.Settings
{
    public class CGeneralSettings : CSettings<CSystemAttributeSQL.SystemAttributeNode>
    {
        public CGeneralSettings()
            : base()
        {

        }

        public override void Save(CSystemAttributeSQL.SystemAttributeNode node)
        {
            CSystemAttributeSQL.UpdateNode(node);
        }

        protected override void Load()
        {
            Settings = CSystemAttributeSQL.GetAllNodes();
        }
    }
}

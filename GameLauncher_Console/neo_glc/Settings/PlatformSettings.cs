using core;

namespace glc.Settings
{
    public class CPlatformSettings : CSettings<CBasicPlatform>
    {
		public CPlatformSettings()
            : base()
        {

        }

        public override void Save(CBasicPlatform platform)
        {
            CPlatformSQL.ToggleActive(platform.ID, platform.IsActive);
        }

        protected override void Load()
        {
            Settings = CPlatformSQL.ListPlatforms();
        }
    }
}

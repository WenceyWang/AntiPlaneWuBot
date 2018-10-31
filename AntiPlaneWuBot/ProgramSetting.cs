using System ;
using System . Collections ;
using System . Collections . Generic ;
using System . Linq ;

using DreamRecorder . ToolBox . CommandLine ;

namespace WenceyWang . AntiPlaneWuBot
{

	public class ProgramSetting : SettingBase <ProgramSetting , ProgramSettingCatalog>
	{

		[SettingItem ( ( int ) ProgramSettingCatalog . Bot , nameof(BotToken) , "" , true , "" )]
		public string BotToken { get ; set ; }

		[SettingItem ( ( int ) ProgramSettingCatalog . Bot , nameof(HttpProxy) , "Http/s Proxy to Use." , true , null )]
		public string HttpProxy { get ; set ; }

	}

}

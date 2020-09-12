using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ChatSource
{
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static Config Instance => ModContent.GetInstance<Config>();

		[Tooltip("If this mods' main feature should work")]
		[Label("Chat Source Enabled")]
		[DefaultValue(true)]
		public bool ChatSourceEnabled;

		[Tooltip("If messages from the vanilla game should be marked as such")]
		[Label("Show Vanilla As Source")]
		[DefaultValue(false)]
		public bool DisplayTerrariaSource;

		[Tooltip("If mods should show with their 'Display Name' or 'Internal Name'")]
		[Label("Show 'Display Name'")]
		[DefaultValue(false)]
		public bool ShowDisplayName;
	}
}

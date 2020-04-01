using Terraria.ModLoader;

namespace ChatSource
{
    public class ModNameCommand : ModCommand
    {
        #region Custom stuff

        private const string COMMANDNAME = "CSmodname";
        private const string ARGUMENT = "internalName";

        public static string CommandStart
        {
            get
            {
                return "/" + COMMANDNAME + " ";
            }
        }
        #endregion

        public override CommandType Type
        {
            get
            {
                return CommandType.Chat;
            }
        }

        public override string Command
        {
            get
            {
                return COMMANDNAME;
            }
        }

        public override string Usage
        {
            get
            {
                return "/" + COMMANDNAME + " " + ARGUMENT;
            }
        }

        public override string Description
        {
            get
            {
                return "Retreive the display name of a mod based on its internal name.";
            }
        }

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length < 1) caller.Reply(Usage);
            else
            {
                try
                {
                    string internalName = args[0];
                    Mod mod = ModLoader.GetMod(internalName);
                    if (mod != null)
                    {
                        caller.Reply($"{internalName}: {mod.DisplayName}");
                    }
                    else
                    {
                        caller.Reply("Given mod doesn't exist, check your spelling");
                    }
                }
                catch
                {

                }
            }
        }
    }
}

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatSource
{
    //public class MPlayer : ModPlayer
    //{
    //    public override bool Shoot(Item item, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
    //    {
    //        Main.NewText("s", Color.White);
    //        return base.Shoot(item, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
    //    }
    //}

    public class ChatSource : Mod
    {
		public ChatSource()
        {

        }

        public override void Load()
        {
            On.Terraria.Main.NewText_string_byte_byte_byte_bool += Main_NewText_string_byte_byte_byte_bool;
            On.Terraria.Main.NewText_List1 += Main_NewText_List1;
        }

        private static void Main_NewText_List1(On.Terraria.Main.orig_NewText_List1 orig, List<TextSnippet> snippets)
        {
            orig(snippets);

            if (snippets.Count <= 0)
                return;

            ModifyLastChatMessage();
        }

        private static void Main_NewText_string_byte_byte_byte_bool(On.Terraria.Main.orig_NewText_string_byte_byte_byte_bool orig, string newText, byte R, byte G, byte B, bool force)
        {
            orig(newText, R, G, B, force);

            ModifyLastChatMessage();
        }

        private static void ModifyLastChatMessage()
        {
            string name = GetCallingName(true);
            if (name == string.Empty)
                return;

            var parse = Main.chatLine[0].parsedText;
            if (parse.Length <= 0)
                return;

            parse[0].Text = name + parse[0].Text;
        }

        private static string GetCallingName(bool whitespace = false)
        {
            string name = string.Empty;
            if (!Config.Instance.ChatSourceEnabled)
                return string.Empty;

            try
            {
                var frames = new StackTrace(true).GetFrames();
                Logging.PrettifyStackTraceSources(frames);
                int index = 2;
                while (frames[index].GetMethod().Name.Contains("NewText"))
                    index++;
                var frame = frames[index];
                var method = frame.GetMethod();
                name = method.DeclaringType.Namespace;
                name = name.Split('.')[0];
                if (!Config.Instance.DisplayTerrariaSource && name == "Terraria")
                    name = string.Empty;
            }
            catch
            {

            }
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            return $"[{name}]" + (whitespace ? " " : "");
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using System.Reflection;
using System;

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
        public override void Load()
        {
            On.Terraria.Main.NewText_string_byte_byte_byte_bool += Main_NewText_string_byte_byte_byte_bool;
            On.Terraria.Main.NewText_List1 += Main_NewText_List1;

            if (activelyModdingField == null)
            {
                try
                {
                    //Terraria.ModLoader.Core: internal class ModCompile: public static bool activelyModding;
                    Assembly ModLoaderAssembly = typeof(ModLoader).Assembly;
                    Type ModCompileType = ModLoaderAssembly.GetType("Terraria.ModLoader.Core.ModCompile");
                    activelyModdingField = ModCompileType.GetField("activelyModding", BindingFlags.Public | BindingFlags.Static);
                }
                catch
                {

                }
            }
        }

        public override void Unload()
        {
            activelyModdingField = null;
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
            if (Main.gameMenu) return;

            /* this is all because of tmls ExceptionHandler doing stuff and printing it to chat when in debug/modder mode, causing some error with the stacktrace
            [19:03:00] [1/WARN] [tML]: Silently Caught Exception: 
            System.BadImageFormatException: OutOfBoundsRead
               at System.Reflection.Throw.OutOfBounds()
               at System.Reflection.Metadata.Ecma335.MethodDebugInformationTableReader.GetSequencePoints(MethodDebugInformationHandle handle)
               at System.Diagnostics.StackTraceSymbols.GetSourceLineInfoWithoutCasAssert(String assemblyPath, IntPtr loadedPeAddress, Int32 loadedPeSize, IntPtr inMemoryPdbAddress, Int32 inMemoryPdbSize, Int32 methodToken, Int32 ilOffset, String& sourceFile, Int32& sourceLine, Int32& sourceColumn)
               at System.Diagnostics.StackFrameHelper.InitializeSourceInfo(Int32 iSkip, Boolean fNeedFileInfo, Exception exception)
               at System.Diagnostics.StackTrace.CaptureStackTrace(Int32 iSkip, Boolean fNeedFileInfo, Thread targetThread, Exception e)
               at ChatSource.ChatSource.GetCallingName(Boolean whitespace) in ChatSource.cs:line 74
               at ChatSource.ChatSource.ModifyLastChatMessage() in ChatSource.cs:line 51
               at ChatSource.ChatSource.Main_NewText_string_byte_byte_byte_bool(orig_NewText_string_byte_byte_byte_bool orig, String newText, Byte R, Byte G, Byte B, Boolean force) in ChatSource.cs:line 47
               at DMD<DMD<Hook<Terraria.Main::NewText>?34669516>?51491948::Hook<Terraria.Main::NewText>?34669516>(String , Byte , Byte , Byte , Boolean )
               at DMD<Terraria.Main::NewText>(String newText, Byte R, Byte G, Byte B, Boolean force)
               at DMD<DMD<Trampoline<Terraria.Main::NewText>?39771549>?45271378::Trampoline<Terraria.Main::NewText>?39771549>(String , Byte , Byte , Byte , Boolean )
               at ChatSource.ChatSource.Main_NewText_string_byte_byte_byte_bool(orig_NewText_string_byte_byte_byte_bool orig, String newText, Byte R, Byte G, Byte B, Boolean force) in ChatSource.cs:line 46
               at DMD<DMD<Hook<Terraria.Main::NewText>?34669516>?51491948::Hook<Terraria.Main::NewText>?34669516>(String , Byte , Byte , Byte , Boolean )
               at AlchemistNPC.AlchemistNPCPlayer.OnEnterWorld(Player player) in AlchemistNPCPlayer.cs:line 499
            */

            //I wish I didn't have to do this. Should have no implications for non-modders

            bool setToFalse = false;

            if ((bool?)activelyModdingField?.GetValue(null) == true)
            {
                activelyModdingField.SetValue(null, false);
                setToFalse = true;
            }

            string name = GetCallingName(true);

            if (setToFalse)
            {
                activelyModdingField.SetValue(null, true);
            }

            if (name == string.Empty)
                return;

            var parse = Main.chatLine[0].parsedText;
            if (parse.Length <= 0)
                return;

            if (parse[0].Text.StartsWith(name))
                return;

            parse[0].Text = name + parse[0].Text;
        }

        public static FieldInfo activelyModdingField;

        private static string GetCallingName(bool whitespace = false)
        {
            string name = string.Empty;
            if (!Config.Instance.ChatSourceEnabled)
                return string.Empty;

            StackFrame[] frames/* = new StackFrame[1]*/;
            try
            {
                frames = new StackTrace(true).GetFrames();
                Logging.PrettifyStackTraceSources(frames);
                int index = 2;
                while (index < frames.Length && frames[index].GetMethod().Name.Contains("NewText"))
                    index++;
                if (index == frames.Length)
                    name = string.Empty;
                else
                {
                    var frame = frames[index];
                    var method = frame.GetMethod();
                    name = method.DeclaringType.Namespace;
                    name = name.Split('.')[0];
                    if (name != "Terraria")
                    {
                        if (Config.Instance.ShowDisplayName)
                        {
                            Mod mod = ModLoader.GetMod(name);
                            if (mod == null)
                            {
                                mod = ModLoader.GetMod(name + "Mod");
                            }
                            if (mod != null)
                            {
                                name = mod.DisplayName;
                            }
                        }
                    }
                    else if(!Config.Instance.DisplayTerrariaSource)
                    {
                        name = string.Empty;
                    }
                }
            }
            catch
            {
                //var logger = ModContent.GetInstance<ChatSource>().Logger;
                //logger.Info("#####");
                //foreach (var frame in frames)
                //{
                //    logger.Info(frame?.ToString() ?? "frame null");
                //}
                //logger.Info("#####");
            }
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            return $"[{name}]" + (whitespace ? " " : "");
        }
    }
}

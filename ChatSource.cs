using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatSource
{
	public class ChatSource : Mod
	{
		public override void Load()
		{
			On_RemadeChatMonitor.AddNewMessage += RemadeChatMonitor_AddNewMessage;

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

		private static void RemadeChatMonitor_AddNewMessage(On_RemadeChatMonitor.orig_AddNewMessage orig, RemadeChatMonitor self, string text, Color color, int widthLimitInPixels)
		{
			orig(self, text, color, widthLimitInPixels);

			ModifyLastChatMessage();
		}

		public override void Unload()
		{
			activelyModdingField = null;
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
			//End of shenanigans

			string name = GetCallingName(true);

			if (setToFalse)
			{
				activelyModdingField.SetValue(null, true);
			}

			if (name == string.Empty)
				return;

			if (Main.chatMonitor is RemadeChatMonitor chat)
			{
				//All classes public
				//RemadeChatMonitor has private List<ChatMessageContainer> _messages;
				//ChatMessageContainer has private List<TextSnippet[]> _parsedText;
				//TextSnippet has public string Text;
				FieldInfo messagesField = typeof(RemadeChatMonitor).GetField("_messages", BindingFlags.Instance | BindingFlags.NonPublic);
				List<ChatMessageContainer> messages = messagesField.GetValue(chat) as List<ChatMessageContainer>;

				FieldInfo parsedTextField = typeof(ChatMessageContainer).GetField("_parsedText", BindingFlags.Instance | BindingFlags.NonPublic);

				var lastMessage = messages[0];

				List<TextSnippet[]> parsedText = parsedTextField.GetValue(lastMessage) as List<TextSnippet[]>;

				if (parsedText.Count <= 0)
					return;

				var snippet = parsedText[0];

				//OriginalText because vanilla recalculates parsedText on window resize based on OriginalText
				var textOriginal = lastMessage.OriginalText;

				if (textOriginal.StartsWith(name))
					return;

				var firstWord = snippet[0];

				if (firstWord.Text.StartsWith(name))
					return;

				firstWord.Text = name + firstWord.Text;
				lastMessage.OriginalText = name + lastMessage.OriginalText;
			}
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
				//We want to find the call after the first found, last NewText or AddNewMessage
				int index;
				bool correctSequenceFound = false;
				for (index = 0; index < frames.Length; index++)
				{
					var method = frames[index].GetMethod();
					var methodName = method.Name;
					if (methodName.Contains("NewText") || methodName.Contains("AddNewMessage"))
					{
						correctSequenceFound = true;
					}
					else if (correctSequenceFound)
					{
						break; //Done
					}
				}

				if (index == frames.Length)
				{
					name = string.Empty;
				}
				else
				{
					var frame = frames[index];
					var method = frame.GetMethod();

					Type declaringType = method.DeclaringType;
					if (declaringType != null)
					{
						name = declaringType.Namespace;
						name = name.Split('.')[0];
					}
					else
					{
						//Autogenerated methods (i.e. MonoMod detours) do not have a declaring type, default to Terraria
						name = "Terraria";
					}

					if (name != "Terraria")
					{
						if (Config.Instance.ShowDisplayName)
						{
							if (ModLoader.TryGetMod(name, out Mod mod))
							{
								name = mod.DisplayName;
							}
							else
							{
								if (ModLoader.TryGetMod(name + "Mod", out Mod mod2))
								{
									name = mod2.DisplayName;
								}
							}
						}
					}
					else if (!Config.Instance.DisplayTerrariaSource)
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

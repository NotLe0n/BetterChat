using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Localization.IME;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace BetterChat;

internal class ChatEdits : ModSystem
{
	public override void Load()
	{
		base.Load();

		On_Main.GetInputText += Main_GetInputText;
		On_Main.DrawPlayerChat += Main_DrawPlayerChat;
		On_Main.DoUpdate_HandleChat += Main_DoUpdate_HandleChat;
	}

	private static readonly FieldInfo __backSpaceRate = typeof(Main).GetField("backSpaceRate", BindingFlags.NonPublic | BindingFlags.Static);
	private static readonly FieldInfo __backSpaceCount = typeof(Main).GetField("backSpaceCount", BindingFlags.NonPublic | BindingFlags.Static);

	//private int cursorPosition;
	private string Main_GetInputText(On_Main.orig_GetInputText orig, string oldString, bool allowMultiLine)
	{
		if (Main.dedServ)
			return "";

		if (!Main.hasFocus)
			return oldString;

		Main.inputTextEnter = false;
		Main.inputTextEscape = false;
		string text = oldString;
		string text2 = "";
		if (text == null)
			text = "";

		bool flag = false;
		bool isAltPressed = Main.inputText.IsKeyDown(Keys.LeftAlt) || Main.inputText.IsKeyDown(Keys.RightAlt); // new!
		bool isCtrlPressed = Main.inputText.IsKeyDown(Keys.LeftControl) || Main.inputText.IsKeyDown(Keys.RightControl); // new!

		//if (Main.inputText.IsKeyDown(Keys.LeftControl) || Main.inputText.IsKeyDown(Keys.RightControl)) {
		if (isCtrlPressed && !isAltPressed) { // new!
			if (Main.inputText.IsKeyDown(Keys.Z) && !Main.oldInputText.IsKeyDown(Keys.Z)) {
				text = "";
			}
			else if (Main.inputText.IsKeyDown(Keys.X) && !Main.oldInputText.IsKeyDown(Keys.X)) {
				Platform.Get<IClipboard>().Value = oldString;
				text = "";
			}
			else if ((Main.inputText.IsKeyDown(Keys.C) && !Main.oldInputText.IsKeyDown(Keys.C)) || (Main.inputText.IsKeyDown(Keys.Insert) && !Main.oldInputText.IsKeyDown(Keys.Insert))) {
				Platform.Get<IClipboard>().Value = oldString;
			}
			else if (Main.inputText.IsKeyDown(Keys.V) && !Main.oldInputText.IsKeyDown(Keys.V)) {
				text2 = PasteTextIn(allowMultiLine, text2);
			}
		}
		else {
			if (Main.inputText.PressingShift()) {
				if (Main.inputText.IsKeyDown(Keys.Delete) && !Main.oldInputText.IsKeyDown(Keys.Delete)) {
					Platform.Get<IClipboard>().Value = oldString;
					text = "";
				}

				if (Main.inputText.IsKeyDown(Keys.Insert) && !Main.oldInputText.IsKeyDown(Keys.Insert))
					text2 = PasteTextIn(allowMultiLine, text2);
			}

			for (int i = 0; i < Main.keyCount; i++) {
				int num = Main.keyInt[i];
				string str = Main.keyString[i];
				if (num == 13)
					Main.inputTextEnter = true;
				else if (num == 27)
					Main.inputTextEscape = true;
				else if (num >= 32 && num != 127)
					text2 += str;
			}
		}

		Main.keyCount = 0;
		if (text2 != "") {
			text += text2; ;
			//text = text.Insert(cursorPosition, text2);
			//cursorPosition += text2.Length;
		}

		Main.oldInputText = Main.inputText;
		Main.inputText = Keyboard.GetState();
		Keys[] pressedKeys = Main.inputText.GetPressedKeys();
		Keys[] pressedKeys2 = Main.oldInputText.GetPressedKeys();
		if (Main.inputText.IsKeyDown(Keys.Back) && Main.oldInputText.IsKeyDown(Keys.Back)) {
			__backSpaceRate.SetValue(null, (float)__backSpaceRate.GetValue(null) - 0.05f);
			if ((float)__backSpaceRate.GetValue(null) < 0f)
				__backSpaceRate.SetValue(null, 0f);

		if ((int)__backSpaceCount.GetValue(null) <= 0) {
				__backSpaceCount.SetValue(null, (int)Math.Round((float)__backSpaceRate.GetValue(null)));
				flag = true;
			}

			__backSpaceCount.SetValue(null, (int)__backSpaceCount.GetValue(null) - 1);
		}
		else {
			__backSpaceRate.SetValue(null, 7f);
			__backSpaceCount.SetValue(null, 15);
		}

		for (int j = 0; j < pressedKeys.Length; j++) {
			bool flag2 = true;
			for (int k = 0; k < pressedKeys2.Length; k++) {
				if (pressedKeys[j] == pressedKeys2[k])
					flag2 = false;
			}

			if ((pressedKeys[j].ToString() ?? "") == "Back" && (flag2 || flag) && text.Length > 0) { //&& cursorPosition != 0) {
				TextSnippet[] snippets = ChatManager.ParseMessage(text, Color.White).ToArray();
				text = (!snippets[^1].DeleteWhole) ? text[0..^1] : text[..^snippets[^1].TextOriginal.Length];
				//text = (!snippets[^1].DeleteWhole) ? text.Remove(cursorPosition - 1, 1) : text[..^snippets[^1].TextOriginal.Length];
				//cursorPosition--;
			}
		}

		return text;
	}

	private static string PasteTextIn(bool allowMultiLine, string newKeys)
	{
		newKeys = ((!allowMultiLine) ? (newKeys + Platform.Get<IClipboard>().Value) : (newKeys + Platform.Get<IClipboard>().MultiLineValue));
		return newKeys;
	}

	private static readonly Assembly tModAssembly = Assembly.GetAssembly(typeof(ModLoader));
	private static readonly MethodInfo __HandleCommand = typeof(CommandLoader).GetMethod("HandleCommand", BindingFlags.NonPublic | BindingFlags.Static);
	private static readonly Type __ChatCommandCaller = tModAssembly.GetType("Terraria.ModLoader.ChatCommandCaller");

	public static Color chatCursorColor = Color.White; // new!
	public static bool chatFocus = true; // new!
	private void Main_DoUpdate_HandleChat(On_Main.orig_DoUpdate_HandleChat orig)
	{
		// new!
		if (!chatFocus)
			return;
		// !new

		if (Main.CurrentInputTextTakerOverride != null) {
			Main.drawingPlayerChat = false;
			return;
		}

		if (Main.editSign) {
			Main.drawingPlayerChat = false;
		}

		if (PlayerInput.UsingGamepad) {
			Main.drawingPlayerChat = false;
		}

		if (!Main.drawingPlayerChat) {
			chatCursorColor = Color.White; // !new
			Main.chatMonitor.ResetOffset();
			return;
		}

		int linesOffset = 0;
		if (Main.keyState.IsKeyDown(Keys.Up)) {
			linesOffset = 1;
		}
		else if (Main.keyState.IsKeyDown(Keys.Down)) {
			linesOffset = -1;
		}
		//else if (Main.keyState.IsKeyDown(Keys.Left) && !Main.oldKeyState.IsKeyDown(Keys.Left)) {
		//	if (cursorPosition - 1 >= 0) {
		//		cursorPosition--;
		//	}
		//}
		//else if (Main.keyState.IsKeyDown(Keys.Right) && !Main.oldKeyState.IsKeyDown(Keys.Right)) {
		//	if (cursorPosition + 1 <= Main.chatText.Length) {
		//		cursorPosition++;
		//	}
		//}

		Main.chatMonitor.Offset(linesOffset);
		if (Main.keyState.IsKeyDown(Keys.Escape)) {
			Main.drawingPlayerChat = false;
		}

		string oldChatText = Main.chatText;
		string newChatText = Main.GetInputText(Main.chatText);
		Main.chatText = newChatText;
		int num = (int)(Main.screenWidth * (1f / Main.UIScale)) - 330;
		if (oldChatText != Main.chatText) {
			for (float x = ChatManager.GetStringSize(FontAssets.MouseText.Value, Main.chatText, Vector2.One).X; x > num; x = ChatManager.GetStringSize(FontAssets.MouseText.Value, Main.chatText, Vector2.One).X) {
				int num2 = Math.Max(0, (int)(x - num) / 100);
				Main.chatText = Main.chatText[..(Main.chatText.Length - 1 - num2)];
			}
		}

		if (oldChatText != Main.chatText) {
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		if (!Main.inputTextEnter || !Main.chatRelease) {
			return;
		}

		var handled = Main.chatText.Length > 0 && Main.chatText[0] == '/' && (bool)__HandleCommand.Invoke(null, new object[] { Main.chatText, Activator.CreateInstance(__ChatCommandCaller, true) });
		if (Main.chatText != "" && !handled) {
			ChatMessage message = ChatManager.Commands.CreateOutgoingMessage(Main.chatText);
			if (Main.netMode == NetmodeID.MultiplayerClient)
				ChatHelper.SendChatMessageFromClient(message);
			else if (Main.netMode == NetmodeID.SinglePlayer)
				ChatManager.Commands.ProcessIncomingMessage(message, Main.myPlayer);
		}

		Main.chatText = "";
		Main.ClosePlayerChat();
		Main.chatRelease = false;
		SoundEngine.PlaySound(SoundID.MenuClose);
	}

	private void Main_DrawPlayerChat(On_Main.orig_DrawPlayerChat orig, Main self)
	{
		TextSnippet[] array = null;
		if (Main.drawingPlayerChat)
			PlayerInput.WritingText = true;

		Main.instance.HandleIME();
		if (Main.drawingPlayerChat) {
			Main.instance.textBlinkerCount++;
			if (Main.instance.textBlinkerCount >= 20) {
				if (Main.instance.textBlinkerState == 0)
					Main.instance.textBlinkerState = 1;
				else
					Main.instance.textBlinkerState = 0;

				Main.instance.textBlinkerCount = 0;
			}

			string text = Main.chatText;
			if (Main.screenWidth > 800) {
				int num = Main.screenWidth - 300;
				int num2 = 78;
				Main.spriteBatch.Draw(TextureAssets.TextBack.Value, new Vector2(num2, Main.screenHeight - 36), new Rectangle(0, 0, TextureAssets.TextBack.Width() - 100, TextureAssets.TextBack.Height()), new Color(100, 100, 100, 100), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
				num -= 400;
				num2 += 400;
				while (num > 0) {
					if (num > 300) {
						Main.spriteBatch.Draw(TextureAssets.TextBack.Value, new Vector2(num2, Main.screenHeight - 36), new Rectangle(100, 0, TextureAssets.TextBack.Width() - 200, TextureAssets.TextBack.Height()), new Color(100, 100, 100, 100), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
						num -= 300;
						num2 += 300;
					}
					else {
						Main.spriteBatch.Draw(TextureAssets.TextBack.Value, new Vector2(num2, Main.screenHeight - 36), new Rectangle(TextureAssets.TextBack.Width() - num, 0, TextureAssets.TextBack.Width() - (TextureAssets.TextBack.Width() - num), TextureAssets.TextBack.Height()), new Color(100, 100, 100, 100), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
						num = 0;
					}
				}
			}
			else {
				Main.spriteBatch.Draw(TextureAssets.TextBack.Value, new Vector2(78f, Main.screenHeight - 36), new Rectangle(0, 0, TextureAssets.TextBack.Width(), TextureAssets.TextBack.Height()), new Color(100, 100, 100, 100), 0f, default, 1f, SpriteEffects.None, 0f);
			}

			List<TextSnippet> list = ChatManager.ParseMessage(text, Color.White);
			string compositionString = Platform.Get<IImeService>().CompositionString;
			if (compositionString != null && compositionString.Length > 0)
				list.Add(new TextSnippet(compositionString, new Color(255, 240, 20)));

			if (Main.instance.textBlinkerState == 1) {
				//if (list.Count == 0) {
				//	list.Add(new TextSnippet("❘", Color.White));
				//	list.Add(new TextSnippet("|", Color.White));
					list.Add(new TextSnippet("|", chatCursorColor));
				//}
				//else {
				//	int i;
				//	int length = 0;
				//	for (i = list.Count - 1; i > 0; i--) {
				//		if (cursorPosition <= list[i].Text.Length) {
				//			break;
				//		}
				//		length += list[i].Text.Length;
				//	}
				//	list[i].Text = list[i].Text.Insert(cursorPosition - length, "❘");
				//}
			}

			array = list.ToArray();

			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, array, new Vector2(88f, Main.screenHeight - 30), 0f, Vector2.Zero, Vector2.One, out int hoveredSnippet);
			if (hoveredSnippet > -1) {
				array[hoveredSnippet].OnHover();
				if (Main.mouseLeft && Main.mouseLeftRelease)
					array[hoveredSnippet].OnClick();
			}
		}

		Main.chatMonitor.DrawChat(Main.drawingPlayerChat);
		if (Main.drawingPlayerChat && array != null) {
			Vector2 stringSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, array, Vector2.Zero);
			Main.instance.DrawWindowsIMEPanel(new Vector2(88f, Main.screenHeight - 30) + new Vector2(stringSize.X + 10f, -6f));
		}

		TimeLogger.DetailedDrawTime(10);
	}
}

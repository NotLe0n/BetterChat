using BetterChat.UI.UIStates;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterChat.UI;

internal class UISystem : ModSystem
{
	public static UISystem instance;

	internal UserInterface UserInterface;

	public override void Load()
	{
		instance = this;

		if (!Main.dedServ) {
			UserInterface = new UserInterface();
			UserInterface.SetState(new BetterChatUI());
		}

		base.Load();
	}

	public override void Unload()
	{
		instance = null;

		UserInterface = null;

		base.Unload();
	}

	private GameTime _lastUpdateUiGameTime;
	public override void UpdateUI(GameTime gameTime)
	{
		_lastUpdateUiGameTime = gameTime;

		if (Main.drawingPlayerChat) {
			UserInterface.Update(gameTime);
		}
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		if (mouseTextIndex == -1) return;

		layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
			"BetterChat: UI",
			delegate
			{
				if (Main.drawingPlayerChat) {
					UserInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
				}

				return true;
			},
			InterfaceScaleType.UI)
		);
	}
}

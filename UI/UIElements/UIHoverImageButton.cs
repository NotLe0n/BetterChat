﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace BetterChat.UI.UIElements;

internal class UIHoverImageButton : UIImageButton
{
	public string HoverText { get; set; }

	public UIHoverImageButton(string texture, string hoverText) : base(ModContent.Request<Texture2D>(texture, ReLogic.Content.AssetRequestMode.ImmediateLoad))
	{
		HoverText = hoverText;
	}

	public override void Update(GameTime gameTime)
	{
		if (IsMouseHovering) {
			Main.LocalPlayer.cursorItemIconText = HoverText;
			Main.LocalPlayer.mouseInterface = true;
		}
		base.Update(gameTime);
	}
}

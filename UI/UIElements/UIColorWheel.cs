using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterChat.UI.UIElements;

internal class UIColorWheel : UIElement
{
	public Color SelectedColor { get; private set; }
	private Vector2 selectedPosition;

	private readonly int diameter;
	private readonly int blackEndDiameter;

	public UIColorWheel(int diameter)
	{
		this.diameter = diameter;
		blackEndDiameter = (int)(diameter / 1.125f);

		Width = new(diameter, 0);
		Height = new(diameter, 0);
		Recalculate();

		selectedPosition = GetDimensions().Center() - Vector2.One * 4;
		SelectedColor = Color.White;
	}

	public void Reset()
	{
		selectedPosition = new Vector2(diameter/2f);
		SelectedColor = Color.White;
	}

	private Color? GetColor(int x, int y)
	{
		int distsq = x * x + y * y;

		if (distsq > diameter * diameter) {
			return null;
		}

		float hue = MathF.Atan2(x, y) / MathF.Tau;
		float sat;
		float lum = 0.5f;
		if (distsq <= (blackEndDiameter * blackEndDiameter) / 2) {
			// color begins to get lighter
			sat = MathHelper.Clamp((distsq) / (float)(blackEndDiameter * blackEndDiameter), 0, 0.5f);
		}
		else if (distsq <= (blackEndDiameter * blackEndDiameter)) {
			// color begins to get darker
			sat = MathHelper.Clamp((distsq) / (float)(blackEndDiameter * blackEndDiameter), 0.5f, 1);
		}
		else {
			// from black to white
			// took me way too long to figure out: https://www.desmos.com/calculator/fy3rtplrum
			sat = 1 - (MathF.Sqrt(distsq) - blackEndDiameter) / (diameter - blackEndDiameter);
			lum = 0;
		}

		return Main.hslToRgb(hue, lum, 1 - sat);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!IsMouseHovering || !Main.mouseLeft) {
			return;
		}

		int radius = diameter / 2;
		float distCent = Vector2.Distance(Main.MouseScreen, GetDimensions().Center());
		if (distCent >= radius) {
			return; // return if mouse is outside of circle
		}

		var translatedMouse = Main.MouseScreen - GetDimensions().Position();

		selectedPosition = translatedMouse;

		const float magic = 0.94444f; // idk why 0.9444
		int circleMouseX = (int)((translatedMouse.X / magic - radius) * 2);
		int circleMouseY = (int)(translatedMouse.Y - radius) * 2;

		Color? color = GetColor(circleMouseX, circleMouseY);
		if (color.HasValue) {
			SelectedColor = color.Value;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Main.LocalPlayer.mouseInterface = true;

		base.Draw(spriteBatch);
		var rect = GetDimensions().ToRectangle();

		// draw color circle
		spriteBatch.Draw(CreateCircle(), rect, Color.White * (IsMouseHovering ? 1 : 0.4f));

		// draw selector
		var selectorTexture = ModContent.Request<Texture2D>("BetterChat/UI/UIAssets/selector", ReLogic.Content.AssetRequestMode.ImmediateLoad);
		spriteBatch.Draw(selectorTexture.Value, selectedPosition + rect.Location.ToVector2() - new Vector2(4), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
	}

	private Texture2D CreateCircle()
	{
		int outerRadius = diameter * 2 + 2; // So circle doesn't go out of bounds
		var texture = new Texture2D(Main.graphics.GraphicsDevice, outerRadius, outerRadius);

		var data = new Color[outerRadius * outerRadius];

		for (int y = -diameter; y <= diameter; y++) {
			for (int x = -diameter; x <= diameter; x++) {
				if (x * x + y * y <= diameter * diameter) {
					// centering top left corner of circle on (0, 0)
					int sx = x + diameter;
					int sy = y + diameter;
					int index = sy * outerRadius + sx + 1; // get array index

					// set pixel at the index to the color
					data[index] = GetColor(x, y) ?? Color.Transparent;
				}
			}
		}

		texture.SetData(data); // fill texture
		return texture;
	}
}

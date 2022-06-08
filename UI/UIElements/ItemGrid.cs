using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterChat.UI.UIElements;

internal class ItemGrid : UIElement
{
	private readonly UIBetterTextBox _textBox;
	private readonly UIScrollbar _scrollbar;

	private int[] items;
	public event Action<int> OnItemClicked;

	public ItemGrid()
	{
		OverflowHidden = true;

		_textBox = new UIBetterTextBox("search") {
			Width = new(0, 0.95f),
			Height = new(30, 0)
		};
		_textBox.OnTextChanged += TextBox_OnTextChanged;
		Append(_textBox);

		_scrollbar = new UIScrollbar() {
			Top = new(35, 0),
			Height = new(0, 0.85f),
			Width = new(20, 0),
			Left = new(-20, 1),
		};
		Append(_scrollbar);

		var temp = new List<int>();
		Item item = new();
		for (int i = 0; i < ItemLoader.ItemCount; i++) {
			item.SetDefaults(i);

			if (item is null || string.IsNullOrEmpty(item.Name))
				continue;

			temp.Add(item.type);
		}

		items = temp.OrderBy(x => new Item(x).Name).ToArray();

		_scrollbar.SetView(GetInnerDimensions().Height, (items.Length - itemOnPageCount) * 10);
	}

	private void TextBox_OnTextChanged()
	{
		if (string.IsNullOrWhiteSpace(_textBox.Text) || _textBox.Text == "") {
			items = items.OrderBy(x => new Item(x).Name).ToArray();
		}

		items = items.OrderByDescending(x => new Item(x).Name.ToLower().Contains(_textBox.Text.ToLower())).ToArray();
	}

	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		base.ScrollWheel(evt);
		_scrollbar.ViewPosition -= evt.ScrollWheelValue;
	}

	const int size = 20;
	const int padding = 8;
	const int wrapAmount = 12;
	const int itemOnPageCount = 144;

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		int row = 0;
		int collumn = 0;
		int startIndex = (int)_scrollbar.ViewPosition / 10 - (int)(_scrollbar.ViewPosition / 10) % wrapAmount;
		int endIndex = (int)_scrollbar.ViewPosition / 10 + itemOnPageCount - ((int)_scrollbar.ViewPosition / 10 + itemOnPageCount) % wrapAmount;
		for (int i = startIndex; i < endIndex; i++) {
			if (i % wrapAmount == 0) {
				row += size + padding;
				collumn = 0;
			}

			Vector2 drawPos = new(GetDimensions().X + collumn + 8, GetDimensions().Y + row + 15);
			GetItemDrawInfo(i, drawPos, size, out var itemTexture, out Rectangle itemFrameRect, out float drawScale, out Vector2 itemPos);

			var drawRect = new Rectangle((int)itemPos.X, (int)itemPos.Y, size, size);
			
			// mouse is over item
			if (drawRect.Contains(Main.MouseScreen.ToPoint())) {
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					OnItemClicked.Invoke(items[i]);
				}
			}

			// draw background
			spriteBatch.Draw(TextureAssets.InventoryBack.Value, new Rectangle((int)GetDimensions().X + collumn, (int)GetDimensions().Y + row + 5, size + 6, size + 6), Color.AliceBlue);

			// draw item
			spriteBatch.Draw(itemTexture.Value, itemPos, itemFrameRect, Color.White, 0, Vector2.Zero, drawScale, SpriteEffects.None, 0);

			// go to next item position
			collumn += size + padding;
		}
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (IsMouseHovering) {
			Main.LocalPlayer.mouseInterface = true;
		}

		ChatEdits.chatFocus = !_textBox.IsFocused;
	}

	private void GetItemDrawInfo(int i, Vector2 drawPos, int boxSize, out Asset<Texture2D> itemTexture, out Rectangle itemFrameRect, out float drawScale, out Vector2 itemPos)
	{
		// Load item texture (be careful to not call this method to often)
		Main.instance.LoadItem(items[i]);
		itemTexture = TextureAssets.Item[items[i]];

		// get correct animation frame
		itemFrameRect = itemTexture.Frame();
		int frameCount = 1;
		if (Main.itemAnimations[items[i]] != null) {
			itemFrameRect = Main.itemAnimations[items[i]].GetFrame(itemTexture.Value);
			frameCount = Main.itemAnimations[items[i]].FrameCount;
		}

		// scale item according to the given box size
		drawScale = 1f;
		if (itemTexture.Width() > boxSize || itemTexture.Height() / frameCount > boxSize) {
			drawScale = boxSize / (float)(itemFrameRect.Width <= itemFrameRect.Height ?
				itemFrameRect.Height :
				itemFrameRect.Width);
		}

		// position the item correctly
		itemPos = drawPos - itemFrameRect.Size() * drawScale / 4;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		ChatEdits.chatFocus = true;
	}
}

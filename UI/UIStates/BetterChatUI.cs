using BetterChat.UI.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterChat.UI.UIStates;

internal class BetterChatUI : UIState
{
	private UIPanel colorPanel;
	private UIPanel itemPanel;

	const int btnPos = -160;
	const int btnWidth = 34;

	public BetterChatUI()
	{
		var colorBtn = new UIHoverImageButton("BetterChat/UI/UIAssets/colorButton", "Colored Text") {
			Top = new(-37, 1),
			Left = new(btnPos, 1),
		};
		colorBtn.OnClick += ColorBtn_OnClick;
		Append(colorBtn);

		var itemBtn = new UIHoverImageButton("BetterChat/UI/UIAssets/itemButton", "Item Icons") {
			Top = new(-37, 1),
			Left = new(btnPos - btnWidth - 16, 1),
		};
		itemBtn.OnClick += ItemBtn_OnClick;
		Append(itemBtn);

		var clearBtn = new UIImage(TextureAssets.Trash) {
			Top = new(-37, 1),
			Left = new(btnPos - btnWidth * 2 - 16 * 2, 1),
		};
		clearBtn.ImageScale = 0.5f;
		clearBtn.OnClick += (_, _) => Main.chatText = "";
		Append(clearBtn);
	}

	private void ItemBtn_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		if (itemPanel is not null) {
			itemPanel.Remove();
			itemPanel = null;
			return;
		}

		colorPanel?.Remove();
		colorPanel = null;

		const int itemPanelWidth = 380;
		const int itemPanelHeight = 400;

		itemPanel = new UIPanel() {
			Top = new(-37 - itemPanelHeight - 13, 1),
			Left = new(btnPos - 16 - btnWidth / 2 - itemPanelWidth / 2, 1),
			Width = new(itemPanelWidth, 0),
			Height = new(itemPanelHeight, 0)
		};
		itemPanel.SetPadding(10);
		Append(itemPanel);

		var grid = new ItemGrid() {
			Width = new(0, 1),
			Height = new(0, 1)
		};
		grid.OnItemClicked += (item) => BeginChatTag(item);
		itemPanel.Append(grid);
	}

	private void ColorBtn_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		if (colorPanel is not null) {
			colorPanel.Remove();
			colorPanel = null;
			return;
		}

		itemPanel?.Remove();
		itemPanel = null;

		const int colorWheelRadius = 180;
		const int colorPanelWidth = colorWheelRadius + 10;
		const int colorPanelHeight = colorWheelRadius + 50;

		colorPanel = new UIPanel() {
			Top = new(-37 - colorPanelHeight - 13, 1),
			Left = new(btnPos + btnWidth / 2 - colorPanelWidth / 2, 1),
			Width = new(colorPanelWidth, 0),
			Height = new(colorPanelHeight, 0)
		};
		colorPanel.SetPadding(10);
		Append(colorPanel);

		var colorWheel = new UIColorWheel(colorWheelRadius);
		colorPanel.Append(colorWheel);

		var applyButton = new UITextPanel<string>("apply", 0.5f) {
			Left = new(0, 0),
			Top = new(-25, 1),
			Width = new(10, 0),
			Height = new(5, 0)
		};
		applyButton.SetPadding(9);
		applyButton.OnClick += (_,_) => BeginChatTag(colorWheel.SelectedColor);
		colorPanel.Append(applyButton);

		var endButton = new UITextPanel<string>("close tag", 0.5f) {
			Left = new(-50, 1),
			Top = new(-25, 1),
			Width = new(20, 0),
			Height = new(5, 0)
		};
		endButton.SetPadding(9);
		endButton.OnClick += (_, _) => EndChatTag();
		colorPanel.Append(endButton);
	}

	private void BeginChatTag(int item)
	{
		Color? oldColor = currentColor;
		if (oldColor is not null) {
			EndChatTag();
		}

		Main.chatText += $"[i:{item}]";

		if (oldColor is not null) {
			BeginChatTag(oldColor.Value);
		}
	}

	private Color? currentColor;
	private void BeginChatTag(Color color)
	{
		if (currentColor is not null) {
			EndChatTag();
		}
		currentColor = color;
		ChatEdits.chatCursorColor = color;
		Main.chatText += $"[c/{color.R:X}{color.G:X}{color.B:X}:";
	}

	private void EndChatTag()
	{
		currentColor = null;
		ChatEdits.chatCursorColor = Color.White;
		Main.chatText += "]";
	}
}

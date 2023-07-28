using BetterChat.UI.UIElements;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace BetterChat.UI.UIStates;

internal class BetterChatUI : UIState
{
	private UIPanel colorPanel;
	private UIPanel itemPanel;

	private const int BtnPos = -160;
	private const int BtnWidth = 34;

	public BetterChatUI()
	{
		var colorBtn = new UIHoverImageButton("BetterChat/UI/UIAssets/colorButton", "Colored Text") {
			Top = new(-37, 1),
			Left = new(BtnPos, 1),
		};
		colorBtn.OnLeftClick += ColorBtn_OnClick;
		Append(colorBtn);

		var itemBtn = new UIHoverImageButton("BetterChat/UI/UIAssets/itemButton", "Item Icons") {
			Top = new(-37, 1),
			Left = new(BtnPos - BtnWidth - 16, 1),
		};
		itemBtn.OnLeftClick += ItemBtn_OnClick;
		Append(itemBtn);

		var clearBtn = new UIImage(TextureAssets.Trash) {
			Top = new(-37, 1),
			Left = new(BtnPos - BtnWidth * 2 - 16 * 2, 1),
			ImageScale = 0.5f
		};
		clearBtn.OnLeftClick += (_, _) => Main.chatText = "";
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

		itemPanel = new UIPanel {
			Top = new(-37 - itemPanelHeight - 13, 1),
			Left = new(BtnPos - 16 - BtnWidth / 2 - itemPanelWidth / 2, 1),
			Width = new(itemPanelWidth, 0),
			Height = new(itemPanelHeight, 0)
		};
		itemPanel.SetPadding(10);
		Append(itemPanel);

		var grid = new ItemGrid() {
			Width = new(0, 1),
			Height = new(0, 1)
		};
		grid.OnItemClicked += BeginChatTag;
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

		colorPanel = new UIPanel {
			Top = new(-37 - colorPanelHeight - 13, 1),
			Left = new(BtnPos + BtnWidth / 2 - colorPanelWidth / 2, 1),
			Width = new(colorPanelWidth, 0),
			Height = new(colorPanelHeight, 0)
		};
		colorPanel.SetPadding(10);
		Append(colorPanel);

		var colorWheel = new UIColorWheel(colorWheelRadius);
		colorPanel.Append(colorWheel);

		float CalcTextScale(string t, int w) => (2*w) / FontAssets.MouseText.Value.MeasureString(t).X;

		string applyText = Language.GetTextValue("Mods.BetterChat.ApplyTag");
		var applyButton = new UITextPanel<string>(applyText, CalcTextScale(applyText, 19)) {
			Left = new(0, 0),
			Top = new(-25, 1),
			Width = new(10, 0),
			Height = new(5, 0)
		};
		applyButton.SetPadding(9);
		applyButton.OnLeftClick += (_,_) => BeginChatTag(colorWheel.SelectedColor);
		colorPanel.Append(applyButton);

		string resetText = Language.GetTextValue("Mods.BetterChat.Reset");
		var resetButton = new UITextPanel<string>(resetText, CalcTextScale(resetText, 19)) {
			Left = new(0, 0.35f),
			Top = new(-25, 1),
			Width = new(10, 0),
			Height = new(5, 0)
		};
		resetButton.SetPadding(9);
		resetButton.OnLeftClick += (_,_) => colorWheel.Reset();
		colorPanel.Append(resetButton);

		string closeText = Language.GetTextValue("Mods.BetterChat.CloseTag");
		var endButton = new UITextPanel<string>(closeText, CalcTextScale(closeText, 19)) {
			Left = new(0, 0.7f),
			Top = new(-25, 1),
			Width = new(10, 0),
			Height = new(5, 0)
		};
		endButton.SetPadding(9);
		endButton.OnLeftClick += (_, _) => EndChatTag();
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

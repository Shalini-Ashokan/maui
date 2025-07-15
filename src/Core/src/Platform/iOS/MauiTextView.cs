using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiTextView : UITextView, IUIViewLifeCycleEvents
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		readonly MauiLabel _placeholderLabel;
		nfloat? _defaultPlaceholderSize;

		public MauiTextView()
		{
			_placeholderLabel = InitPlaceholderLabel();
			UpdatePlaceholderLabelFrame();
			Changed += OnChanged;
		}

		public MauiTextView(CGRect frame)
			: base(frame)
		{
			_placeholderLabel = InitPlaceholderLabel();
			UpdatePlaceholderLabelFrame();
			Changed += OnChanged;
		}

		public override void WillMoveToWindow(UIWindow? window)
		{
			base.WillMoveToWindow(window);
		}

		// Native Changed doesn't fire when the Text Property is set in code
		// We use this event as a way to fire changes whenever the Text changes
		// via code or user interaction.
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		public event EventHandler? TextSetOrChanged;

		// Event fired when placeholder changes and may affect the required size
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		public event EventHandler? PlaceholderSizeChanged;

		public string? PlaceholderText
		{
			get => _placeholderLabel.Text;
			set
			{
				_placeholderLabel.Text = value;
				_placeholderLabel.SizeToFit();
				OnPlaceholderSizeChanged();
			}
		}

		public NSAttributedString? AttributedPlaceholderText
		{
			get => _placeholderLabel.AttributedText;
			set
			{
				_placeholderLabel.AttributedText = value;
				_placeholderLabel.SizeToFit();
				OnPlaceholderSizeChanged();
			}
		}

		public UIColor? PlaceholderTextColor
		{
			get => _placeholderLabel.TextColor;
			set => _placeholderLabel.TextColor = value;
		}

		public TextAlignment VerticalTextAlignment { get; set; }

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
				{
					HidePlaceholderIfTextIsPresent(value);
					TextSetOrChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public override UIFont? Font
		{
			get => base.Font;
			set
			{
				base.Font = value;
				UpdatePlaceholderFont(value);

			}
		}

		public override NSAttributedString AttributedText
		{
			get => base.AttributedText;
			set
			{
				var old = base.AttributedText;

				base.AttributedText = value;

				if (old?.Value != value?.Value)
				{
					HidePlaceholderIfTextIsPresent(value?.Value);
					TextSetOrChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			UpdatePlaceholderLabelFrame();
			ShouldCenterVertically();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var baseSize = base.SizeThatFits(size);

			// If text is empty and we have placeholder text, ensure we're at least as big as the placeholder
			if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(_placeholderLabel?.Text))
			{
				var placeholderSize = GetPlaceholderRequiredSize(size.Width);
				baseSize = new CGSize(
					Math.Max(baseSize.Width, placeholderSize.Width),
					Math.Max(baseSize.Height, placeholderSize.Height)
				);
			}

			return baseSize;
		}

		MauiLabel InitPlaceholderLabel()
		{
			var placeholderLabel = new MauiLabel
			{
				BackgroundColor = UIColor.Clear,
				TextColor = ColorExtensions.PlaceholderColor,
				Lines = 0,
				VerticalAlignment = UIControlContentVerticalAlignment.Top
			};

			AddSubview(placeholderLabel);

			return placeholderLabel;
		}

		void UpdatePlaceholderLabelFrame()
		{
			if (Bounds != CGRect.Empty && _placeholderLabel is not null)
			{
				var x = TextContainer.LineFragmentPadding;
				var y = TextContainerInset.Top;
				var width = Bounds.Width - (x * 2);
				var height = Frame.Height - (TextContainerInset.Top + TextContainerInset.Bottom);
				_placeholderLabel.SizeThatFits(new CGSize(width, height));
				_placeholderLabel.Frame = new CGRect(x, y, width, height);
			}
		}

		void HidePlaceholderIfTextIsPresent(string? value)
		{
			_placeholderLabel.Hidden = !string.IsNullOrEmpty(value);
		}

		void OnChanged(object? sender, EventArgs e)
		{
			HidePlaceholderIfTextIsPresent(Text);
			TextSetOrChanged?.Invoke(this, EventArgs.Empty);
		}

		void ShouldCenterVertically()
		{
			var contentHeight = ContentSize.Height;
			var availableSpace = Bounds.Height - contentHeight * ZoomScale;
			if (availableSpace <= 0)
				return;
			ContentOffset = VerticalTextAlignment switch
			{
				Maui.TextAlignment.Center => new CGPoint(0, -Math.Max(1, availableSpace / 2)),
				Maui.TextAlignment.End => new CGPoint(0, -Math.Max(1, availableSpace)),
				_ => ContentOffset,
			};

			// Scroll the content to the cursor position if it is hidden by the keyboard
			if (KeyboardAutoManagerScroll.IsKeyboardShowing && (VerticalTextAlignment == Maui.TextAlignment.Center || VerticalTextAlignment == Maui.TextAlignment.End))
			{
				var cursorRect = KeyboardAutoManagerScroll.FindCursorPosition();
				var keyboardTop = KeyboardAutoManagerScroll.KeyboardFrame.Top;

				if (cursorRect.HasValue && cursorRect.Value.Bottom > keyboardTop)
				{
					var offset = cursorRect.Value.Bottom - KeyboardAutoManagerScroll.KeyboardFrame.Top;
					ContentOffset = new CGPoint(ContentOffset.X, ContentOffset.Y + offset);
				}
			}
		}

		void UpdatePlaceholderFont(UIFont? value)
		{
			_defaultPlaceholderSize ??= _placeholderLabel.Font.PointSize;
			_placeholderLabel.Font = value ?? _placeholderLabel.Font.WithSize(
				value?.PointSize ?? _defaultPlaceholderSize.Value);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}

		void OnPlaceholderSizeChanged()
		{
			PlaceholderSizeChanged?.Invoke(this, EventArgs.Empty);
		}

#pragma warning disable RS0016 // Add public types and members to the declared API
		public CGSize GetPlaceholderRequiredSize(double maxWidth = double.PositiveInfinity)
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			if (string.IsNullOrEmpty(_placeholderLabel?.Text))
				return CGSize.Empty;

			// Calculate available width (same logic as UpdatePlaceholderLabelFrame)
			var availableWidth = double.IsInfinity(maxWidth)
				? 300 // fallback width
				: maxWidth - (TextContainer.LineFragmentPadding * 2);

			// Use a temporary label with the same configuration to calculate size
			using var tempLabel = new UILabel
			{
				Text = _placeholderLabel.Text,
				Font = _placeholderLabel.Font,
				Lines = 0, // Allow unlimited lines for wrapping
				LineBreakMode = UILineBreakMode.WordWrap,
				PreferredMaxLayoutWidth = (nfloat)availableWidth
			};

			var placeholderSize = tempLabel.SizeThatFits(new CGSize(availableWidth, nfloat.MaxValue));

			// Add text container insets to get the total required UITextView size
			var totalHeight = placeholderSize.Height + TextContainerInset.Top + TextContainerInset.Bottom;
			var totalWidth = Math.Min(placeholderSize.Width + (TextContainer.LineFragmentPadding * 2), availableWidth);

			return new CGSize(totalWidth, totalHeight);
		}
	}
}

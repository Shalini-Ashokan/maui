#nullable disable
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		static void MapSemantics(IViewHandler handler, VisualElement element)
		{
			if (handler.PlatformView is not UIView platformView ||
				platformView is UIControl ||
				AutomationProperties.GetIsInAccessibleTree(element) == true ||
				element is not IVisualTreeElement visualTreeElement ||
				!HasExplicitlyAccessibleDescendant(visualTreeElement))
			{
				return;
			}

			platformView.IsAccessibilityElement = false;
		}

		static bool HasExplicitlyAccessibleDescendant(IVisualTreeElement element)
		{
			foreach (var child in element.GetVisualChildren())
			{
				if (child is BindableObject bindable &&
					AutomationProperties.GetIsInAccessibleTree(bindable) == true)
				{
					return true;
				}

				if (HasExplicitlyAccessibleDescendant(child))
					return true;
			}

			return false;
		}
	}
}
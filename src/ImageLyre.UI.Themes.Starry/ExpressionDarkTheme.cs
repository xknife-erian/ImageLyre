/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using AvalonDock.Themes;

namespace TVS.Ginkgo.Themes.Starry
{
	/// <inheritdoc/>
	public class ExpressionDarkTheme : Theme
	{
		/// <inheritdoc/>
		public override Uri GetResourceUri()
		{
			return new Uri(
				"/TVS.Ginkgo.Themes.Starry;component/DarkTheme.xaml",
				UriKind.Relative);
		}
	}
}

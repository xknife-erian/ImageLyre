﻿/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TVS.Ginkgo.Themes.Starry.Controls
{
	public class SplineBorder : Control
	{

		public SplineBorder()
		{
		}


		#region Thickness

		/// <summary>
		/// Thickness Dependency Property
		/// </summary>
		public static readonly DependencyProperty ThicknessProperty =
			DependencyProperty.Register("Thickness", typeof(double), typeof(SplineBorder),
				new FrameworkPropertyMetadata((double)1.0, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gets or sets the Thickness property.  This dependency property 
		/// indicates the border thickness.
		/// </summary>
		public double Thickness
		{
			get => (double)GetValue(ThicknessProperty);
			set => SetValue(ThicknessProperty, value);
		}

		#endregion

		#region Fill

		/// <summary>
		/// Fill Dependency Property
		/// </summary>
		public static readonly DependencyProperty FillProperty =
			DependencyProperty.Register("Fill", typeof(Brush), typeof(SplineBorder),
				new FrameworkPropertyMetadata((Brush)null, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gets or sets the Fill property.  This dependency property 
		/// indicates the fill color.
		/// </summary>
		public Brush Fill
		{
			get => (Brush)GetValue(FillProperty);
			set => SetValue(FillProperty, value);
		}

		#endregion

		#region Stroke

		/// <summary>
		/// Stroke Dependency Property
		/// </summary>
		public static readonly DependencyProperty StrokeProperty =
			DependencyProperty.Register("Stroke", typeof(Brush), typeof(SplineBorder),
				new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gets or sets the Stroke property.  This dependency property 
		/// indicates the stroke brush.
		/// </summary>
		public Brush Stroke
		{
			get => (Brush)GetValue(StrokeProperty);
			set => SetValue(StrokeProperty, value);
		}

		#endregion

		#region BottomBorderMargin

		/// <summary>
		/// BottomBorderMargin Dependency Property
		/// </summary>
		public static readonly DependencyProperty BottomBorderMarginProperty =
			DependencyProperty.Register("BottomBorderMargin", typeof(double), typeof(SplineBorder),
				new FrameworkPropertyMetadata((double)0.0, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gets or sets the BottomBorderMargin property.  This dependency property 
		/// indicates the adjustment for the bottom margin.
		/// </summary>
		public double BottomBorderMargin
		{
			get => (double)GetValue(BottomBorderMarginProperty);
			set => SetValue(BottomBorderMarginProperty, value);
		}

		#endregion

		protected override void OnRender(DrawingContext drawingContext)
		{

			var pgFill = new PathGeometry();
			var pfFill = new PathFigure() { IsFilled = true, IsClosed = true };
			pfFill.StartPoint = new Point(0.0, 0.0);

			var q1Fill = new QuadraticBezierSegment() { Point1 = new Point(RenderSize.Width / 3, RenderSize.Height / 10.0), Point2 = new Point(RenderSize.Width, RenderSize.Height), IsStroked = false };
			pfFill.Segments.Add(q1Fill);

			pfFill.Segments.Add(new LineSegment() { Point = new Point(0, RenderSize.Height), IsStroked = false });

			pgFill.Figures.Add(pfFill);

			drawingContext.DrawGeometry(Fill, null, pgFill);

			var pgBorder = new PathGeometry();
			var pfBorder = new PathFigure() { IsFilled = false, IsClosed = false };
			pfBorder.StartPoint = new Point(0.0, Thickness / 2);

			var q1Border = new QuadraticBezierSegment() { Point1 = new Point(RenderSize.Width / 3, RenderSize.Height / 10.0), Point2 = new Point(RenderSize.Width, RenderSize.Height) };
			pfBorder.Segments.Add(q1Border);

			pfFill.Segments.Add(new LineSegment() { Point = new Point(0, RenderSize.Height), IsStroked = false });

			pgBorder.Figures.Add(pfBorder);

			drawingContext.DrawGeometry(null, new Pen(Stroke, Thickness), pgBorder);

			base.OnRender(drawingContext);
		}
	}
}

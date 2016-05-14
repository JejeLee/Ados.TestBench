﻿using System.Windows;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace Microsoft.Research.DynamicDataDisplay
{
	public class MarkerPointsGraph : PointsGraphBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
		/// </summary>
		public MarkerPointsGraph()
		{
			ManualTranslate = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
		/// </summary>
		/// <param name="dataSource">The data source.</param>
		public MarkerPointsGraph(IPointDataSource dataSource)
			: this()
		{
			DataSource = dataSource;
		}

		protected override void OnVisibleChanged(Rect newRect, Rect oldRect)
		{
			base.OnVisibleChanged(newRect, oldRect);
			InvalidateVisual();
		}

		public PointMarker Marker
		{
			get { return (PointMarker)GetValue(MarkerProperty); }
			set { SetValue(MarkerProperty, value); }
		}

		public static readonly DependencyProperty MarkerProperty =
			DependencyProperty.Register(
			  "Marker",
			  typeof(PointMarker),
			  typeof(MarkerPointsGraph),
			  new FrameworkPropertyMetadata { DefaultValue = null, AffectsRender = true }
				  );

		protected override void OnRenderCore(DrawingContext dc, RenderState state)
		{
			if (DataSource == null) return;
			if (Marker == null) return;

			var transform = Plotter2D.Viewport.Transform;

			Rect bounds = Rect.Empty;
			using (IPointEnumerator enumerator = DataSource.GetEnumerator(GetContext()))
			{
				Point point = new Point();
				while (enumerator.MoveNext())
				{
					enumerator.GetCurrent(ref point);
					enumerator.ApplyMappings(Marker);

					//Point screenPoint = point.Transform(state.Visible, state.Output);
					Point screenPoint = point.DataToScreen(transform);

					bounds = Rect.Union(bounds, point);
					Marker.Render(dc, screenPoint);
				}
			}

			ContentBounds = bounds;
		}
	}

    public class FilteredMarkerPointsGraph : MarkerPointsGraph
    {
        public FilteredMarkerPointsGraph()
            : base()
        {
            ;
        }

        public FilteredMarkerPointsGraph(IPointDataSource dataSource)
            : base(dataSource)
        {
            ;
        }

        protected override void OnRenderCore(DrawingContext dc, RenderState state)
        {
            // base.OnRenderCore
            if (DataSource == null) return;
            if (Marker == null) return;

            var left = Viewport.Visible.Location.X;
            var right = Viewport.Visible.Location.X + Viewport.Visible.Size.Width;
            var top = Viewport.Visible.Location.Y;
            var bottom = Viewport.Visible.Location.Y + Viewport.Visible.Size.Height;

            var transform = Plotter2D.Viewport.Transform;

            Rect bounds = Rect.Empty;
            using (IPointEnumerator enumerator = DataSource.GetEnumerator(GetContext()))
            {
                Point point = new Point();
                while (enumerator.MoveNext())
                {
                    enumerator.GetCurrent(ref point);

                    if (point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom)
                    {
                        enumerator.ApplyMappings(Marker);

                        Point screenPoint = point.DataToScreen(transform);

                        bounds = Rect.Union(bounds, point);
                        Marker.Render(dc, screenPoint);
                    }
                }
            }

            ContentBounds = bounds;
        }
    }
}

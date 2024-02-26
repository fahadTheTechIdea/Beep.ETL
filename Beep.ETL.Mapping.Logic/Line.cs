using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.ETL.Mapping.Logic
{
    public class Line
    {
        public SKPoint Start { get; set; }
        public SKPoint End { get; set; }

        public Line(SKPoint start, SKPoint end)
        {
            Start = start;
            End = end;
        }

        public bool ContainsPoint(SKPoint point)
        {
            // Define a tolerance range for checking if a point is on or near the line
            float tolerance = 5.0f;

            // Calculate the distances from the point to the start and end points of the line
            float distanceToStart = (float)Math.Sqrt(Math.Pow(point.X - Start.X, 2) + Math.Pow(point.Y - Start.Y, 2));
            float distanceToEnd = (float)Math.Sqrt(Math.Pow(point.X - End.X, 2) + Math.Pow(point.Y - End.Y, 2));

            // Calculate the length of the line
            float lineLength = (float)Math.Sqrt(Math.Pow(End.X - Start.X, 2) + Math.Pow(End.Y - Start.Y, 2));

            // If the sum of the distances to the start and end points is equal to the length of the line (within the tolerance range),
            // then the point is on the line
            if (Math.Abs(distanceToStart + distanceToEnd - lineLength) <= tolerance)
            {
                return true;
            }

            return false;
        }
    }

}

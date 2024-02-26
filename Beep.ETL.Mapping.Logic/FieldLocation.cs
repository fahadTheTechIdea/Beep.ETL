using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TheTechIdea.Beep.DataBase;

namespace Beep.ETL.Mapping.Logic
{
    public class FieldLocation
    {
        public IEntityField Field { get; set; }
        public SKRect Location { get; set; }
    }
}

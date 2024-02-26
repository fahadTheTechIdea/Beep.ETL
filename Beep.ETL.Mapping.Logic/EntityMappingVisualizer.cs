using Beep.Skia;
using SkiaSharp;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Workflow;

namespace Beep.ETL.Mapping.Logic
{
    public abstract class EntityMappingVisualizer
    {
        public IEntityField _draggedField { get; set; }
        public SKPoint? _dragEndPoint { get; set; }
        private IEntityField _hoveredField;
        protected IEntityStructure _draggedEntity;
        protected Dictionary<IEntityStructure, SKRect> _entityHeaderRects = new Dictionary<IEntityStructure, SKRect>();
        protected Dictionary<IEntityField, SKRect> _sourcefieldRects = new Dictionary<IEntityField, SKRect>();
        protected Dictionary<IEntityField, SKRect> _targetfieldRects = new Dictionary<IEntityField, SKRect>();
        public IEntityStructure _sourceEntity { get; set; } = new EntityStructure();
        public IEntityStructure _targetEntity { get; set; } = new EntityStructure();
        public  List<IMapping_rep_fields> _fieldMappings { get; set; }=new List<IMapping_rep_fields>();
        protected Dictionary<string, bool> SourceMappedFieldsStatus { get; set; } = new Dictionary<string, bool>();
        protected Dictionary<string, bool> TargetMappedFieldsStatus { get; set; } = new Dictionary<string, bool>();
        protected abstract void OnFieldSelected(IEntityField field, IEntityStructure entity, bool isRightClick);


        protected abstract void OnLineSelected(Line line, IMapping_rep_fields mapping, bool isRightClick);


        protected abstract void OnHeaderSelected(IEntityField field, IEntityStructure entity, bool isRightClick);


        protected abstract void OnHeaderDropped(IEntityField field, IEntityStructure entity);
       
        private SKPoint _startPoint;
        private SKPoint _endPoint;
        private SKCanvas _canvas;
        private IEntityField _selectedField;
        private const int padding = 10;

        private  int rowHeight = 20;
        private int columnWidth = 100;
        protected SKRect _sourceEntityRect;
        protected SKRect _targetEntityRect;
        protected SKRect _sourceheaderRect;
        protected SKRect _targetheaderRect;
        protected int _sourcewidth;
        protected int _targetwidth;
        protected int _sourceheight;
        protected int _targetheight;
        protected SKPoint sourcePosition;
        protected SKPoint targetPosition;
        // The header height
        private float headerHeight;
        // Add a dictionary to store the mapped fields status
        protected string lastsource = string.Empty;
        protected string lasttarget = string.Empty;

        protected IEntityField sourceSelectedField=null;
        protected IEntityField targetSelectedField=null;
        protected bool IsLeftMouseButtonPresses=false;
        protected bool IsRightMouseButtonPresses=false;
        protected SKPoint mouseDownPoint;
        
        protected bool IsDraggingOn=false;
        protected float dragThreshold = 5; // Adjust this to control how far the mouse needs to move to be considered a drag

        protected bool IsDraw = false;
        protected SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black,  // choose a contrasting color to your background
            TextSize = 12,
            IsAntialias = true

        };
        protected SKPaint HeadertextPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black,  // choose a contrasting color to your background
            TextSize = 12,
            IsAntialias = true

        };
        protected SKPaint EntitytextPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black,  // choose a contrasting color to your background
            TextSize = 12,
            IsAntialias = true

        };
        float canvasWidth;
        float canvasHeight;
        protected List<(Line line, IMapping_rep_fields mapping)> MappingLines { get; } = new List<(Line, IMapping_rep_fields)>();

        public EntityMappingVisualizer(IEntityStructure sourceEntity, IEntityStructure targetEntity, List<IMapping_rep_fields> fieldMappings)
        {
            _sourceEntity = sourceEntity;
            _targetEntity = targetEntity;
            _fieldMappings = fieldMappings;

            // Initialize the source and target mapped fields status dictionaries

            init();
            // Check each field in the source and target entities to see if it has been mapped
            foreach (var field in sourceEntity.Fields)
            {
                SourceMappedFieldsStatus[field.fieldname] = fieldMappings.Any(m => m.FromFieldName == field.fieldname);
            }

            foreach (var field in targetEntity.Fields)
            {
                TargetMappedFieldsStatus[field.fieldname] = fieldMappings.Any(m => m.ToFieldName == field.fieldname);
            }
        }

        public EntityMappingVisualizer()
        {

            // Initialize the source and target mapped fields status dictionaries

            init();



        }
        private void init()
        {
           
       

            _sourcefieldRects = new Dictionary<IEntityField, SKRect>();
            _targetfieldRects = new Dictionary<IEntityField, SKRect>();
            _fieldMappings = new List<IMapping_rep_fields>();
          
        }
        public void Draw(SKCanvas canvas)
        {
            _canvas = canvas;
            _canvas.Clear(SKColors.White);
             canvasWidth = _canvas.LocalClipBounds.Width;
             canvasHeight = _canvas.LocalClipBounds.Height;


            if (_sourceEntity != null)
            {
                if(!string.IsNullOrEmpty(_sourceEntity.EntityName ))
                {
                    if ((lastsource.Equals(_sourceEntity.EntityName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        init();
                    }
                }
            }
            else
            {
                
                        init();
   
            }
            if (_targetEntity != null)
            {
                if (!string.IsNullOrEmpty(_targetEntity.EntityName))
                {
                    if ((lasttarget.Equals(_targetEntity.EntityName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        init();
                    }
                }
            }
            else
            {
                init();
            }
            sourcePosition = new SKPoint(padding, padding);
            _sourceheight  = CalculateEntityHeight(_sourceEntity, textPaint);
            _sourcewidth = CalculateEntityWidth(_sourceEntity, textPaint);
            columnWidth = (_sourcewidth - 2 * padding);
            _sourceEntityRect = new SKRect(sourcePosition.X, sourcePosition.Y, sourcePosition.X + _sourcewidth, sourcePosition.Y + _sourceheight);
            DrawEntity(_sourceEntity, sourcePosition, "Source",true);
            // DrawEntityBorder(sourcePosition, _sourceEntity,true);

             
          //  var targetPosition = new SKPoint(3 * padding + 3 * _sourcewidth, padding);
           _targetheight = CalculateEntityHeight(_targetEntity, textPaint);
            _targetwidth = CalculateEntityWidth(_targetEntity, textPaint);
            columnWidth = (_targetwidth - 2 * padding);
            targetPosition = new SKPoint(canvasWidth - _targetwidth - padding - 50, padding);
           

            _targetEntityRect = new SKRect(targetPosition.X, targetPosition.Y, targetPosition.X + _targetwidth, targetPosition.Y + _targetheight);
            DrawEntity(_targetEntity, targetPosition, "Target",false);
            //  DrawEntityBorder(targetPosition, _targetEntity,false);

         

            foreach (var mapping in _fieldMappings)
            {
                DrawMappingLine(mapping);
            }
        }
        private void DrawEntity(IEntityStructure entity, SKPoint position, string headerText, bool isSourceEntity)
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.LightGray,
                IsAntialias=true
                
            };

          
            // Draw table header
            DrawTableHeader(headerText, position, isSourceEntity);

            // Update Y coordinate for the entity header
            SKPoint entityHeaderPosition = new SKPoint(position.X, position.Y + headerHeight);

            // Draw entity header
            var headerRect = new SKRect(entityHeaderPosition.X, entityHeaderPosition.Y, entityHeaderPosition.X + 2 * columnWidth, entityHeaderPosition.Y + headerHeight);
            _canvas.DrawRect(headerRect, paint);

            // Update totalHeight with entity header height
            float totalHeight = entityHeaderPosition.Y + headerHeight;
            // In DrawEntity or DrawTableHeader
            if (!_entityHeaderRects.ContainsKey(entity))
            {
                _entityHeaderRects.Add(entity, headerRect);
            }
            else
            {
                _entityHeaderRects[entity] = headerRect;
            }
            if (entity != null)
            {
                if (entity.EntityName != null)
                {
                    _canvas.DrawText(entity.EntityName, entityHeaderPosition.X + padding, entityHeaderPosition.Y + padding + headerHeight / 2, textPaint);

                    // Draw fields
                    var y = entityHeaderPosition.Y + headerHeight;
                    foreach (var field in entity.Fields)
                    {
                        DrawField(field, new SKPoint(entityHeaderPosition.X, y),isSourceEntity);
                        y += rowHeight;
                        totalHeight += rowHeight;
                    }
                }
            }

            // Draw table border
            // DrawTableBorder(position, totalHeight);
        }
        private void DrawField(IEntityField field, SKPoint position, bool isSourceField)
        {
            var isHovered = _hoveredField == field;
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = isHovered ? SKColors.Yellow : SKColors.White  // Change background color based on hover
            };

          
            // Calculate the text height and vertical position
            float textHeight = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
            float textVerticalPos = position.Y + (rowHeight - textHeight) / 2 - textPaint.FontMetrics.Ascent;

            // Draw field name
            var nameRect = new SKRect(position.X, position.Y, position.X + columnWidth, position.Y + rowHeight);
            _canvas.DrawRect(nameRect, paint);
            _canvas.DrawText(field.fieldname, position.X + padding, textVerticalPos, textPaint);

            // Draw field type
            var typeRect = new SKRect(position.X + columnWidth, position.Y, position.X + 2 * columnWidth, position.Y + rowHeight);
            _canvas.DrawRect(typeRect, paint);
            _canvas.DrawText(field.fieldtype, position.X + columnWidth + padding, textVerticalPos, textPaint);
            // Draw border around the row
            var borderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke, // Change the style to Stroke to draw borders
                Color = SKColors.Black,
                StrokeWidth = 1
            };
            var rowRect = new SKRect(position.X, position.Y, position.X + 2 * columnWidth, position.Y + rowHeight);
            _canvas.DrawRect(rowRect, borderPaint);

            // Use the correct dictionary based on the entity the field belongs to
            //var mappingStatusDictionary = isSourceField ? SourceMappedFieldsStatus : TargetMappedFieldsStatus;

            //if (mappingStatusDictionary.Count > 0)
            //{
            //    if (mappingStatusDictionary.ContainsKey(field.fieldname))
            //    {
            //        if (mappingStatusDictionary[field.fieldname] )
            //        {
            //            var mappedIndicatorPosition = isSourceField
            //                ? new SKPoint(position.X + 2 * columnWidth + padding, position.Y + rowHeight / 2)
            //                : new SKPoint(position.X - padding, position.Y + rowHeight / 2);
            //            DrawMappedIndicator(_canvas, mappedIndicatorPosition);
            //        }
            //    }
              
            //}
            if (isSourceField)
            {
                if (!_sourcefieldRects.ContainsKey(field))
                {
                    _sourcefieldRects.Add(field, rowRect);
                }
                else
                {
                    _sourcefieldRects[field] = rowRect;
                }

            }
            else
            {
                if (!_targetfieldRects.ContainsKey(field))
                {
                    _targetfieldRects.Add(field, rowRect);
                }
                else
                {
                    _targetfieldRects[field] = rowRect;
                }
            }
            // In DrawField
            
            // If the field is mapped, draw a visual indication

            // If this field is being dragged, draw some indicator of that
            if (_draggedField == field)
            {
                using (var indicatorpaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Red, StrokeWidth = 2 })
                {
                    var fieldRect = GetFieldRect(field, ParentOf(field));
                    _canvas.DrawRect(fieldRect, indicatorpaint);  // Draw a red outline around the field
                }
            }
            IEntityField selectedfield=   isSourceField ? sourceSelectedField : targetSelectedField;
            if(selectedfield != null)
            {
                if (selectedfield == field)
                {
                    using (var indicatorpaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Blue, StrokeWidth = 2 })
                    {
                        var fieldRect = GetFieldRect(selectedfield, ParentOf(selectedfield));
                        _canvas.DrawRect(fieldRect, indicatorpaint);  // Draw a red outline around the field
                    }
                }
               
            }
        }
        private void DrawMappedIndicator(SKCanvas canvas, SKPoint position)
        {
            // This method will draw an indicator (e.g., a small circle) at the given position
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Green,
                StrokeWidth = 2
            };

            canvas.DrawCircle(position, 5, paint);
        }
        private void DrawMappingLine(IMapping_rep_fields mapping)
        {
            var sourceField = _sourceEntity.Fields.FirstOrDefault(f => f.fieldname == mapping.FromFieldName);
            var targetField = _targetEntity.Fields.FirstOrDefault(f => f.fieldname == mapping.ToFieldName);

            if (sourceField == null || targetField == null)
            {
                return; // One or both fields are not found.
            }

            // Calculate the start and end points for the line
            var sourcePoint = GetFieldPosition(sourceField, _sourceEntity);
            var targetPoint = GetFieldPosition(targetField, _targetEntity);

            // Draw the line
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2
            })
            {
                _canvas.DrawLine(sourcePoint, targetPoint, paint);
            }
        }
        private SKPoint GetFieldPosition(IEntityField field, IEntityStructure entity)
        {
            var index = entity.Fields.IndexOf((EntityField)field);

            // Get rectangle of the field based on whether it's a source or target field
            var fieldRect = entity == _sourceEntity ? _sourcefieldRects[field] : _targetfieldRects[field];

            // Adjust x to be at the indicator position (right edge of the rectangle for source fields, left edge for target fields)
            float x = entity == _sourceEntity ? fieldRect.Right : fieldRect.Left;
            float y = padding + 2 * headerHeight + index * rowHeight + rowHeight / 2; // add rowHeight / 2 to get the vertical center of the field row

            return new SKPoint(x, y);
        }
        public (IEntityField, IEntityStructure) FindFieldAtPoint(SKPoint point)
        {
            foreach (var entity in new[] { _sourceEntity, _targetEntity })
            {
                // If the point is within the bounds of the entity
                var entityRect = entity == _sourceEntity ? _sourceEntityRect : _targetEntityRect;
                if (entityRect.Contains(point))
                {
                    // If the point is within the bounds of the entity header
                    var headerRect = entity == _sourceEntity ? _sourceheaderRect : _targetheaderRect;
                    if (headerRect.Contains(point))
                    {
                        return (null, entity);
                    }

                    // If the point is within the bounds of a field
                    foreach (var field in entity.Fields)
                    {
                        var fieldRect = GetFieldRect(field, entity);
                        if (fieldRect.Contains(point))
                        {
                            return (field, entity);
                        }
                    }
                }
            }

            return (null, null);
        }
        private (Line line, IMapping_rep_fields mapping) FindLineAtPoint(SKPoint point)
        {
            foreach (var line in MappingLines)
            {
                if (line.line.ContainsPoint(point))
                {
                    return line;
                }
            }
            return (null,null);
        }
        public int GetSourceFieldindex(IEntityField field)
        {
            return _sourceEntity.Fields.FindIndex(p=>p.Equals(field));
        }
        public int GetSourceFieldindex(string fieldname)
        {
            return _sourceEntity.Fields.FindIndex(p => p.fieldname.Equals(fieldname,StringComparison.InvariantCultureIgnoreCase));
        }
        public int GetTargetFieldindex(IEntityField field)
        {
            return _targetEntity.Fields.FindIndex(p => p.Equals(field));
        }
        public int GetTargetFieldindex(string fieldname)
        {
            return _targetEntity.Fields.FindIndex(p => p.fieldname.Equals(fieldname, StringComparison.InvariantCultureIgnoreCase));
        }
        public SKRect GetFieldRect(IEntityField field, IEntityStructure entity)
        {
            if (entity == _sourceEntity && _sourcefieldRects.ContainsKey(field))
            {
                return _sourcefieldRects[field];
            }
            else if (entity == _targetEntity && _targetfieldRects.ContainsKey(field))
            {
                return _targetfieldRects[field];
            }
            else
            {
                return new SKRect(); // return an empty rectangle if the field is not found
            }
        }
        public IEntityStructure ParentOf(IEntityField field)
        {
            if (_sourceEntity.Fields.Contains(field))
                return _sourceEntity;
            else if (_targetEntity.Fields.Contains(field))
                return _targetEntity;
            else
                return null;  // Or throw an exception if this situation should not occur
        }
        public void BatchMapFields()
        {
            // Go through each field in the source entity
            foreach (var sourceField in _sourceEntity.Fields)
            {
                // Try to find a field with the same name in the target entity
                var targetField = _targetEntity.Fields.FirstOrDefault(f => f.fieldname.Equals(sourceField.fieldname, StringComparison.InvariantCultureIgnoreCase));

                // If a matching field is found, create a mapping
                if (targetField != null)
                {
                    var newMapping = new Mapping_rep_fields()
                    {
                        FromEntityName = sourceField.EntityName,
                        FromFieldIndex = sourceField.FieldIndex,
                        FromFieldName = sourceField.fieldname,
                        FromFieldType = sourceField.fieldtype,
                        ToEntityName = targetField.EntityName,
                        ToFieldIndex = targetField.FieldIndex,
                        ToFieldName = targetField.fieldname,
                        ToFieldType = targetField.fieldtype
                    };

                    // Add the new mapping to the list
                    _fieldMappings.Add(newMapping);

                    // Mark the fields as mapped
                    SourceMappedFieldsStatus[sourceField.fieldname] = true;
                    TargetMappedFieldsStatus[targetField.fieldname] = true;
                }
            }

            // After batch mapping, redraw the visualizer to reflect the new mappings
            Redraw();
        }

        // In EntityMappingVisualizer
        public virtual void Redraw()
        {
            IsDraw=false;
            // Default implementation does nothing
        }
        public bool ValidateMapping(IEntityField sourceField, IEntityField targetField)
        {
            // Add your validation logic here. For example, you might want to validate if the source field type
            // is compatible with the target field type. If they are not compatible, return false.

            if (sourceField.fieldtype != targetField.fieldtype)
            {
                // The field types are not the same, so we consider the mapping invalid
                return false;
            }

            // All checks passed, so we consider the mapping valid
            return true;
        }
        // Your method
        private void DrawTableHeader(string headerText, SKPoint position, bool isSourceEntity)
        {
            var headerPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.LightBlue
            };

            var headerTextPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black,
                TextSize = 14
            };

            // Use the corresponding width according to the entity
            int entityWidth = isSourceEntity ? _sourcewidth : _targetwidth;

            //// If there are no fields in the entity, default to columnWidth
            //if (isSourceEntity && _sourceEntity.Fields.Count == 0 ||
            //    !isSourceEntity && _targetEntity.Fields.Count == 0)
            //{
            //    entityWidth = 2 * columnWidth;
            //}

            float textHeight = headerTextPaint.FontMetrics.Descent - headerTextPaint.FontMetrics.Ascent;
            headerHeight = (float)(Math.Ceiling(textHeight) + 2 * padding);
        
           

            var headerRect = new SKRect(position.X, position.Y, position.X + 2 * columnWidth, position.Y + headerHeight);
            _canvas.DrawRect(headerRect, headerPaint);

            // Draw header text after drawing the header rectangle
            _canvas.DrawText(headerText, position.X + padding, position.Y + padding + headerHeight / 2, headerTextPaint);

            // Save the header rectangle
            if (isSourceEntity)
            {
                _sourceheaderRect = headerRect;
            }
            else
            {
                _targetheaderRect = headerRect;
            }
        }
        private void DrawEntityBorder(SKPoint position, IEntityStructure entity, bool isSourceEntity)
        {
            var borderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2
            };

            // Use the corresponding width and height according to the entity
            int entityWidth = isSourceEntity ? _sourcewidth : _targetwidth;
            int entityHeight = isSourceEntity ? _sourceheight : _targetheight;

            // If there are no fields in the entity, default to some predefined values
            if (entity.Fields.Count == 0)
            {
                entityWidth = 2 * columnWidth; // This is just an example, you can choose another width
                entityHeight = (int)(2 * headerHeight); // Space for table name and table header
            }

            var borderRect = new SKRect(position.X, position.Y, position.X + entityWidth, position.Y + entityHeight);
            _canvas.DrawRect(borderRect, borderPaint);
        }
        private int CalculateEntityHeight(IEntityStructure entity, SKPaint textPaint)
        {
            float textHeight = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
            int headerHeight = (int)Math.Ceiling(textHeight) + 2 * padding;
            int rowHeight = (int)Math.Ceiling(textHeight) + padding;
            return headerHeight + (entity.Fields.Count * rowHeight);
        }
        private int CalculateEntityWidth(IEntityStructure entity, SKPaint textPaint)
        {
            // Start with the width of the header text
            int entityWidth = (int)textPaint.MeasureText(entity.EntityName);
            if (entity.Fields.Count > 0)
            {
                // Loop over all fields in the entity
                foreach (var field in entity.Fields)
                {
                    // Measure the text width of each field
                    int fieldWidth = (int)textPaint.MeasureText(field.fieldname) + 2 * padding;

                    // Update the entityWidth if the current fieldWidth is larger
                    if (fieldWidth > entityWidth)
                    {
                        entityWidth = fieldWidth;
                    }
                }
            }
            else
                entityWidth = 2*columnWidth;


            // Adding 2*padding to account for the padding in each side
            return entityWidth + 2 * padding;
        }
        private SKRect GetEntityHeaderRect(IEntityStructure entity)
        {
            return _entityHeaderRects[entity];
        }
        protected bool ReplaceEntity(SKPoint skPoint)
        {
            
            if (_sourceEntityRect.Contains(skPoint))
            {
                // Drop happened over source entity
                _sourceEntity = _draggedEntity;

                SourceMappedFieldsStatus = new Dictionary<string, bool>();

                foreach (var fieldt in _sourceEntity.Fields)
                {
                    SourceMappedFieldsStatus.Add(fieldt.fieldname, false);
                }
            }
            else if (_targetEntityRect.Contains(skPoint))
            {
                // Drop happened over target entity
                _targetEntity = _draggedEntity;
                TargetMappedFieldsStatus = new Dictionary<string, bool>();
                foreach (var fieldt in _targetEntity.Fields)
                {
                    TargetMappedFieldsStatus.Add(fieldt.fieldname, false);
                }
            }else
                return false;
           return true;
        }
        protected bool DropFieldFromSourceToTarget(SKPoint skPoint)
        {
            if (_targetEntityRect.Contains(skPoint))
            {
                // Check if drop point is over a valid field in _targetEntity.
                var (targetField, targetentity) = FindFieldAtPoint(skPoint);
                if (targetField != null && _draggedField != null && ParentOf(targetField) == _targetEntity)
                {
                    OnFieldDropped(_draggedField, targetField);
                    // Create a connection between field and targetField
                    // Add code to manage connections.
                    // Then request a redraw to show the new connection
                    IsDraw = true;
                    return true;
                }else return false;

            }else return false;
        }
        protected IEntityStructure IsHeaderHit(SKPoint point)
        {
            if (_sourceheaderRect.Contains(point))
            {
                return _sourceEntity;
            }
            else if (_targetheaderRect.Contains(point))
            {
                return _targetEntity;
            }

            return null;
        }
        #region "Events"
        protected void OnFieldDropped(IEntityField sourceField, IEntityField targetField)
        {
            // Your validation logic here, for example, you might want to check if the field types are compatible
            if (!ValidateMapping(sourceField, targetField))
            {
                // The validation failed, so we return without adding the mapping
                return;
            }

            var newMapping = new Mapping_rep_fields()
            {
                FromEntityName = sourceField.EntityName,
                FromFieldIndex = sourceField.FieldIndex,
                FromFieldName = sourceField.fieldname,
                FromFieldType = sourceField.fieldtype,
                ToEntityName = targetField.EntityName,
                ToFieldIndex = targetField.FieldIndex,
                ToFieldName = targetField.fieldname,
                ToFieldType = targetField.fieldtype
            };

            if (!_fieldMappings.Contains(newMapping))
            {
                // Add the new mapping to the list
                _fieldMappings.Add(newMapping);

                // Clean up
                _draggedField = null;

                // Mark the fields as mapped
                SourceMappedFieldsStatus[sourceField.fieldname] = true;
                TargetMappedFieldsStatus[targetField.fieldname] = true;
            }

            // Redraw the visualizer to reflect the new mapping
           
        }
       
        #endregion"Events"
        #region "Mouse Events"
        public void OnMouseDown(object sender, PointEventArgs e)
        {
            var mousePos = new SKPoint(e.X, e.Y);
            var (hitField, hitEntity) = FindFieldAtPoint(mousePos);
            var (hitLine, hitMapping) = FindLineAtPoint(mousePos);
            IsDraggingOn = false;
            if (hitField != null )
            {
                OnFieldSelected(hitField, hitEntity, e.IsRightButton);
                if (hitEntity != null)
                {
                    if (hitEntity == _sourceEntity)
                    {
                        sourceSelectedField = hitField;

                    }
                    else
                    {
                        targetSelectedField = hitField;
                    }
                     IsDraw= true;
                   
                }
            }
            else if (hitLine != null)
            {
                OnLineSelected(hitLine, hitMapping, e.IsRightButton);
            }
            else if (hitEntity != null)
            {
                OnHeaderSelected(null, hitEntity, e.IsRightButton);
            }
            else
            {
                _startPoint = mousePos;
                _endPoint = mousePos;
            }
            if (IsDraw) { Redraw(); }
        }
        public void OnMouseUp(object sender, PointEventArgs e)
        {
            var mousePos = new SKPoint(e.X, e.Y);
            var (hitField, hitEntity) = FindFieldAtPoint(mousePos);
            var skPoint = new SKPoint(e.X, e.Y);
            var (field, entity) = FindFieldAtPoint(skPoint);
            if (IsLeftMouseButtonPresses)
            {
                var currentPoint = new SKPoint(e.X, e.Y);
                if (SKPoint.Distance(mouseDownPoint, currentPoint) <= dragThreshold)
                {
                    // The user has clicked the mouse
                    // Your selection logic goes here...
                }
                IsDraggingOn = false;
                // Reset the flag
                IsLeftMouseButtonPresses = false;
            }
            IsDraggingOn =false; 
            if (field == null && entity != null)
            {
                // Handle entity header release here
            }
            if (_selectedField != null && hitField != null && hitField != _selectedField)
            {
                OnFieldDropped(_selectedField, hitField);
                _selectedField = null;
            }
            else if (hitEntity != null)
            {
                OnHeaderDropped(null, hitEntity);
            }
            else
            {
                _endPoint = mousePos;
            }

            if (IsDraw) { Redraw(); }
        }
        public void OnMouseMove(object sender, PointEventArgs e)
        {
            var mousePos = new SKPoint(e.X, e.Y);
            _endPoint = mousePos;
           
            var (hitField, hitEntity) = FindFieldAtPoint(mousePos);
            // If an entity is being dragged, update the temporary drag end point
            if (IsDraggingOn)
            {
                _dragEndPoint = new SKPoint(e.X, e.Y);
                
                // IsDraw = true;
            }

            //if (hitField != _hoveredField)
            //{
            //    //_hoveredField = hitField;
            //    // Force a redraw to show the highlight change
            // //   IsDraw = true;
            //}
            if (IsLeftMouseButtonPresses)
            {
                var currentPoint = new SKPoint(e.X, e.Y);
                if (SKPoint.Distance(mouseDownPoint, currentPoint) > dragThreshold)
                {
                    // The user has dragged the mouse
                    // Your drag logic goes here...
                    _draggedField = hitField;
                    IsDraggingOn=true;
                    IsDraw=true; 
                }
            }
            if(IsDraw) { Redraw(); }
        }
        #endregion "Mouse Events"

    }

}



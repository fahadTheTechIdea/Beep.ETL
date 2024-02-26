using Beep.ETL.Mapping.Logic;
using Beep.Skia;
using Beep.Winform.Vis.Helpers;
using BeepEnterprize.Vis.Module;
using OpenTK.Platform.Windows;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Workflow;
using TheTechIdea.Util;

namespace Beep.ETL.Mapping
{
    public class Winform_DrawingManager : EntityMappingVisualizer
    {
       
        private Point dragCursorPoint;
        private Point dragFormPoint;
        private SKGLControl _skControl;
        public IDMEEditor dMEEditor { get; set; }
        public Winform_DrawingManager(IEntityStructure sourceEntity, IEntityStructure targetEntity, List<IMapping_rep_fields> fieldMappings, SKGLControl sKControl, IDMEEditor _dMEEditor)
            : base(sourceEntity, targetEntity, fieldMappings)
        {
            _skControl = sKControl;
            dMEEditor = _dMEEditor;
            // Set up event handlers
            _skControl.MouseDown += _skControl_MouseDown;// => OnMouseDown(sender, new PointEventArgs { X = e.X, Y = e.Y , IsRightButton= e.Button == MouseButtons.Right });
            _skControl.MouseMove += _skControl_MouseMove;
            _skControl.MouseUp += _skControl_MouseUp;
            _skControl.PaintSurface += (sender, e) => Draw(e.Surface.Canvas);
            // Allow drop
            _skControl.AllowDrop = true;
            _skControl.DragEnter += _skControl_DragEnter;
            _skControl.DragDrop += _skControl_DragDrop;
        }
        public Winform_DrawingManager(SKGLControl sKControl, IDMEEditor _dMEEditor)
        {
            dMEEditor = _dMEEditor;
            _skControl = sKControl;
            // Set up event handlers
            _skControl.MouseDown += _skControl_MouseDown;// => OnMouseDown(sender, new PointEventArgs { X = e.X, Y = e.Y , IsRightButton= e.Button == MouseButtons.Right });
            _skControl.MouseMove += _skControl_MouseMove;
            _skControl.MouseUp += _skControl_MouseUp;
            _skControl.PaintSurface += (sender, e) => Draw(e.Surface.Canvas);
            // Allow drop
            _skControl.AllowDrop = true;

            _skControl.DragEnter += _skControl_DragEnter;
            _skControl.DragDrop += _skControl_DragDrop;
        }
        private void _skControl_DragDrop(object sender, DragEventArgs e)
        {
            bool IsEntitySafe = false;
            bool IsEntity = false;
            bool IsField = false;
          
            IEntityField field = new EntityField();
            // Get the IEntityStructure from the drag event
            if (e.Data.GetDataPresent(typeof(IEntityStructure)))
            {
                _draggedEntity = (IEntityStructure)e.Data.GetData(typeof(IEntityStructure));
                // Handle IEntityStructure...
                IsEntity = true;
            }
            else if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode n = (TreeNode)e.Data.GetData(typeof(TreeNode));
                IBranch branch = (IBranch)n.Tag;
                if (branch != null)
                {
                    if (branch.EntityStructure==null)
                    {
                        IDataSource ds = dMEEditor.GetDataSource(branch.DataSourceName);
                        if (ds != null)
                        {
                            ds.Openconnection();
                            if(ds.ConnectionStatus== System.Data.ConnectionState.Open)
                            {
                                _draggedEntity = ds.GetEntityStructure(_draggedEntity.EntityName, true);
                                IsEntitySafe = true;
                                ds.Closeconnection();
                            }
                        }
                    }
                    _draggedEntity = branch.EntityStructure;
                }
                // Handle TreeNode...
                IsEntity = true;
            }
            else if (e.Data.GetDataPresent(typeof(EntityField)))
            {
                field = (IEntityField)e.Data.GetData(typeof(EntityField));

                IsField = true;
            }

            // Find if the drop point is over the source or target entity header
            var point = _skControl.PointToClient(new Point(e.X, e.Y));
            var (_, droppedOnEntity) = FindFieldAtPoint(new SKPoint(point.X, point.Y));
            // Find if the drop point is over the source or target entity

            SKPoint skPoint = new SKPoint(point.X, point.Y);



            if (IsEntity)
            {
                if (_draggedEntity != null)
                {
                    if (_draggedEntity.Fields.Count == 0)
                    {
                        IDataSource ds = dMEEditor.GetDataSource(_draggedEntity.DataSourceID);
                        _draggedEntity = ds.GetEntityStructure(_draggedEntity.EntityName, false);
                        if (_draggedEntity.Fields.Count == 0)
                        {
                            ds.Openconnection();
                            if (ds.ConnectionStatus == System.Data.ConnectionState.Open)
                            {
                                _draggedEntity = ds.GetEntityStructure(_draggedEntity.EntityName, true);
                                IsEntitySafe = true;
                                ds.Closeconnection();
                            }
                        }
                        else
                        {
                            IsEntitySafe = true;
                        }
                    }
                    else
                    {
                        IDataSource ds = dMEEditor.GetDataSource(_draggedEntity.DataSourceID);
                        _draggedEntity = ds.GetEntityStructure(_draggedEntity.EntityName, false);
                        if (_draggedEntity.Fields.Count == 0)
                        {
                            ds.Openconnection();
                            if (ds.ConnectionStatus == System.Data.ConnectionState.Open)
                            {
                                _draggedEntity = ds.GetEntityStructure(_draggedEntity.EntityName, true);
                                IsEntitySafe = true;
                                ds.Closeconnection();
                            }
                        }
                        else
                        {
                            IsEntitySafe = true;
                        }
                    }
                }

               
            }
            if (IsEntitySafe)
            {
               if(ReplaceEntity(skPoint))
                {
                    // Redraw the UI
                    IsDraw = true; ;
                }

               
            }
            if (IsField)
            {
                DropFieldFromSourceToTarget(skPoint);
            }
            if (IsDraw)
            {
                Redraw();
            }
        }
        private void _skControl_DragEnter(object sender, DragEventArgs e)
        {
            // Only allow IEntityStructure objects to be dropped
            if (e.Data.GetDataPresent(typeof(IEntityStructure)) || e.Data.GetDataPresent(typeof(TreeNode)) || e.Data.GetDataPresent(typeof(EntityField)))
            {
                if (e.Data.GetData(typeof(TreeNode)).Is<TreeNode>())
                {
                    e.Effect = DragDropEffects.Move;
                    TreeNode n = (TreeNode)e.Data.GetData(typeof(TreeNode));
                    IBranch branch = (IBranch)n.Tag;
                    if (branch != null)
                    {
                        _draggedEntity = branch.EntityStructure;
                    }
                }
                if (e.Data.GetData(typeof(EntityField)).Is<EntityField>())
                {
                    // Handle TreeNode...
                    _draggedField=(IEntityField)e.Data.GetData(typeof(EntityField));
                    e.Effect = DragDropEffects.Move;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void _skControl_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(sender, new PointEventArgs { X = e.X, Y = e.Y, IsRightButton = e.Button == MouseButtons.Right });
           
            if (IsDraw)
            {
                Redraw();
            }
        }
        private void _skControl_MouseDown(object sender, MouseEventArgs e)
        {
            var point = new PointEventArgs { X = e.X, Y = e.Y, IsRightButton = e.Button == MouseButtons.Right };
            var skPoint = new SKPoint(e.X, e.Y);
            IsDraw = false;
            mouseDownPoint = new SKPoint(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                IsLeftMouseButtonPresses = true;
                IsRightMouseButtonPresses = false;

            }
            if (e.Button == MouseButtons.Right)
            {
                IsLeftMouseButtonPresses = false;
                IsRightMouseButtonPresses = true;

            }
            IEntityStructure hitEntity = IsHeaderHit(skPoint);
            //var (field, _) = FindFieldAtPoint(skPoint);
           
            OnMouseDown(hitEntity, point);
            if (hitEntity != null)
            {
                // Handle entity header click...
            }
            if (IsDraw)
            {
                Redraw();
            }

        }
        private void _skControl_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(sender, new PointEventArgs { X = e.X, Y = e.Y });
           
            if (IsDraggingOn && _draggedField!=null)
            {
                _skControl.DoDragDrop(_draggedField, DragDropEffects.Move);
            }
            //if (IsDraw)
            //{
            //    Redraw();
            //}
        }
        public override void Redraw()
        {
            IsDraw = false;
            _skControl.Invalidate();
        }
        public IErrorsInfo ShowLineSelectedMenu()
        {
            try
            {

            }
            catch (Exception ex)
            {

                throw;
            }
            return dMEEditor.ErrorObject;
        }
        public IErrorsInfo ShowFieldSelectedMenu()
        {
            try
            {

            }
            catch (Exception ex)
            {

                throw;
            }
            return dMEEditor.ErrorObject;
        }

        protected override void OnFieldSelected(IEntityField field, IEntityStructure entity, bool isRightClick)
        {
            //if(field != null)
            //{
            //    if (isRightClick)
            //    {
            //        if(entity==_targetEntity)
            //        {

            //        }
            //    }
            //}else if (entity != _sourceEntity)
            //{
            //    _draggedField = field;
            //}
      
          
        }
        protected override void OnLineSelected(Line line, IMapping_rep_fields mapping, bool isRightClick)
        {
            // Do something when a line is selected. For example, you might want to change the appearance of the line,
            // or display some additional information about the mapping. 
            // The actual action depends on the specifics of your application.
            // Note: you can use the 'mapping' parameter to access the details of the mapping that was selected.

            // If you want to store the currently selected mapping, you could add a property to your class, e.g.:
            // SelectedMapping = mapping;

            Redraw();
        }
        protected override void OnHeaderSelected(IEntityField field, IEntityStructure entity, bool isRightClick)
        {
            return;
        }
        protected override void OnHeaderDropped(IEntityField field, IEntityStructure entity)
        {
            return;
        }
        public void ShowMenu(object sender, MouseEventArgs e, List<(string menutext, string icontext, string forcolor, string backcolor, Action functiontoexecute)> menuItems)
        {
            // Create a new ContextMenuStrip
            ContextMenuStrip menu = new ContextMenuStrip();

            // Populate the ContextMenuStrip based on the provided menuItems
            foreach (var menuItem in menuItems)
            {
                ToolStripMenuItem item = new ToolStripMenuItem
                {
                    Text = menuItem.menutext,
                    // Assuming that icontext is the path to the icon file
                    Image = Image.FromFile(menuItem.icontext),
                    // Assuming that forcolor is a color in hex format
                    ForeColor = ColorTranslator.FromHtml(menuItem.forcolor),
                    // Assuming that backcolor is a color in hex format
                    BackColor = ColorTranslator.FromHtml(menuItem.backcolor),
                };

                item.Click += (sender, e) => menuItem.functiontoexecute();

                menu.Items.Add(item);
            }

            // Show the context menu at the current mouse position
            menu.Show(_skControl.PointToClient(Cursor.Position));
        }
    
    }
}




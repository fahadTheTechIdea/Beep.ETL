
using Beep.Winform.Vis.FunctionsandExtensions;
using BeepEnterprize.Vis.Module;
using System.Windows.Forms.VisualStyles;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Logger;
using TheTechIdea.Util;


namespace Beep.ETL.Mapping.Skia
{
    [AddinAttribute(Caption = "Beep Mapping", Name = "Beep Mapper", misc = "App", ObjectType = "Beep", addinType = AddinType.Control, displayType = DisplayType.InControl)]
    public partial class uc_MappingControl : UserControl, IDM_Addin
    {
        public uc_MappingControl()
        {
            InitializeComponent();
        }

        public string ParentName { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public string AddinName { get; set; }
        public string Description { get; set; }
        public bool DefaultCreate { get; set; }
        public string DllPath { get; set; }
        public string DllName { get; set; }
        public string NameSpace { get; set; }
        public IErrorsInfo ErrorObject { get; set; }
        public IDMLogger Logger { get; set; }
        public IDMEEditor DMEEditor { get; set; }
        public EntityStructure EntityStructure { get; set; }
        public string EntityName { get; set; }
        public IPassedArgs Passedarg { get; set; }
        IProgress<PassedArgs> progress;
        CancellationToken token;
        IBranch RootAppBranch;
        IBranch branch;
        public IVisManager Visutil { get; set; }
        FunctionandExtensionsHelpers extensionsHelpers;
        Winform_DrawingManager DrawingManager;

        private bool isDragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        public void Run(IPassedArgs pPassedarg)
        {
            if (pPassedarg != null)
            {
                if (pPassedarg.ObjectType != null)
                {
                    if (pPassedarg.ObjectType.Equals("CODE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(pPassedarg.ObjectName))
                        {

                        }
                    }

                }

            }
        }

        public void SetConfig(IDMEEditor pbl, IDMLogger plogger, IUtil putil, string[] args, IPassedArgs e, IErrorsInfo per)
        {
            Passedarg = e;
            Logger = plogger;
            ErrorObject = per;
            DMEEditor = pbl;
            //Python = new PythonHandler(pbl,TextArea,OutputtextBox, griddatasource);

            Visutil = (IVisManager)e.Objects.Where(c => c.Name == "VISUTIL").FirstOrDefault().obj;

            if (e.Objects.Where(c => c.Name == "Branch").Any())
            {
                branch = (IBranch)e.Objects.Where(c => c.Name == "Branch").FirstOrDefault().obj;
            }
            if (e.Objects.Where(c => c.Name == "RootAppBranch").Any())
            {
                RootAppBranch = (IBranch)e.Objects.Where(c => c.Name == "RootAppBranch").FirstOrDefault().obj;
            }
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            DrawingManager = new Winform_DrawingManager(skControl1, DMEEditor);
            this.isDragging = true;
            this.dragCursorPoint = Cursor.Position;
            this.dragFormPoint = this.Location;
            this.splitContainer1.AllowDrop = true;
            this.splitContainer1.Panel2.AllowDrop = true;
            skControl1.AllowDrop = true;
            //this.splitContainer1.Panel2.MouseUp += Uc_MappingControl_MouseDown;
            //this.splitContainer1.Panel2.MouseMove += Uc_MappingControl_MouseMove;
            //this.splitContainer1.Panel2.MouseUp += Uc_MappingControl_MouseUp;
        }


    }
}

using Beep.Winform.Vis.FunctionsandExtensions;
using BeepEnterprize.Vis.Module;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Util;

namespace Beep.ETL.Mapping.Logic
{
    [AddinAttribute(Caption = "Mapping", Name = "MappingMenuFunctions", ObjectType = "Beep", menu = "Beep", misc = "IFunctionExtension", addinType = AddinType.Class, iconimage = "mapping.ico", order = 1)]
    public class MappingFunctions : IFunctionExtension
    {
        public IDMEEditor DMEEditor { get; set; }
        public IPassedArgs Passedargs { get; set; }

        private BasicFunctionandExtensionsHelpers ExtensionsHelpers;


        public MappingFunctions(IDMEEditor pdMEEditor, IVisManager pvisManager, ITree ptreeControl)
        {
            DMEEditor = pdMEEditor;

            ExtensionsHelpers = new BasicFunctionandExtensionsHelpers(DMEEditor, pvisManager, ptreeControl);
        }

        
        [CommandAttribute(Caption = "Mapping Manager", Name = "Mapping Manager", Click = true, iconimage = "newproject.ico", ObjectType = "Beep", PointType = EnumPointType.Global)]
        public IErrorsInfo NewProject(IPassedArgs Passedarguments)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {

                ExtensionsHelpers.GetValues(Passedarguments);
                ExtensionsHelpers.Vismanager.ShowPage("uc_MappingControl", (PassedArgs)DMEEditor.Passedarguments,DisplayType.InControl);
                // DMEEditor.AddLogMessage("Success", $"Open Data Connection", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Fail", $"Could not create new project {ex.Message}", DateTime.Now, 0, Passedarguments.DatasourceName, Errors.Failed);
            }
            return DMEEditor.ErrorObject;

        }
       

    }
}

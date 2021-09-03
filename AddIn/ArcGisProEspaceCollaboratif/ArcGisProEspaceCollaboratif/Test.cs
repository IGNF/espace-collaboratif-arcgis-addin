using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;
using ArcGisProEspaceCollaboratif.ViewModels;

namespace ArcGisProEspaceCollaboratif
{
    internal class Test : Button
    {

        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                "Page de test",
                Constantes.INFORMATION
            );
        }
    }
}

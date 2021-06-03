using ArcGIS.Desktop.Framework.Contracts;

namespace ArcGisProEspaceCollaboratif
{
    internal class SeeReport : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Voir le signalement", "Espace collaboratif");
        }
    }
}

using ArcGIS.Desktop.Framework.Contracts;

namespace ArcGisProEspaceCollaboratif
{
    internal class DeleteReport : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Supprimer les signalements de la carte en cours", "Espace collaboratif");
        }
    }
}

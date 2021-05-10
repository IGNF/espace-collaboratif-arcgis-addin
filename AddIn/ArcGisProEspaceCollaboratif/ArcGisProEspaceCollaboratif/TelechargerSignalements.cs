using ArcGIS.Desktop.Framework.Contracts;

namespace ArcGisProEspaceCollaboratif
{
    internal class TelechargerSignalements : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Télécharger les signalements", "Espace collaboratif");
        }
    }
}

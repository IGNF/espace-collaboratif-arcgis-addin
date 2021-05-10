using ArcGIS.Desktop.Framework.Contracts;

namespace ArcGisProEspaceCollaboratif
{
    internal class VoirSignalement : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Voir le signalement", "Espace collaboratif");
        }
    }
}

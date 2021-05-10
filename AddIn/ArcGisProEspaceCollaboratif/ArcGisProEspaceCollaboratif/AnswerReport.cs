using ArcGIS.Desktop.Framework.Contracts;

namespace ArcGisProEspaceCollaboratif
{
    internal class AnswerReport : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Répondre à un signalement", "Espace collaboratif");
        }
    }
}

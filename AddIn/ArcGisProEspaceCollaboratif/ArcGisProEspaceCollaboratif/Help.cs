using ArcGIS.Desktop.Framework.Contracts;

namespace ArcGisProEspaceCollaboratif
{
    /*internal class HelpOpenDialogConfigure : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Aide-Configurer", "Espace collaboratif");
        }
    }*/

    internal class HelpOpenManual : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Aide-Ouvrir manuel", "Espace collaboratif");
        }
    }

    internal class HelpOpenFileLog : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Aide-Ouvrir log", "Espace collaboratif");
        }
    }

    internal class HelpAbout : Button
    {
        protected override void OnClick()
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Aide-A propos", "Espace collaboratif");
        }
    }

}

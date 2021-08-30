using ArcGIS.Desktop.Framework.Contracts;
using ArcGisProEspaceCollaboratif.Core;

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

            //Creat a Progress Bar user control
            /*var progressBarControl = new System.Windows.Controls.ProgressBar
            {
                //Configure the progress bar
                Minimum = 0,
                Maximum = 100,
                IsIndeterminate = true,
                Width = 300,
                Value = 10,
                Height = 25,
                Visibility = System.Windows.Visibility.Visible
            };
            //Create a MapViewOverlayControl. 
            var mapViewOverlayControl = new MapViewOverlayControl(progressBarControl, true, true, true, OverlayControlRelativePosition.BottomCenter, .5, .8);
            //Add to the active map
            MapView.Active.AddOverlayControl(mapViewOverlayControl);
            await Task.Delay(1000);
            //Remove from active map
            MapView.Active.RemoveOverlayControl(mapViewOverlayControl);*/

            //#2 create just a ProgressDialog for manual control
            /*ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progDialog = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog("Manual Dialog with programmatic show and hide");
            progDialog.Show();
            await DoSomeWork(progDialog);*/

            /*uint maxSteps = 10;
            var pd = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog("Running SPU", maxSteps, false);
            ProgressorSource cps = new ProgressorSource(pd);
            cps.Progressor.Max = (uint)maxSteps;

            await QueuedTask.Run(() =>
            {
                //check every second
                for (int i = 0; i < 10; i++)
                {
                    cps.Progressor.Value += 1;
                    cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
                    cps.Progressor.Message = "Message " + cps.Progressor.Value;

                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("RunCancelableProgress Loop{0}", cps.Progressor.Value));
                    }
                    //block the CIM for a second
                    Task.Delay(1000).Wait();
                    //are we done?
                    if (cps.Progressor.Value == cps.Progressor.Max) break;
                }


            }, cps.Progressor);*/
        }

            /*private static Task DoSomeWork(ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progDialog)
            {
                return QueuedTask.Run(async () =>
                {
                    for (uint iSeconds = 0; iSeconds < 10; iSeconds++)
                    {
                        await Task.Delay(1000);
                    }
                    progDialog.Hide();
                });
            }*/
    }
}

using System.Windows.Forms;
using System;

namespace ArcGisProEspaceCollaboratif.Core
{
    public class DropDownTreeView : TreeView
    {
        public DropDownTreeView() : base()
        {
        }

        private ComboBoxTreeNode m_CurrentNode = null;

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            // Are we dealing with a dropdown node?
            if (e.Node is ComboBoxTreeNode)
            {
                this.m_CurrentNode = (ComboBoxTreeNode)e.Node;

                // Need to add the node's ComboBox to the
                // TreeView's list of controls for it to work
                this.Controls.Add(this.m_CurrentNode.ComboBox);

                // Listen to the SelectedValueChanged
                // event of the node's ComboBox
                this.m_CurrentNode.ComboBox.SelectedValueChanged +=
                   new EventHandler(ComboBox_SelectedValueChanged);
                this.m_CurrentNode.ComboBox.DropDownClosed +=
                   new EventHandler(ComboBox_DropDownClosed);

                // Now show the ComboBox
                this.m_CurrentNode.ComboBox.Show();
                this.m_CurrentNode.ComboBox.Location = e.Location; 
                this.m_CurrentNode.ComboBox.DroppedDown = true;
            }
            base.OnNodeMouseClick(e);
        }

        void ComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            HideComboBox();
        }

        void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            HideComboBox();
        }
        private void HideComboBox()
        {
            if (this.m_CurrentNode != null)
            {
                // Unregister the event listener
                this.m_CurrentNode.ComboBox.SelectedValueChanged -=
                                     ComboBox_SelectedValueChanged;
                this.m_CurrentNode.ComboBox.DropDownClosed -=
                                     ComboBox_DropDownClosed;

                // Copy the selected text from the ComboBox to the TreeNode
                this.m_CurrentNode.Text = this.m_CurrentNode.ComboBox.Text;

                // Hide the ComboBox
                this.m_CurrentNode.ComboBox.Hide();
                this.m_CurrentNode.ComboBox.DroppedDown = false;

                // Remove the control from the TreeView's
                // list of currently-displayed controls
                this.Controls.Remove(this.m_CurrentNode.ComboBox);

                // And return to the default state (no ComboBox displayed)
                this.m_CurrentNode = null;
            }

        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HideComboBox();
            base.OnMouseWheel(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);

        }
    }
}

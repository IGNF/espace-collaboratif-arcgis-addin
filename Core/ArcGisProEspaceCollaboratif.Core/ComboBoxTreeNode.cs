using System.Windows.Forms;

namespace ArcGisProEspaceCollaboratif.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ComboBoxTreeNode : TreeNode
    {
        private ComboBox _comboBox = new ComboBox();

        public ComboBox ComboBox
        {
            get
            {
                this._comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                return this._comboBox;
            }
            set
            {
                this._comboBox = value;
                this._comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace swgMacros
{
    public partial class commandEditor : Form
    {
        Parser parser;
        Main form1;
        public commandEditor(Parser inp, Main inp2)
        {
            InitializeComponent();
            parser = inp;
            update(null);
            form1 = inp2;
            btnAdd.Enabled = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Command temp;
            txtname.Text = txtname.Text.Replace(";", "").Replace("/","");
            txtrules.Text = txtrules.Text.Replace(";", "").Replace(" ", "");
            txtdesc.Text=txtdesc.Text.Replace(";", "");
            txtcategory.Text = txtcategory.Text.Replace(";", "");
            if (null == (temp = parser.getCommand(txtname.Text)))
            { parser.addCommand(txtname.Text.Replace(";", ""), txtrules.Text, txtdesc.Text,txtcategory.Text); }
            else
            {
                temp.setDescription(txtdesc.Text);
                temp.setParameters(txtrules.Text);
                temp.setCategory(txtcategory.Text);
            }
            update(null);
            btnAdd.Enabled = false;

        }

        private void commandbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Command temp=null;
            if (null != (temp = parser.getCommand(commandbox.SelectedItem.ToString())))
            {
                txtname.Text = temp.getName();
                txtrules.Text = temp.getParameters();
                txtdesc.Text = temp.getDescription();
                txtcategory.Text = temp.getCategory();
            }
            btnAdd.Enabled = false;
        }

        private void commandEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            form1.setCommopen();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            form1.setCommopen();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Are You sure you want to remove " + txtname.Text + " from the database?", "Remove?", MessageBoxButtons.YesNo))
            { parser.removeCommand(txtname.Text); }
            update(null);
        }

        private void txtname_TextChanged(object sender, EventArgs e)
        {
            btnAdd.Enabled = true;
        }
        private void update( Control sender )
        {
            categorybox.Enabled = false; string temp = categorybox.Text;
            categorybox.Items.Clear(); categorybox.Items.AddRange(parser.listCategorys());
            categorybox.Text = temp;
            categorybox.Enabled = true;
            commandbox.Items.Clear(); commandbox.Items.AddRange(parser.listCommands(txtsearch.Text, categorybox.Text));
            if (commandbox.Items.Count == 0) { commandbox.Items.Add("No result"); } else { commandbox.SelectedIndex = commandbox.Items.IndexOf(txtname.Text); if (sender != null) { sender.Focus(); } else { commandbox.Focus(); } } 
        }

        private void txtsearch_TextChanged(object sender, EventArgs e)
        {
            update((Control)sender);
        }

        private void category_Changed(object sender, EventArgs e)
        {
            if (categorybox.Enabled)
            {
              
                update((Control)sender);
            }

        }

    }
}

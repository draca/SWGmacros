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
    public partial class Main : Form
    {
        static MacroList list;
        static bool changed=false;
        static bool filechanged=false;
        static Parser parser;
        static bool comopen=false;
        public Main()
        {
            InitializeComponent();
            list = new MacroList();
            parser = new Parser(list);
            txtIcon.Items.AddRange(parser.listIcons());
            errorbox.Items.Add(parser.sizeIcons().ToString() + " Icon names loaded...");
            errorbox.Items.Add(parser.sizeCommands().ToString() + " Commands loaded...");
            
        }
        public bool saveFile()
        {
                if (filechanged)
                {
                    DialogResult dr = MessageBox.Show("Want to save Macro file before contine?", "Save?", MessageBoxButtons.YesNoCancel);
                    if(dr == DialogResult.Yes)
                    {
                        filechanged = false;
                        saveToolStripMenuItem_Click(null, null);
                        return true;
                    }
                    else if (dr == DialogResult.No) { filechanged = false; return true; }
                    else { return false; }

                }
                return true;
            
        }
        private bool checkfield()
        {
            if (changed)
            {
                DialogResult dr = MessageBox.Show("Want to save Macro before contine?", "Save?", MessageBoxButtons.YesNoCancel);
                if (DialogResult.Yes == dr) { saveToolStripMenuItem1_Click(null, null); changed = false; return true; }
                else if (DialogResult.No == dr) { changed = false; return true; }
                else { return false; }
            }
            return true;
        }


        private void macros_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkfield())
            {
                Macro temp = list.findMacro((String)macros.SelectedItem);
                txtmacro.Text = "";
               
                txtName.Text = temp.getTitle();
                txtIcon.Text = temp.getIcon();
                txtmacro.Text = temp.getText().Replace(";", ";\n");
                changed = false;
            }
 
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkfield();
            saveFile();  
            loadfile(false);
            
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkfield();
            DialogResult dr;
            if (saveDia.FileName == "") { dr = saveDia.ShowDialog(); }else{dr=DialogResult.OK; }
            if (dr != DialogResult.Cancel) { list.tofile(saveDia.FileName.ToString()); }
            filechanged = false;  
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Macro temp;
          
          
           if (txtName.Text != "")
           {
               errorFix();
               if ((temp = list.findMacro(txtName.Text)) == null)
               { list.add(txtName.Text, txtIcon.Text, "#ffffff", txtmacro.Text.Replace(";\n",";").Replace("\n",";")); update(); }
               else { temp.setText(txtmacro.Text.Replace(";\n", ";")); }
               txtName.Text = temp.getTitle();
               txtIcon.Text = temp.getIcon();
               txtmacro.Text = temp.getText();
               changed = false;
               filechanged = true;
           }
           else { MessageBox.Show("The macro need a name"); }
        }
        private void update()
        {
            macros.Items.Clear();
            macros.Items.AddRange(list.listX());
            macros.SelectedIndex = macros.Items.IndexOf(txtName.Text);
        }
        

        private void txtmacro_TextChanged(object sender, EventArgs e)
        {
            changed = true;
            errorbox.Items.Clear();
            updateInfoField();
            errorbox.Items.AddRange(parser.parse(txtmacro.Text,txtName.Text,txtIcon.Text).Split("\n".ToCharArray()));
            
        }
        int calcRows()
        {
            return txtmacro.Text.Length - txtmacro.Text.Replace("\n", "").Length;
        }
        int calcWords()
        { return txtmacro.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length; }


        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (checkfield())
            {
                txtmacro.Text = txtName.Text = txtIcon.Text = ""; changed = false;
            }

        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            parser.saveConfig();

            checkfield();
            saveFile(); 
            
        }

        private void changeFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(DialogResult.Cancel != fontDia.ShowDialog())
            {
                txtmacro.Font = fontDia.Font;
                macros.Font = fontDia.Font;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string version = Application.ProductVersion.ToString();
            string name = Application.ProductName.ToString();
            MessageBox.Show(name + " \nVersion " + version + "\nCopyright \u00a9 2010 Draca \nContact: draca@live.se\nSWGEMU name: Krachera ", "About");
        }

        private void addCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            parser.addIcon(txtIcon.Text);
            txtIcon.Items.Add(txtIcon.Text);
            parser.saveConfig();

        }

        private void removeCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            parser.removeIcon(txtIcon.Text);
            txtIcon.Items.Remove(txtIcon.Text);
            parser.saveConfig();
        }

        private void commandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!comopen)
            { (new commandEditor(parser, this)).Show(); comopen = true; }
            else { MessageBox.Show("Command browser allready open!"); }
        }
        public void setCommopen()
        { comopen=false;}

        private void mergeLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkfield();
            loadfile(true);
        }
        private void loadfile(bool merge)
        {
            DialogResult dr;
            if (checkfield())
            {
                dr = dia.ShowDialog();
                if (!merge) { saveDia.FileName = dia.FileName; }
                if (dr != DialogResult.Cancel) { if (!list.loadFile(dia.FileName.ToString(), merge)) { MessageBox.Show("File format error, is this realy a macro file?","Read error",MessageBoxButtons.OK,MessageBoxIcon.Error); } }
                txtmacro.Text = txtName.Text = txtIcon.Text = "";
                update();
                filechanged = false;
                changed = false;

            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Sure you want to remove the macro " + txtName.Text + " ?", "Remove?", MessageBoxButtons.YesNo)) { list.remove(txtName.Text); }
            update();
            filechanged = true;
        }
        private int currentRow()
        {
           return txtmacro.Text.Substring(0, txtmacro.SelectionStart).Split("\n".ToCharArray()).Length;
        }
        private void updateInfoField(){lblstats.Text = "Selected Row: "+ currentRow() +" Characters: " + txtmacro.Text.Length + " Characters(no whitespace): " + txtmacro.Text.Replace(" ", "").Replace("\n", "").Length + " Words: " + calcWords() + " Rows: " + calcRows(); 
}

        private void txtmacro_SelectionChanged(object sender, EventArgs e)
        {
            updateInfoField();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkfield();
            DialogResult dr = saveDia.ShowDialog();
            if (dr != DialogResult.Cancel) { list.tofile(saveDia.FileName.ToString()); }
            filechanged = false;  
        }

        private void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (checkfield() && saveFile())
            {
                saveDia.FileName = "";
                macros.Items.Clear();
            }
            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void errorbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(DialogResult.Yes==MessageBox.Show("Fix all parse errors?","Fix erros?",MessageBoxButtons.YesNo,MessageBoxIcon.Question))
            {
                int count = errorbox.Items.Count;
                errorFix();
                MessageBox.Show("Fixed " + (count - errorbox.Items.Count) + " errors", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void errorFix()
        {

            parser.addIcon("admin");
            if (!parser.isIcon(txtIcon.Text)) { txtIcon.Text = "admin"; }
            txtName.Text = txtName.Text.Replace(" ", "").Replace(";", "");
            txtmacro.Text = parser.errorFixer(txtmacro.Text);
        }


    }
}

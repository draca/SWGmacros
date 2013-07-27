using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace swgMacros
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
    public class Command
    {
        String name;
        String parameters;
        String description;
        String category;

        public Command(String pname,String parameter,String description,String category )
        {
            name = pname;
            this.parameters = parameter;
            this.description = description;
            this.category = category;
            
        }
        public String getName() { return name; }
        public String getParameters() { return parameters; }
        public String getDescription() { return description; }
        public String fileFormat() { return name + ";" + parameters + ";" + description+";"+category ; }
        public String getCategory() { return category; }
        public void setParameters(string inp) { parameters = inp; }
        public void setDescription(string inp) { description = inp; }
        public void setCategory(string inp) { category = inp; }
       

    }

    public class Parser
    {
        // parameter value 
        // num string word macro numword none
        List<Command> commands;
        List<String> icons;
        MacroList macroList;
        String error;
        public Parser(MacroList macro)
        { macroList = macro; commands = new List<Command>(); icons = new List<String>(); loadConfig();}

        private bool isNum(String inp) { try { float.Parse(inp.Replace(".",",")); } catch { return false; } return true; }
        private bool isMacro(String inp) { return macroList.findMacro(inp) != null; }
        public Command getCommand(String inp) { return commands.Find(delegate(Command p) { return p.getName() == inp; }); }
        private bool isWord(String inp) { return !inp.Contains(";"); }
        public void addCommand(String name, String param, String description,String category) { if (getCommand(name) == null) { commands.Add(new Command(name, param, description,category)); } sortCommands(); }
        public void addIcon(String name) { if (!isIcon(name)) { icons.Add(name); } sortIcons(); }
        public void removeIcon(String name) { if (isIcon(name)) { icons.Remove(name); } }
        public bool isIcon(String icon) { return (null != icons.Find(delegate(String p) { return p == icon; })); }
        public void removeCommand(String name) { if (null != getCommand(name)) { commands.Remove(getCommand(name)); } }
        public int sizeIcons() { return icons.Count; }
        public int sizeCommands() { return commands.Count; }
        
        
        public String parse(String inp,String name, String icon) 
        {

            String errorstring="";
            String[] temp;
            inp = inp.Replace(";", " ;");
            temp = inp.Split("\n".ToCharArray());
            if (name.Contains(";")) { errorstring = errorstring + "Error macro name may not contain semi colon (;)\n"; }
            if (name.Contains(" ")) { errorstring = errorstring + "Error macro name may not contain spaces\n"; }
            if (!isIcon(icon)) { errorstring = errorstring + "Error invalid icon name\n"; }
           
            for (int i=0; i < temp.Length; i++)
            {
                if (temp[i].StartsWith("/"))
                {
                    if (!parseCommand(temp[i]))
                    {
                        errorstring = errorstring + "Error on Row " + (i + 1) + ": " + error + "\n";
                    }
                }
                else { if (!temp[i].EndsWith(";") && temp[i].Length>0) { errorstring = errorstring + "Error on Row " + (i + 1) + " Missing semicolon at the end\n"; } }
            }

            if (errorstring != "")
            {
                return errorstring;
            }
            else { return "No errors!"; }
        }
        private bool parseCommand(String line)
        {
            Command temp;
            String substring;
            String[] inp;
            int count=1,i=1;
            error = "";
            inp = line.Split(" ".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            temp = getCommand(substring=inp[0].Replace("/", ""));

            while (temp == null && i < inp.Length) { temp = getCommand(substring = substring + " " + inp[i]); i++; }
           
             

            if (temp != null) { count = temp.getName().Split(" ".ToCharArray()).Length; }
            
            if (temp == null) { error = " Unknown command"; return false; }
            else if (temp.getParameters() == "none") { error = "Missing semicolon at the end"; return inp[inp.Length - 1] == ";"; }
            else if (temp.getParameters() == "number") { try { if (!isNum(inp[count])) { error ="-- "+ inp[count].ToString() + " -- is not a number"; } return isNum(inp[count]) && inp[count + 1] == ";"; } catch { error = " Expecting name, a number and a semicolon"; return false; } }
            else if (temp.getParameters() == "macro") { error = " No macro by that name  "; try { return isMacro(inp[count]) && inp[count + 1] == ";"; } catch { error = " Expecting macroname and a semicolon"; return false; } }
            else if (temp.getParameters() == "wordnumber") { try { error = " Expecting player name(optional) number semicolon"; return (isWord(inp[count]) && isNum(inp[count + 1]) && inp[count + 2] == ";" || (isNum(inp[count])) && inp[count + 1] == ";"); } catch { return false; } }
            else if (temp.getParameters() == "strict") { try { error ="-- "+inp[count] + "-- is not a semicolon "; return inp[count] == ";"; } catch { error = " Missing ; (semicolon)"; return false; } }
            else { error = "Unknown rule for this command. Report as bug"; return false; } 
        }


        public string errorFixer(string inp)
        {
            if (inp.Split(" \n;".ToCharArray(),StringSplitOptions.RemoveEmptyEntries).Length==0) { return inp; }
            inp = inp.Replace("\n", ";\n").Replace(";;",";");
            if (!inp.EndsWith(";") && !inp.EndsWith("\n")) { inp = inp + ";"; }

            return inp;

        }
  /*      public void loadRawFile()
        {
            StreamReader file = new StreamReader(File.OpenRead("./commands.txt"));
            String temp,name="", text="";
            
            while(!file.EndOfStream)
            {
             temp =file.ReadLine();
             name = temp.Split(" ".ToCharArray())[0].Substring(1);
             text = temp.Substring(name.Length);
             addCommand(name,"none",text);
            }
            

        }
   */
        public bool loadConfig()
        {
            String[] name;
            String temp;
            
            try
            {
                StreamReader file = File.OpenText("./Data.txt");
                file.ReadLine();
                while ("--icons--" != (temp = file.ReadLine()) && !file.EndOfStream)
                {
                    name = temp.Split(";".ToCharArray());
                    
                    if (name.Length == 3) { addCommand(name[0], name[1], name[2],"Other"); }
                    if (name.Length == 4) { if (name[3] == "") { name[3] = "Other"; } addCommand(name[0], name[1], name[2], name[3]); }
                }
                while (!file.EndOfStream)
                {
                    temp = file.ReadLine();
                    addIcon(temp);
                }
                file.Close();
                return true;
            }
            catch { commands.Clear(); icons.Clear(); return false;  }
            

        }
        public void saveConfig()
        {
            if (commands.Count != 0 || icons.Count != 0)
            {
                StreamWriter temp = File.CreateText("./Data.txt");
                string str = "";
                //save commands
                temp.WriteLine("--commands--");
                commands.ForEach(delegate(Command p)
                { str = str + p.fileFormat() + "\n"; });
                temp.WriteLine(str);
                //save icons
                temp.WriteLine("--icons--");
                icons.ForEach(delegate(String p)
                { temp.WriteLine(p); });
                temp.Close();
            }

        }
        public void sortCommands()
        {
            commands.Sort(delegate(Command p1, Command p2)
                { return p1.getName().CompareTo(p2.getName()); });
        }
        public void sortIcons()
        {
            icons.Sort(delegate(String p1, String p2)
            { return p1.CompareTo(p2); });
        }
        public String[] listCommands(String search,String catergory)
        {
            String[] temp = new String[commands.Count];
            String[] list;
            int i = 0;
            commands.ForEach(delegate(Command p)
            { if ((p.getName().Contains(search) || search == "") && (catergory == p.getCategory() || catergory == "All")) { temp[i] = p.getName(); i++; } });

            list = new String[i];
            for (int j = 0; j < i; j++)
            { list[j] = temp[j]; }
                return list;

        }
        public String[] listCategorys()
        {
            List<String> category = new List<String>() ;
            

            category.Add("All");
            commands.ForEach(delegate(Command p)
            { if (null == category.Find(delegate(String x) { return x == p.getCategory(); })) { category.Add(p.getCategory()); } });

            return category.ToArray();

        }
        public String[] listIcons()
        {
            String[] temp = new String[icons.Count];
            int i = 0;
            icons.ForEach(delegate(String p)
            { temp[i] = p; i++; });
            return temp;

        }
       

    }

    public class Macro
    {
        String title;
        String icon;
        String text;
        String color;

        public Macro(string title,string icon,string color, string text)
        {
            setColor(color);
            setIcon(icon);
            setText(text);
            setTitle(title);
        }

        public void setTitle(string inp)
        { title=inp; }
        public void setIcon(string inp)
        { icon = inp; }
        public void setText(string inp)
        { text = inp; }
        public void setColor(string inp)
        { color = inp; }
        public String getTitle()
        { return title; }
        public String getIcon()
        { return icon; }
        public String getText()
        { return text; }
        public String getColor()
        { return color; }
        public override string  ToString()
        {
            return title + " " + icon + " " + color + " " + text;
        }

    }
    public class MacroList
    {
        List<Macro> macrolist;
        public MacroList()
        {
            macrolist = new List<Macro>();
        }
        public Macro findMacro(string macro){ return macrolist.Find(delegate(Macro p) { return p.getTitle() == macro; });}
        public void Sort()
        {
            macrolist.Sort(delegate(Macro p1, Macro p2)
            { return p1.getTitle().CompareTo(p2.getTitle()); });
        }
        public int add(string title, string icon, string color, string text) { if (findMacro(title) == null) { Macro newUs = new Macro(title, icon, color,text); macrolist.Add(newUs); Sort();  return 0; } return -1; }
        public void tofile(string path)
        {
            int i = 1;
           StreamWriter r = File.CreateText(path);
           r.Write("version: 0000\n");
            macrolist.ForEach(delegate(Macro p)
            { r.WriteLine(i + " " + p.ToString()); i++; });
            r.Close();
        }
        public bool loadFile(string path) { return loadFile(path, false); }

        public bool loadFile(string path,bool merge)
        {
            StreamReader read = new StreamReader(File.OpenRead(path));
            String temp;
            String[] split;
            if (!merge) { macrolist.Clear(); };
            try
            {
                temp = read.ReadLine();
                if (temp != "version: 0000") { return false; }
                while (!read.EndOfStream)
                {
                    temp = read.ReadLine();
                    split = temp.Split(" ".ToCharArray());
                    add(split[1], split[2], split[3], temp.Substring(split[0].Length + split[1].Length + split[2].Length + split[3].Length + 4));

                }
                read.Close();
                return true;
            }
            catch { return false; }


        }
        public void remove(string name) { Macro temp; if ((temp = findMacro(name)) != null) { macrolist.Remove(temp); } }
        public string list()
        { string temp="";
            macrolist.ForEach(delegate(Macro p)
            { temp =temp + p.ToString()+ "\n"; });
            return temp;
        }
        public String[] listX()
        {   String[] temp = new String[macrolist.Count];
            int i = 0;
            macrolist.ForEach(delegate(Macro p)
            { temp[i] = p.getTitle(); i++; });
            return temp;
            
        }

        

    }
}


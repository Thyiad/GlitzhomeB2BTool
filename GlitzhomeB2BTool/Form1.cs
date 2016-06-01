using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace GlitzhomeB2BTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<string> keys = new List<string>();

        private List<string> enKeys = new List<string>();
        private List<string> zhKeys = new List<string>();

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            keys.Clear();

            var txt = txtFile.Text.Trim();
            if (string.IsNullOrEmpty(txt)) return;

            var files =  Regex.Split(txt,Environment.NewLine);
            foreach (var file in files)
            {
                if (Directory.Exists(file))
                {
                    foreach (var item in Directory.GetFiles(file))
                    {
                        AnalyzeFile(item);
                    }
                }
                else if (File.Exists(file))
                {
                    AnalyzeFile(file);
                }
            }

            Out();
        }

        private Regex regYiitapp = new Regex(@"Yii::t\('app'\s*,\s*'([^']+)'\)");
        private Regex regModelGetuimsg = new Regex(@"[A-z]+::getUIMsg\('([^']+)'\)");
        private void AnalyzeFile(string file)
        {
            foreach (var line in File.ReadAllLines(file))
            {
                if (regYiitapp.IsMatch(line))
                {
                    MatchCollection matches = regYiitapp.Matches(line);
                    foreach (Match match in matches)
                    {
                        var key = match.Groups[1].Value;

                        if (!keys.Contains(key)) keys.Add(key);
                    }
                }

                if (regModelGetuimsg.IsMatch(line))
                {
                    MatchCollection matches = regModelGetuimsg.Matches(line);
                    foreach (Match match in matches)
                    {
                        var key = match.Groups[1].Value;

                        if (!keys.Contains(key)) keys.Add(key);
                    }
                }
            }
        }

        private void Out()
        {
            InitExistKeys();

            List<string> existKeys = ckZH.Checked ? zhKeys : enKeys;

            txtOut.Text = "";
            StringBuilder builder = new StringBuilder();
            foreach (var item in keys)
            {
                if(!existKeys.Contains(item))
                    builder.AppendLine(string.Format("'{0}' => '',", item));
            }
            txtOut.Text = builder.ToString();
        }

        private void InitExistKeys()
        {
            var en = txtMsg.Text.Trim() + "/en/app.php";
            var zh = txtMsg.Text.Trim() + "/zh-CN/app.php";

            Regex regex = new Regex(@"'(?'key'[^']+)'\s*=>\s*'[^']*',");
            foreach (var line in File.ReadAllLines(en))
            {
                if (regex.IsMatch(line))
                {
                    var match = regex.Match(line);
                    var key = match.Groups["key"].Value;

                    if (!enKeys.Contains(key)) enKeys.Add(key);
                }
            }
            foreach (var line in File.ReadAllLines(zh))
            {
                if (regex.IsMatch(line))
                {
                    var match = regex.Match(line);
                    var key = match.Groups["key"].Value;

                    if (!zhKeys.Contains(key)) zhKeys.Add(key);
                }
            }
        }

        private List<string> labels = new List<string>();
        private void label2model_Click(object sender, EventArgs e)
        {
            var txt = txtFile.Text.Trim();
            if (string.IsNullOrEmpty(txt)) return;

            var files = Regex.Split(txt, Environment.NewLine);
            foreach (var file in files)
            {
                if (Directory.Exists(file))
                {
                    foreach (var item in Directory.GetFiles(file))
                    {
                        AnalyzeLabel(item);
                    }
                }
                else if (File.Exists(file))
                {
                    AnalyzeLabel(file);
                }
            }

            // out label
            txtOut.Text = "";
            StringBuilder builder = new StringBuilder();
            foreach (var item in labels)
            {
                builder.AppendLine(string.Format("'{0}'=>Yii::t('app','{0}'),",item));
            }
            txtOut.Text = builder.ToString();
        }

        private Regex regeLabel = new Regex("label:\\s*(?:'([^']+)'|\"([^ \"]+)\")\\s*,");
        private void AnalyzeLabel(string file)
        {
            foreach (var line in File.ReadAllLines(file))
            {
                if (regeLabel.IsMatch(line))
                {
                    string key = regeLabel.Match(line).Groups[1].Value;
                    if (!labels.Contains(key)) labels.Add(key);
                }
            }
        }

        private void ckZH_CheckedChanged(object sender, EventArgs e)
        {
            Out();
        }
    }
}

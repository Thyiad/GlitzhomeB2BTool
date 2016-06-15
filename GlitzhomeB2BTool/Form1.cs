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

        private List<string> GetFiles()
        {
            List<string> files = new List<string>();
            var txt = txtFile.Text.Trim();
            files = Regex.Split(txt, Environment.NewLine).ToList();

            return files;
        }

        #region 多语言
        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            keys.Clear();

            var files = GetFiles();
            if (files.Count == 0) return;

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
                if (!existKeys.Contains(item))
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
        private void ckZH_CheckedChanged(object sender, EventArgs e)
        {
            Out();
        }
        #endregion

        #region label转ui Msglabel = 'xxx' 转到uiMsg方法中的'xxx'=>Yii::t('app','xxx')
        private List<string> labels = new List<string>();
        private void label2model_Click(object sender, EventArgs e)
        {
            var files = GetFiles();
            if (files.Count == 0) return;
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
                builder.AppendLine(string.Format("'{0}'=>Yii::t('app','{0}'),", item));
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
        #endregion


        #region attributes转换成formData
        private void btnAttr2formData_Click(object sender, EventArgs e)
        {
            attrs.Clear();
            if (ckInnerText.Checked)
            {
                foreach (var line in Regex.Split(txtFile.Text, Environment.NewLine))
                {
                    if (regexAttr.IsMatch(line))
                    {
                        string key = regexAttr.Match(line).Groups[1].Value;
                        if (!attrs.Contains(key)) attrs.Add(key);
                    }
                }
            }
            else
            {
                var files = GetFiles();
                if (files.Count == 0) return;
                foreach (var file in files)
                {
                    if (Directory.Exists(file))
                    {
                        foreach (var item in Directory.GetFiles(file))
                        {
                            AnalyzeAttribute(item);
                        }
                    }
                    else if (File.Exists(file))
                    {
                        AnalyzeAttribute(file);
                    }
                }
            }
            
            txtOut.Text = "";
            StringBuilder builder = new StringBuilder();
            foreach (var item in attrs)
            {
                builder.AppendLine("[");
                builder.AppendLine(string.Format("'label' => Yii::t('app','{0}'),", item));
                builder.AppendLine(string.Format("'name' => '{0}',", item));
                builder.AppendLine("'type' => 'text',");
                builder.AppendLine("'required' => false,");
                builder.AppendLine("'disabled' => $type==='view'?true:false,");
                builder.AppendLine("// 'format' => 'digits, number, email, url',");
                builder.AppendLine("// 'trim' => false,");
                builder.AppendLine("// 'minlength' => 1,");
                builder.AppendLine("// 'maxlength' => 10,");
                builder.AppendLine("// 'regex' => '/pattern/',");
                builder.AppendLine("// 'unique' => 'controller/action',");
                builder.AppendLine("// 'reject' => false,");
                builder.AppendLine("],");
            }
            txtOut.Text = builder.ToString();
        }

        private Regex regexAttr = new Regex("(?:'|\")([^ '\"]+)(?:'|\"),");
        private List<string> attrs = new List<string>();
        private void AnalyzeAttribute(string file)
        {
            bool beginMethod = false;
            foreach (var line in File.ReadAllLines(file))
            {
                if (Regex.IsMatch(line, @"public\s+function\s+attributes\(\)"))
                {
                    beginMethod = true;
                    continue;
                }
                else if (beginMethod && line.Trim() == "}")
                {
                    break;
                }
                else if (beginMethod && regexAttr.IsMatch(line))
                {
                    var match = regexAttr.Match(line);

                    while (match.Success)
                    {
                        string key = match.Groups[1].Value;
                        if (!attrs.Contains(key)) attrs.Add(key);
                        match = match.NextMatch();
                    }
                }
            }
        }
        #endregion

        private void txtFile_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.A && e.Control)
            {
                (sender as TextBox).SelectAll();
            }
        }
    }
}

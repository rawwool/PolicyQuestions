using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Questions.Model;

namespace PolicyQuestionsWF
{
    public partial class Form1 : Form
    {
        const string QUESTIONS_FILE = "questions.json";
        public Form1()
        {
            InitializeComponent();
            this.Text = GetCaption("");
            tableLayoutPanel2.Hide();
            splitContainer1.Hide();

            if (Questions.Utility.Questions.DeserialiseQuestions(QUESTIONS_FILE))
            {
                LoadQuestions("About you");
                PolulateTabs();
                this.Text = GetCaption("Cached Reference Data");
            }
        }

        private void PolulateTabs()
        {
            var categories = Questions.Utility.Questions.GetCategtories();
            flowLayoutPanel2.Controls.Clear();
            foreach(var category in categories)
            {
                AddButton(category, Label_Click);
            }
            AddButton("Requests", ShowJson);
        }

        private void AddButton(string category, EventHandler handler)
        {
            Button button = new Button();
            //label.BackColor = Color.FromArgb(216, 25, 54);
            button.Margin = new Padding(3);
            button.AutoSize = true;
            button.Text = category;
            button.Size = new Size(160, 80);
            button.Click += handler;
            flowLayoutPanel2.Controls.Add(button);
        }

        private void Label_Click(object sender, EventArgs e)
        {
            LoadQuestions((sender as Button).Text);
        }

        private void ShowJson(object sender, EventArgs e)
        {
            try
            {
                var response = Questions.Utility.Questions.GetResponseJSON();
                this.SuspendLayout();
                string viewerName = "JSONViewer";
                label1.Text = "Request bodies";

                this.flowLayoutPanel1.Controls.Clear();
                response.ToList().ForEach(s =>
                {
                    LinkLabel linkLabel = new LinkLabel();
                    linkLabel.AutoSize = true;
                    linkLabel.Text = s.RelativeURL;
                    linkLabel.Tag = s;
                    linkLabel.Click += LinkLabel_Click;
                    this.flowLayoutPanel1.Controls.Add(linkLabel);
                    TextBox textBox = new TextBox();
                    textBox.Multiline = true;
                    textBox.WordWrap = false;
                    textBox.ScrollBars = ScrollBars.Both;
                    textBox.Text = s.JSONBody;
                    //textBox.Dock = DockStyle.Fill;    
                    textBox.WordWrap = true;
                    textBox.ScrollBars = ScrollBars.None;
                    textBox.BorderStyle = BorderStyle.None;
                    //textBox.BackColor = 
                    textBox.Width = this.flowLayoutPanel1.Width 
                    - 2*this.flowLayoutPanel1.Padding.Left
                    - 2*this.flowLayoutPanel1.Margin.Left
                    - 2*textBox.Margin.Left;
                    Label label = new Label();
                    label.Width = textBox.Width;
                    label.AutoSize = true;
                    label.Font = this.flowLayoutPanel1.Font;
                    label.Text = s.JSONBody;
                    this.flowLayoutPanel1.Controls.Add(label);
                    //label.Refresh();
                    textBox.Height = label.Height + 2 * textBox.Margin.Top;
                    textBox.Left = this.flowLayoutPanel1.Margin.Left;
                    textBox.Top = this.flowLayoutPanel1.Margin.Top;
                    textBox.Name = viewerName;
                    textBox.ReadOnly = true;
                    textBox.BackColor = SystemColors.ControlLightLight;
                    this.flowLayoutPanel1.Controls.Add(textBox);
                    this.flowLayoutPanel1.Controls.Remove(label);
                });
                /*
                var viewer = this.tableLayoutPanel2.Controls.Find(viewerName, true).FirstOrDefault() as TextBox;

                if (viewer == null)
                {
                    //this.flowLayoutPanel1.Controls.Clear();
                    TextBox textBox = new TextBox();
                    textBox.Multiline = true;
                    textBox.WordWrap = false;
                    textBox.ScrollBars = ScrollBars.Both;
                    textBox.Text = json;
                    textBox.Dock = DockStyle.Fill;
                    textBox.Name = viewerName;
                    textBox.ReadOnly = true;
                    tableLayoutPanel2.Controls.Add(textBox);
                }
                else
                {
                    viewer.Text = json;
                }
                this.flowLayoutPanel1.Hide();
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LinkLabel_Click(object sender, EventArgs e)
        {
            if (sender is LinkLabel && (sender as LinkLabel).Tag is Questions.Utility.APIRequest)
            {
                //MessageBox.Show(((sender as LinkLabel).Tag as Questions.Utility.APIRequest).JSONBody);
                ProcessRequest((sender as LinkLabel).Text, ((sender as LinkLabel).Tag as Questions.Utility.APIRequest).JSONBody);
            }
        }

        private void ProcessRequest(string resource, string jSONBody)
        {
            var result = Questions.Web.Service.TestExecutePostRequest("https://hiscox-uk-dev.apigee.net/misdev/", resource, jSONBody);
            MessageBox.Show(result.Message, result.StstusCode.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadQuestions(string group)
        {
            this.SuspendLayout();
            tableLayoutPanel2.Show();
            splitContainer1.Show();
            this.flowLayoutPanel1.Hide(); 
            this.flowLayoutPanel1.Controls.Clear();

            var questions = Questions.Utility.Questions.GetQuestion(group)
               .ToList();
            if (questions.FirstOrDefault() != null) label1.Text = questions.FirstOrDefault().Category; else label1.Text = string.Empty;
            questions.ForEach(s =>
               {
                   QuestionControl control = new QuestionControl();
                   control.SetQuestion(s);
                   this.flowLayoutPanel1.Controls.Add(control);
                   
               });
            questions.ForEach(s =>
            {
                
                s.Children.ForEach(q =>
                {
                    if (q.ShowHide != null)
                    {
                        q.ShowHide.Invoke(q.InvokeThisQuestion());
                    }
                    q.LogicalChildren.ForEach(r =>
                    {
                        if (r.ShowHide != null)
                        {
                            r.ShowHide.Invoke(r.InvokeThisQuestion());
                        }
                    });

                });
                s.LogicalChildren.ForEach(q =>
                {
                    if (q.ShowHide != null)
                    {
                        q.ShowHide.Invoke(q.InvokeThisQuestion());
                    }
                });
                //if (s.ShowHide != null)
                //{
                //    s.ShowHide.Invoke(s.InvokeThisQuestion());
                //}
            });

            /*
            foreach(var control in this.flowLayoutPanel1.Controls)
            {
                if (control is QuestionControl) (control as QuestionControl).SetHeight();
                if (control is QuestionControl) (control as QuestionControl).Refresh();
            }

            questions.ForEach(s =>
            {
                if (s.ShowHide != null)
                {
                    s.ShowHide.Invoke(s.InvokeThisQuestion());
                }
            });*/
            flowLayoutPanel1.Show();
            this.ResumeLayout();
            this.Refresh();
        }

        private void aboutHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadQuestions("About your home");    
        }

        private void aboutYourCoverToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LoadQuestions("About your cover");
        }

        private void importantQuestionsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LoadQuestions("Important questions");
        }

        private void aboutYouToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LoadQuestions("About you");
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Excel files|*.xlsx";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        Questions.Utility.Questions.LoadQuestions(Path.Combine(Environment.CurrentDirectory, /*"Questions.xlsx"*/ofd.FileName), new string[]
                        { "About you", "Important questions","About your home", "About your cover"});

                        LoadQuestions("About you");
                        PolulateTabs();
                        Questions.Utility.Questions.SerilaiseQuestions(QUESTIONS_FILE);
                        this.Text = GetCaption(ofd.FileName);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error in reading", MessageBoxButtons.OK,  MessageBoxIcon.Error);
            }
        }

        private string GetCaption(string fileName)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            string caption = $"Prashn v{version.Major}.{version.Minor}";
            if (!string.IsNullOrEmpty(fileName)) caption +=  $" - {Path.GetFileNameWithoutExtension(fileName)}";
            return caption;
        }

        private void exportReferenceDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var data =  Questions.Utility.Questions.GetReferenceData().Select(s=>s.ToString()).Aggregate((a,b) => $"{a}\n\n{b}");
            Clipboard.SetText(data);

        }
    }
}

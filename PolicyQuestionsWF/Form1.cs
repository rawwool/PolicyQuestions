using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Questions.Model;

namespace PolicyQuestionsWF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
           

        private void PolulateTabs()
        {
            var categories = Questions.Utility.Questions.GetCategtories();
            flowLayoutPanel2.Controls.Clear();
            foreach(var category in categories)
            {
                Button label = new Button();
                label.Margin = new Padding(3);
                label.AutoSize = true;
                label.Text = category;
                label.Size = new Size(160, 80);
                label.Click += Label_Click;
                flowLayoutPanel2.Controls.Add(label);
            }
        }

        private void Label_Click(object sender, EventArgs e)
        {
            LoadQuestions((sender as Button).Text);
        }

        private void LoadQuestions(string group)
        {
            this.SuspendLayout();
            this.flowLayoutPanel1.Controls.Clear();
            var questions = Questions.Utility.Questions.GetQuestion(group)
               .ToList();
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
                });
                s.LogicalChildren.ForEach(q =>
                {
                    if (q.ShowHide != null)
                    {
                        q.ShowHide.Invoke(q.InvokeThisQuestion());
                    }
                });
            });

            foreach(var control in this.flowLayoutPanel1.Controls)
            {
                if (control is QuestionControl) (control as QuestionControl).SetHeight();
                if (control is QuestionControl) (control as QuestionControl).Refresh();
            }

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
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error in reading", MessageBoxButtons.OK,  MessageBoxIcon.Error);
            }
        }
    }
}

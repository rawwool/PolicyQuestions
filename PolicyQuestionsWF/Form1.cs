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
           

        private void LoadQuestions(string group)
        {
            this.SuspendLayout();
            this.flowLayoutPanel1.Controls.Clear();
            Questions.Utility.Questions.GetQuestion(group)
               .ToList()
               .ForEach(s =>
               {
                   QuestionControl control = new QuestionControl();
                   control.SetQuestion(s);
                   this.flowLayoutPanel1.Controls.Add(control);
               });
            this.ResumeLayout();
            this.Refresh();
        }

        private void aboutHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadQuestions("1c About your home");    
        }

        private void aboutYourCoverToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LoadQuestions("1d About your cover ");
        }

        private void importantQuestionsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LoadQuestions("1b Important questions ");
        }

        private void aboutYouToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LoadQuestions("1a About you");
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel files|*.xlsx";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Questions.Utility.Questions.LoadQuestions(Path.Combine(Environment.CurrentDirectory, /*"Questions.xlsx"*/ofd.FileName), new string[]
                    { "About you"/*, "Important questions ", "About your home", "About your cover"*/});
                    
                    LoadQuestions("About you");
                }
            }
        }
    }
}

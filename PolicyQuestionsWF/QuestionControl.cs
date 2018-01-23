﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Questions.Model;

namespace PolicyQuestionsWF
{
    public partial class QuestionControl : UserControl
    {
        Questions.Model.Question _Question;
        public QuestionControl()
        {
            InitializeComponent();
            //textBoxHelp.TextChanged += TextBoxHelp_TextChanged;
        }

        private void TextBoxHelp_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox1 = sender as TextBox;
            Size sz = new Size(textBox1.ClientSize.Width, int.MaxValue);
            TextFormatFlags flags = TextFormatFlags.WordBreak;
            int padding = 3;
            int borders = textBox1.Height - textBox1.ClientSize.Height;
            sz = TextRenderer.MeasureText(textBox1.Text, textBox1.Font, sz, flags);
            int h = sz.Height + borders + padding;
            //if (textBox1.Top + h > this.ClientSize.Height - 10)
            //{
            //    h = this.ClientSize.Height - 10 - textBox1.Top;
            //}
            textBox1.Height = h;
        }

        public void SetQuestion(Question question)
        {
            this.SuspendLayout();
            _Question = question;
            this.labelReference.Text = question.Ref;
            this.labelReference.Tag = question.HelpText;
            this.labelQuestion.Text = question.Text;
            //this.labelHelp.Text = question.HelpText;
            this.panelResponse.Controls.Clear();
            ResponsePicker responsePicker = new ResponsePicker();
            responsePicker.SetResponseDataCaptureType(question.DataCaptureType, question.ResponseChoices);
            this.panelResponse.Controls.Add(responsePicker);
            this.Dock = DockStyle.Fill;
            //this.Height = flowLayoutPanel1.DisplayRectangle.Height + 20 + this.Padding.Bottom + this.Padding.Top + this.Margin.Top + this.Margin.Bottom;
            this.ResumeLayout();
            this.Refresh();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelReference_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show((sender as LinkLabel).Tag as string, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

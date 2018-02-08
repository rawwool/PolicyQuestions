using System;
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

        public QuestionControl(int maxWidth, int minWidth):this()
        {
            this.MaximumSize = new Size(maxWidth, 0);
            this.MinimumSize = new Size(minWidth, 0);
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

        public Action<string> Changed;

        public void ResponseChanged(string value)
        {
            if (Changed != null)
            {
                Changed.Invoke(value);
            }
            _Question.UserResponse = value.Trim();
            
            _Question.Children.ForEach(s =>
            {
                bool invoke = s.InvokeThisQuestion();
                if (s.ShowHide != null)
                {
                    s.ShowHide.Invoke(invoke);
                }
                s.Children.ForEach(c =>
                {
                    if (c.ShowHide != null)
                    {
                        c.ShowHide.Invoke(invoke && c.InvokeThisQuestion());
                    }
                });
                s.LogicalChildren.ForEach(c =>
                {
                    if (c.ShowHide != null)
                    {
                        c.ShowHide.Invoke(invoke && c.InvokeThisQuestion());
                    }
                });
            });
            _Question.LogicalChildren.ForEach(s =>
            {
                bool invoke = s.InvokeThisQuestion();
                if (s.ShowHide != null)
                {
                    s.ShowHide.Invoke(invoke);
                }
                s.LogicalChildren.ForEach(c =>
                {
                    if (c.ShowHide != null)
                    {
                        c.ShowHide.Invoke(invoke && c.InvokeThisQuestion());
                    }
                });
            });

            this.SetHeight();
            // If this control is the parent of a logical child, then the child may physically belong to
            // a common parent. Therefore the parent needs to recalculate the height too to cover this scenario.
            if (this.Parent != null && this.Parent.Parent != null && this.Parent.Parent.Parent != null && this.Parent.Parent.Parent is QuestionControl)
            {
                (this.Parent.Parent.Parent as QuestionControl).SetHeight();
            }

            this.Invalidate();
            this.Update();
            this.Refresh();
            this.Parent?.Refresh();
        }

        void ShowHideQuestion(bool show)
        {
            if (show)
            {
                //this.Show();
                this.Margin = new Padding(4);
                this.SetHeight();
               
            }
            else
            {
                //this.Hide();
                this.Height = 0;
                this.Margin = new Padding(0);
            }
            //ShowHideLogicalChildren();
            //SetHeight();
            
        }

        private void ShowHideLogicalChildren()
        {
            if (_Question != null)
            {
                _Question.LogicalChildren.ForEach(s =>
                    s.ShowHide(s.InvokeThisQuestion())
                    );
            }
        }

        public void SetQuestion(Question question)
        {
            this.SuspendLayout();
            _Question = question;
            question.ShowHide = this.ShowHideQuestion;//  (Show) => { if (Show) this.Show(); else this.Hide(); }; 
            this.labelReference.Text = question.Ref;
            this.labelReference.Tag = question.HelpText + Environment.NewLine + question.Expressions;
            this.labelQuestion.Text = question.Text;
            //this.labelHelp.Text = question.HelpText;
            this.panelResponse.Controls.Clear();
            ResponsePicker responsePicker = new ResponsePicker();
            responsePicker.SetResponseDataCaptureType(question.DataCaptureType, question.ResponseChoices, question.UserResponse);
            responsePicker.Changed = ResponseChanged;
            this.panelResponse.Controls.Add(responsePicker);
            question.Children.ForEach(s =>
            {
                QuestionControl control = new QuestionControl(550, 500);
                control.SetQuestion(s);
                this.flowLayoutPanel2.Controls.Add(control);
                s.ShowHide(s.InvokeThisQuestion());
            });
            //this.Dock = DockStyle.Fill;

            SetHeight();
            this.ResumeLayout();
            this.Refresh();
        }

        public void SetHeight()
        {
            //int height = flowLayoutPanel1.DisplayRectangle.Height + this.Padding.Bottom + this.Padding.Top + this.Margin.Top + this.Margin.Bottom;
            // reduce the height of the hidden child questions
            //foreach (Control control in flowLayoutPanel2.Controls)
            //{
            //    if (control is QuestionControl && control.Visible == false)
            //    {
            //        height -= control.DisplayRectangle.Height;
            //    }
            //}
            int height = this.flowLayoutPanel2.Top;
            int innerHeight = 0;
            foreach (Control control in flowLayoutPanel2.Controls)
            {
                if (control is QuestionControl && control.Visible)
                {
                    if (control.DisplayRectangle.Height > 0)
                    {
                        innerHeight += control.DisplayRectangle.Height + control.Padding.Size.Height * 2 + control.Margin.Size.Height * 2;
                    }
                }
            }

            this.Height = height + innerHeight;
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

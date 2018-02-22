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
using static Questions.Model.Question;
using Questions.Utility;

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

        public Action<ResponseChoice> Changed;

        public void ResponseChanged(ResponseChoice value)
        {
            if (Changed != null)
            {
                Changed.Invoke(value);
            }
            _Question.UserResponse = value;
            
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
                    Question q = Questions.Utility.Questions.GetQuestion(c);
                    if (q != null && q.ShowHide != null)
                    {
                        q.ShowHide.Invoke(q.InvokeThisQuestion());
                    }
                });
            });
            _Question.LogicalChildren.ForEach(s =>
            {
                Question q = Questions.Utility.Questions.GetQuestion(s);
                bool invoke = q.InvokeThisQuestion();
                if (q.ShowHide != null)
                {
                    q.ShowHide.Invoke(invoke);
                }
                q.LogicalChildren.ForEach(c =>
                {
                    Question r = Questions.Utility.Questions.GetQuestion(c);

                    if (r.ShowHide != null)
                    {
                        r.ShowHide.Invoke(invoke && r.InvokeThisQuestion());
                    }
                });
            });

            this.SetHeight();
            // If this control is the parent of a logical child, then the child may physically belong to
            // a common parent. Therefore the parent needs to recalculate the height too to cover this scenario.
            if (this.Parent != null 
                && this.Parent.Parent != null
                && this.Parent.Parent.Parent != null
                && this.Parent.Parent.Parent.Parent != null
                && this.Parent.Parent.Parent.Parent is QuestionControl)
            {
                (this.Parent.Parent.Parent.Parent as QuestionControl).SetHeight();
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
                {
                    Question q = Questions.Utility.Questions.GetQuestion(s);
                    q.ShowHide(q.InvokeThisQuestion());
                }
                    );
            }
        }

        public void SetQuestion(Question question)
        {
            this.SuspendLayout();
            _Question = question;
            question.ShowHide = this.ShowHideQuestion;//  (Show) => { if (Show) this.Show(); else this.Hide(); }; 
            this.labelReference.Text = question.Ref;
            this.labelReference.Tag = question.ToString() + Environment.NewLine + "Expression:" + question.Expressions;
            this.labelQuestion.Text = question.Text;
            //this.labelHelp.Text = question.HelpText;
            this.panelResponse.Controls.Clear();
            ResponsePicker responsePicker = new ResponsePicker();
            responsePicker.Changed = ResponseChanged;

            responsePicker.SetResponseDataCaptureType(question.DataCaptureType, question.ResponseChoices, question.UserResponse);
            this.panelResponse.Controls.Add(responsePicker);
            if (question.HasArrayOfChildren)
            {
                Button button = new Button();
                button.Text = "Add";
                button.Width = 100;
                button.Height = 25;
                button.Click += Button_Click;
                this.flowLayoutPanel1.Controls.Add(button);
                this.flowLayoutPanel1.Controls.SetChildIndex(button, 3);
            }

            AddChildQuestions(question, false);
            
            SetHeight();
            this.ResumeLayout();
            this.Refresh();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();
            AddChildQuestions(_Question, true);
            SetHeight();
            this.ResumeLayout();
            this.Refresh();
        }

        private void AddChildQuestions(Question question, bool additional)
        {
            //Add this to the question's children collection if needed:
            if (question.HasArrayOfChildren && additional/*question.Children.Count() == this.flowLayoutPanel2.Controls.Count * distinctChildren.Count()*/ )
            {
                var distinctChildren = question.Children.GroupBy(s => s.Ref).Select(s => s.First()).ToList();
                if (question.Children.Count() == question.ChildSetCount * distinctChildren.Count)
                {
                    // We now have to add a new set of child questions
                    var questionsToRender = distinctChildren.CloneList<Question>();
                    questionsToRender.ForEach(q => q.ChildSetCount = 0);
                    question.Children.AddRange(questionsToRender);
                }
                question.ChildSetCount++;
            }

            if (!question.HasArrayOfChildren || question.ChildSetCount > 0)
            {
                this.flowLayoutPanel2.Controls.Clear();
                //question.Children.ForEach(s =>
                int setCount = question.Children.GroupBy(s => s.Text).Count();
                int questionCount = question.Children.Count();
                for (int i = 0; i < questionCount; i += setCount)
                {
                    FlowLayoutPanel childQuestionsPanel = new FlowLayoutPanel();
                    childQuestionsPanel.AutoScroll = true;
                    childQuestionsPanel.AutoSize = true;
                    childQuestionsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
                    childQuestionsPanel.BackColor = System.Drawing.SystemColors.ControlLight;
                    childQuestionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                    childQuestionsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
                    //childQuestionsPanel.Location = new System.Drawing.Point(27, 49);
                    childQuestionsPanel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
                    //childQuestionsPanel.Name = "flowLayoutPanel3";
                    childQuestionsPanel.Size = new System.Drawing.Size(42, 0);
                    //childQuestionsPanel.TabIndex = 5;
                    childQuestionsPanel.WrapContents = false;

                    question.Children.Skip(i).Take(setCount).ToList().ForEach(s =>
                    {
                        QuestionControl control = new QuestionControl(550, 500);
                        control.SetQuestion(s);
                    //this.flowLayoutPanel2.Controls.Add(control);
                    childQuestionsPanel.Controls.Add(control);
                        s.ShowHide(s.InvokeThisQuestion());
                    });

                    this.flowLayoutPanel2.Controls.Add(childQuestionsPanel);
                };
            }
        }

        public void SetHeight()
        {
            int height = this.flowLayoutPanel2.Top;
            
            int innerHeight = 0;
            GetInnerHeight(ref innerHeight);
            
            this.Height = height + innerHeight;
        }

        private void GetInnerHeight(ref int height)
        {
            int innerHeight = 0;
            foreach (Control childcontrol in flowLayoutPanel2.Controls)
            {
                if (childcontrol is FlowLayoutPanel)
                {
                    foreach (Control control in childcontrol.Controls)
                    {
                        if (control is QuestionControl && control.Visible)
                        {
                            if (control.DisplayRectangle.Height > 0)
                            {
                                innerHeight += control.DisplayRectangle.Height + /*control.Padding.Size.Height * 2 +*/ control.Margin.Size.Height * 2;
                            }
                        }
                    }
                    innerHeight += (childcontrol.Margin.Top + childcontrol.Margin.Bottom);
                }
            }

            height += innerHeight;
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

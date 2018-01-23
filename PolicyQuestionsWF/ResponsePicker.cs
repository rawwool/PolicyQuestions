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
    public partial class ResponsePicker : UserControl
    {
        public ResponsePicker()
        {
            InitializeComponent();
        }

        public void SetResponseDataCaptureType(enumDataCaptureType type, IEnumerable<string> possibleResponses)
        {
            this.AutoSize = true;
            switch(type)
            {
                case enumDataCaptureType.DateTimePicker:
                    DateTimePicker picker = new DateTimePicker();
                    this.flowLayoutPanel1.Controls.Add(picker);
                    break;
                case enumDataCaptureType.TextBox:
                    var textBox = new TextBox();
                    textBox.Multiline = true;
                    
                    this.flowLayoutPanel1.Controls.Add(textBox);
                    break;
                case enumDataCaptureType.DropDown:
                    var combo = new ComboBox();
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.DataSource = possibleResponses;
                    this.flowLayoutPanel1.Controls.Add(combo);
                    break;
                case enumDataCaptureType.RadioButton:
                    //FlowLayoutPanel panel = new FlowLayoutPanel();
                    //panel.Dock = DockStyle.Fill;
                    //panel.AutoSize = true;
                    //this.flowLayoutPanel1.Controls.Add(panel);
                    possibleResponses.ToList().ForEach(s =>
                    {
                        RadioButton rb = new RadioButton();
                        rb.AutoSize = true;
                        rb.BackColor = Color.White;
                        rb.Margin = new Padding(2);
                        //rb.BackColor = Color.Red;
                        rb.Text = s;
                        this.flowLayoutPanel1.Controls.Add(rb);
                    });
                    break;
                case enumDataCaptureType.TextBoxAndNotSureCheckBox:
                    var textBox2 = new TextBox();
                    textBox2.Multiline = true;

                    this.flowLayoutPanel1.Controls.Add(textBox2);
                    CheckBox checkBox = new CheckBox();
                    checkBox.Text = "Not sure";
                    this.flowLayoutPanel1.Controls.Add(checkBox);

                    break;
            }
        }
    }
}

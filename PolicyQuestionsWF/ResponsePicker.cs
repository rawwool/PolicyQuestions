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

        public Action<string> Changed;

        public void SetResponseDataCaptureType(enumDataCaptureType type, IEnumerable<string> possibleResponses, string userResponse)
        {
            this.AutoSize = true;
            switch(type)
            {
                case enumDataCaptureType.DateTimePicker:
                    DateTimePicker picker = new DateTimePicker();
                    if (userResponse != null) picker.Value = DateTime.Parse(userResponse);
                    picker.ValueChanged += Picker_ValueChanged;
                    this.flowLayoutPanel1.Controls.Add(picker);
                    break;
                case enumDataCaptureType.TextBox:
                    var textBox = new TextBox();
                    textBox.Multiline = true;
                    textBox.Width = 350;
                    if (userResponse != null) textBox.Text = userResponse;
                    textBox.TextChanged += TextBox_TextChanged;
                    this.flowLayoutPanel1.Controls.Add(textBox);
                    break;
                case enumDataCaptureType.DropDown:
                    var combo = new ComboBox();
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    combo.DataSource = possibleResponses;
                    if (userResponse != null) combo.SelectedValue = userResponse;
                    combo.SelectedValueChanged += Combo_SelectedValueChanged;
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
                        if (userResponse != null && rb.Text == userResponse) rb.Checked = true;
                        rb.CheckedChanged += Rb_CheckedChanged;
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
                case enumDataCaptureType.NoDataCapture:
                    break;
            }
        }

        private void Rb_CheckedChanged(object sender, EventArgs e)
        {
            if (Changed != null && (sender as RadioButton).Checked)
            {
                Changed.Invoke((sender as RadioButton).Text.ToString());
            }
        }

        private void Combo_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                Changed.Invoke((sender as ComboBox).SelectedValue.ToString());
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                Changed.Invoke((sender as TextBox).Text);
            }
        }

        private void Picker_ValueChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                Changed.Invoke((sender as DateTimePicker).Value.ToString());
            }
        }
    }
}

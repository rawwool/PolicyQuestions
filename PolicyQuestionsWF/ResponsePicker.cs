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

namespace PolicyQuestionsWF
{
    public partial class ResponsePicker : UserControl
    {
        public ResponsePicker()
        {
            InitializeComponent();
        }

        public Action<ResponseChoice> Changed;

        public void SetResponseDataCaptureType(enumDataCaptureType type, IEnumerable<ResponseChoice> possibleResponses, ResponseChoice userResponse)
        {
            this.AutoSize = true;
            switch(type)
            {
                case enumDataCaptureType.DateTimePicker:
                    DateTimePicker picker = new DateTimePicker();
                    picker.Format = DateTimePickerFormat.Custom;
                    picker.CustomFormat = "yyyy-MM-dd";
                    if (userResponse != null && userResponse.Display != null) picker.Value = DateTime.Parse(userResponse.Display);
                    picker.ValueChanged += Picker_ValueChanged;
                    if (userResponse == null) picker.Value = DateTime.Today;
                    
                    this.flowLayoutPanel1.Controls.Add(picker);
                    break;
                case enumDataCaptureType.TextBox:
                    var textBox = new TextBox();
                    //textBox.Multiline = true;
                    textBox.Width = 350;
                    if (userResponse != null) textBox.Text = userResponse.Display;
                    textBox.TextChanged += TextBox_TextChanged;
                    if (userResponse == null) textBox.Text = "";
                    this.flowLayoutPanel1.Controls.Add(textBox);
                    break;
                case enumDataCaptureType.DropDown:
                    var combo = new ComboBox();
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    //combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    combo.Width = 350;
                    combo.DataSource = possibleResponses;
                    combo.DisplayMember = "Display";
                    if (userResponse != null)
                    {
                        combo.SelectedValue = possibleResponses.FirstOrDefault(s => s.Value == userResponse.Value);
                    }
                    else
                    {
                        combo.SelectedValue = possibleResponses.FirstOrDefault();
                    }
                    combo.SelectedValueChanged += Combo_SelectedValueChanged;
                    this.flowLayoutPanel1.Controls.Add(combo);
                    break;
                case enumDataCaptureType.RadioButton:
                    //FlowLayoutPanel panel = new FlowLayoutPanel();
                    //panel.Dock = DockStyle.Fill;
                    //panel.AutoSize = true;
                    //this.flowLayoutPanel1.Controls.Add(panel);
                    bool rbOneInvoked = false;
                    RadioButton rbOne = null;
                    possibleResponses.ToList().ForEach(s =>
                    {
                        RadioButton rb = new RadioButton();
                        rb.AutoSize = true;
                        rb.Margin = new Padding(2);
                        rb.Text = s.Display;
                        rb.Tag = s;
                        if (userResponse != null && rb.Text == userResponse.Display) rb.Checked = true;
                        rb.CheckedChanged += Rb_CheckedChanged;
                        if (userResponse == null)
                            if (rbOneInvoked == false)
                            {
                                rbOne = rb;
                                rbOneInvoked = true;
                            }
                        this.flowLayoutPanel1.Controls.Add(rb);
                    });
                    if (rbOne != null) rbOne.Checked = true;
                    break;
                case enumDataCaptureType.CheckBox:
                    CheckBox checkBoxChoice = new CheckBox();
                    checkBoxChoice.Text = possibleResponses.FirstOrDefault().Display;
                    bool state = false;

                    if (userResponse != null)
                    {
                        bool.TryParse(userResponse.Display, out state);
                        checkBoxChoice.Checked = state;
                    }
                    checkBoxChoice.CheckedChanged += CheckBoxChoice_CheckedChanged;
                    if (userResponse == null)
                    {
                        checkBoxChoice.Checked = true;
                        checkBoxChoice.Checked = false;
                    }
                    this.flowLayoutPanel1.Controls.Add(checkBoxChoice);
                    break;
                case enumDataCaptureType.TextBoxAndNotSureCheckBox:
                    var textBox2 = new TextBox();
                    textBox2.Name = "TextBoxAndNotSureCheckBox_TextBox";
                    //textBox2.Multiline = true;
                    CheckBox checkBox = new CheckBox();
                    checkBox.Text = "Not sure";
                    checkBox.Name = "TextBoxAndNotSureCheckBox_CheckBox";
                    if (userResponse != null)
                    {
                        if (userResponse.Display == checkBox.Text) checkBox.Checked = true;
                        else textBox2.Text = userResponse.Display;
                    }
                    textBox2.TextChanged += TextBoxAndNotSureCheckBo_Changed;
                    this.flowLayoutPanel1.Controls.Add(textBox2);
                    checkBox.CheckedChanged += TextBoxAndNotSureCheckBo_Changed;
                    this.flowLayoutPanel1.Controls.Add(checkBox);

                    break;
                case enumDataCaptureType.NoDataCapture:
                    break;
            }
        }

        private void TextBoxAndNotSureCheckBo_Changed(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                var checkBox = this.flowLayoutPanel1.Controls.Find("TextBoxAndNotSureCheckBox_CheckBox", false).FirstOrDefault();
                var textBox = this.flowLayoutPanel1.Controls.Find("TextBoxAndNotSureCheckBox_TextBox", false).FirstOrDefault();
                if (checkBox != null && checkBox is CheckBox && (checkBox as CheckBox).Checked)
                {
                    Changed.Invoke(new ResponseChoice(checkBox.Text));
                }
                else if (textBox != null && textBox is TextBox)
                {
                    Changed.Invoke(new ResponseChoice(textBox.Text.Trim()));
                }
            }
        }

        private void CheckBoxChoice_CheckedChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                Changed.Invoke((sender as CheckBox).Checked ? new ResponseChoice("true") : new ResponseChoice("false"));
            }
        }

        private void Rb_CheckedChanged(object sender, EventArgs e)
        {
            if (Changed != null && (sender as RadioButton).Checked)
            {
                //Changed.Invoke(new ResponseChoice((sender as RadioButton).Text.ToString()));
                Changed.Invoke(((sender as RadioButton).Tag) as ResponseChoice);
            }
        }

        private void Combo_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                var choice = (sender as ComboBox).SelectedValue as ResponseChoice;
                Changed.Invoke(choice);
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                Changed.Invoke(new ResponseChoice((sender as TextBox).Text));
            }
        }

        private void Picker_ValueChanged(object sender, EventArgs e)
        {
            if (Changed != null)
            {
                Changed.Invoke(new ResponseChoice((sender as DateTimePicker).Value.ToString("yyyy-MM-dd")));
            }
        }
    }
}

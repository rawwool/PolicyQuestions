using System.ComponentModel;

namespace Questions.Model
{
    public enum enumDataCaptureType
    {
        [Description("Radio, Radio button, Button")]
        RadioButton,
        [Description("Drop down, Combo")]
        DropDown,
        [Description("Text box")]
        TextBox,
        [Description("Date, Calendar")]
        DateTimePicker,
        [Description("Text box & not sure button")]
        TextBoxAndNotSureCheckBox
    }
}
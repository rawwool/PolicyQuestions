using System.ComponentModel;

namespace Questions.Model
{
    public enum enumDataCaptureType
    {
        [Description("Radio, Radio button, Button")]
        RadioButton,
        [Description("Drop down, Combo")]
        DropDown,
        [Description("Text box, UK Postcode, house number")]
        TextBox,
        [Description("Date, Calendar")]
        DateTimePicker,
        [Description("Text box & not sure button")]
        TextBoxAndNotSureCheckBox,
        [Description("N/A, NA, Not applicable")]
        NoDataCapture
    }
}
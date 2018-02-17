using System.ComponentModel;

namespace Questions.Model
{
    public enum enumDataCaptureType
    {
        [Description("Radio, Radio button, Button, Slidebar, slide bar")]
        RadioButton,
        [Description("Drop down, Combo")]
        DropDown,
        [Description("Text box, Free Text, UK Postcode, house number, House name")]
        TextBox,
        [Description("Date, Calendar")]
        DateTimePicker,
        [Description("Text box & not sure button")]
        TextBoxAndNotSureCheckBox,
        [Description("Checkbox, check box, boolean")]
        CheckBox,
        //[Description("Slidebar, slide bar")]
        //SlideBar,
        [Description("N/A, NA, Not applicable")]
        NoDataCapture
    }
}
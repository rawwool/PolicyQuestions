using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PolicyQuestionsWF
{
    public partial class LinkLabelEx : LinkLabel
    {
        public LinkLabelEx()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.Selectable, false);
        }
    }
}

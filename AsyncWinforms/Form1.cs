using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FuseeSim;

namespace AsyncWinforms
{
    public partial class Form1 : Form
    {
        FuseeApp fuseeApp;

        public Form1()
        {
            fuseeApp = new FuseeApp();
            InitializeComponent();
            fuseeApp.Init();
        }

        public void OnIdle(object sender, EventArgs args)
        {
            fuseeApp.RenderAFrame();
        }

    }

}

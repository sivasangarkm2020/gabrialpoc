using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TodaysBestExchangeRates
{
    public partial class frmMessageBox : Form
    {
        public frmMessageBox()
        {
            InitializeComponent();
        }
        private void frmMessageBox_Load(object sender, EventArgs e)
        {
            DBHelper dBHelper = new DBHelper();
            List<BestExchangeRate> bestExchangeRates = new List<BestExchangeRate>();
            bestExchangeRates = dBHelper.GetBestExchangeRates();
            if (bestExchangeRates.Count > 0)
            {
                gvBestRates.DataSource = bestExchangeRates;
            }
            else
            {
                gvBestRates.Visible = false;
                lblErrorMessage.Visible = true;
                lblErrorMessage.Text = "Error while loding data.";
            }
            
        }
    }
}

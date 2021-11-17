using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;

namespace BasicWebScraper
{
    public partial class WebScraper : Form
    {
        DataTable dt = new DataTable();

        public WebScraper()
        {
            InitializeComponent();
            RegisterEvents();
        }

        // Methods to Register Events
        private void RegisterEvents()
        {
            this.btnStart.Click += new EventHandler(this.btnStart_Click);
            this.dgvResult.CellClick += new DataGridViewCellEventHandler(this.dgvResult_CellClick);
            Load_cmbMonths();
            this.btnSearch.Click += new EventHandler(this.btnSearch_Click);
        }

        // Method for btnSearch Click Event
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbMonth.SelectedItem.ToString() == " ")
                {
                    dgvResult.DataSource = dt;
                    this.txtCount.Text = dgvResult.Rows.Count.ToString();
                }
                else
                {
                    DataView DView = new DataView();
                    string SelectedMonth = DateTime.ParseExact(this.cmbMonth.SelectedItem.ToString(), "MMMM", null).ToString("MMM");
                    string filter = "[Ex-Dividend Date] LIKE '%" + SelectedMonth + "%'";
                    dt.TableName = "Filter Table";
                    DView.Table = dt;
                    DView.RowFilter = filter;
                    dgvResult.DataSource = (DataTable)DView.ToTable();
                    this.txtCount.Text = dgvResult.Rows.Count.ToString();
                }
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        // Method for dgvResult Cell Click Event
        private void dgvResult_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == dgvResult.Columns["Open_URL"].Index)
                {
                    string URL = dgvResult.Rows[e.RowIndex].Cells["Stock URL"].Value.ToString();
                    if (!string.IsNullOrEmpty(URL))
                        Process.Start(URL);
                }
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        // Method to get Data From Web Page 
        public async Task GetDataFromWebPage()
        {
            try
            {
                string data = String.Empty;

                string url = "https://www.moneycontrol.com/stocks/marketinfo/dividends_declared/index.php";

                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        using (HttpContent content = response.Content)
                        {
                            data = content.ReadAsStringAsync().Result;
                        }
                    }
                }
                PraseHtml(data);
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        // Method to Parse Html from Web Page to Data Grid View
        public void PraseHtml(string htmlData)
        {
            try
            {
                CreateDataTable();
                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(htmlData);

                var table = htmlDocument.DocumentNode.Descendants("table");

                foreach (var row in table.ToList()[2].ChildNodes)
                {
                    if (row.Name == "tr")
                    {
                        List<string> rowdata = new List<string>();
                        string Stock_URL = String.Empty;
                        foreach (var column in row.ChildNodes)
                        {
                            if (column.Name == "td")
                            { 
                                string link_data = column.InnerHtml;
                                HtmlAgilityPack.HtmlDocument link_doc = new HtmlAgilityPack.HtmlDocument();
                                link_doc.LoadHtml(link_data);
                                var link = link_doc.DocumentNode.SelectNodes("//a[@href]");
                                if(link != null)
                                    Stock_URL = "https://www.moneycontrol.com" + link[0].Attributes["href"].Value;
                                rowdata.Add(column.InnerText);
                            }
                        }
                        if (!(rowdata[0].StartsWith("COMPANY") || rowdata[0].StartsWith("Type")))
                        {
                            if(!string.IsNullOrEmpty(Stock_URL))
                            {
                                rowdata.Add(Stock_URL);
                            }
                            dt.Rows.Add(rowdata.ToArray());
                        }
                    }
                }
                dgvResult.DataSource = dt;
                FormatDataGridView();
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        // Method for btnStart Click Event
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                GetDataFromWebPage();
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        // Methos to Create Structure of Data Table
        private void CreateDataTable()
        {
            try
            {
                dt = new DataTable();
                dt.Columns.Add("Company Name");
                dt.Columns.Add("Dividend Type");
                dt.Columns.Add("Dividend %");
                dt.Columns.Add("Announcement Date");
                dt.Columns.Add("Record Date");
                dt.Columns.Add("Ex-Dividend Date");
                dt.Columns.Add("Stock URL");
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        // Method to Format Data Grid View
        private void FormatDataGridView()
        {
            try
            {
                if (!(dgvResult.Columns.Contains("Open_URL") && dgvResult.Columns["Open_URL"].Visible))
                {
                    DataGridViewButtonColumn btnURL = new DataGridViewButtonColumn();
                    btnURL.Name = "Open_URL";
                    btnURL.HeaderText = "Open_URL";
                    btnURL.Text = "Stock Details";
                    btnURL.UseColumnTextForButtonValue = true;
                    btnURL.Width = 50;
                    dgvResult.Columns.Add(btnURL);
                }

                foreach (DataGridViewRow dgvr in dgvResult.Rows)
                {
                    if (!string.IsNullOrEmpty(dgvr.Cells["Announcement Date"].Value.ToString()) && dgvr.Cells["Announcement Date"].Value.ToString() != "-")
                        dgvr.Cells["Announcement Date"].Value = DateTime.ParseExact(dgvr.Cells["Announcement Date"].Value.ToString(), "dd-MM-yyyy", null).ToString("dd MMM yyyy");
                    if (!string.IsNullOrEmpty(dgvr.Cells["Record Date"].Value.ToString()) && dgvr.Cells["Record Date"].Value.ToString() != "-")
                        dgvr.Cells["Record Date"].Value = DateTime.ParseExact(dgvr.Cells["Record Date"].Value.ToString(), "dd-MM-yyyy", null).ToString("dd MMM yyyy");
                    if (!string.IsNullOrEmpty(dgvr.Cells["Ex-Dividend Date"].Value.ToString()) && dgvr.Cells["Ex-Dividend Date"].Value.ToString() != "-")
                        dgvr.Cells["Ex-Dividend Date"].Value = DateTime.ParseExact(dgvr.Cells["Ex-Dividend Date"].Value.ToString(), "dd-MM-yyyy", null).ToString("dd MMM yyyy");
                }

                this.txtCount.Text = dgvResult.Rows.Count.ToString();
                this.cmbMonth.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                throw (ex);
            }
        }

        // Method to Load List in cmbMonths
        private void Load_cmbMonths()
        {
            try
            {
                List<string> MonthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames.Take(12).ToList();
                MonthNames.Insert(0, " ");
                cmbMonth.DataSource = MonthNames;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}

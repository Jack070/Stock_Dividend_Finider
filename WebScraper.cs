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

namespace BasicWebScraper
{
    public partial class WebScraper : Form
    {
        DataTable dt = new DataTable();
        public WebScraper()
        {
            InitializeComponent();
            RegisterEvents();
            CreateDataTable();
        }

        private void RegisterEvents()
        {
            this.btnStart.Click += new EventHandler(this.btnStart_Click);
        }

        public async Task GetDataFromWebPage()
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

            //string fullUrl = "https://coinmarketcap.com/";

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //if(response.StatusCode == HttpStatusCode.OK)
            //{
            //    Stream receiveStream = response.GetResponseStream();
            //    StreamReader readStream = null;
            //    if (String.IsNullOrWhiteSpace(response.CharacterSet))
            //        readStream = new StreamReader(receiveStream);
            //    else
            //        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
            //    data = readStream.ReadToEnd();
            //    response.Close();
            //    readStream.Close();
            //}

            //HttpClient client = new HttpClient();
            //HttpResponseMessage request = await client.GetAsync(fullUrl);
            //String response = await request.Content.ReadAsStringAsync();

            PraseHtml(data);
        }

        public void PraseHtml(string htmlData)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(htmlData);

                var table = htmlDocument.DocumentNode.Descendants("table");

                foreach (var row in table.ToList()[2].ChildNodes)
                {
                    if (row.Name == "tr")
                    {
                        List<string> rowdata = new List<string>();
                        foreach (var column in row.ChildNodes)
                        {
                            if (column.Name == "td")
                            {
                                rowdata.Add(column.InnerText);
                            }
                        }
                        if (!(rowdata[0].StartsWith("COMPANY") || rowdata[0].StartsWith("Type")))
                        {
                            dt.Rows.Add(rowdata.ToArray());
                        }
                    }
                }
                dgvResult.DataSource = dt;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            GetDataFromWebPage();
        }

        private void CreateDataTable()
        {
            dt.Columns.Add("Company Name");
            dt.Columns.Add("Dividend Type");
            dt.Columns.Add("Dividend %");
            dt.Columns.Add("Announcement Date");
            dt.Columns.Add("Record Date");
            dt.Columns.Add("Ex-Dividend Date");
        }
    }
}

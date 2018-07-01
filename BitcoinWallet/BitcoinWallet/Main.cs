using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NBitcoin;
using Newtonsoft.Json.Linq;
using QBitNinja.Client;

namespace BitcoinWallet
{
    public partial class Main : Form
    {
        static Network network;

        static string keyData;
        static string adress;
        static string key;
        static string net;
        static string fileLocation = "keyinfo.key";

        static string balance;

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            log("Bitcon wallet started");
            keyData = File.ReadLines(fileLocation).Skip(0).Take(1).First();
            adress = ExtractString(keyData, "addr");
            key = ExtractString(keyData, "priv");
            net = ExtractString(keyData, "net");
            if (net == "main") network = Network.Main;
            else
            {
                network = Network.TestNet;
                net = "test";
            }

            if(string.IsNullOrEmpty(adress) && !string.IsNullOrEmpty(key))
            {
                var getAdressFromPriv = new BitcoinSecret(key);
                string foundAdress = getAdressFromPriv.GetAddress().ToString();
                adress = foundAdress;
                lineChanger("<addr>" + adress + "</addr><priv>" + key + "</priv><net>" + net + "</net>", fileLocation, 1);
            }
            if((!string.IsNullOrEmpty(adress) && string.IsNullOrEmpty(key)) || (string.IsNullOrEmpty(adress) && string.IsNullOrEmpty(key))){
                MessageBox.Show("No bitcoin address found (or the adress is corrupt), generating adress");
                generateKey();
            }
            int getint;
            if (net == "main") getint = 1;
            else getint = 0;
            string walletInfo = getAddressInfo(adress, getint);
            if (walletInfo != "e")
            {
                var jsonArray = JObject.Parse(walletInfo);
                balance = (string)jsonArray["balance"];
                string deci = balance.Substring(balance.Length - 8, balance.Length - 2);
                balance = balance.Remove(balance.IndexOf(deci));
                balance = balance + "." + deci;
                label1.Text += adress + " Balance: " + balance + " BTC Network: " + net;
            }
            else log("Server error");

        }
        static void generateKey()
        {
            RandomUtils.Random = new UnsecureRandom();

            var privateKey = new Key();
            var bitcoinPrivateKey = privateKey.GetBitcoinSecret(network);
            var newAdress = bitcoinPrivateKey.GetAddress();
            adress = newAdress.ToString();
            key = bitcoinPrivateKey.ToString();
            keyData = "<addr>" + adress + "</addr><priv>" + key + "</priv><net>" + net + "</net>";
            lineChanger(keyData, fileLocation, 1);
        }
        string getAddressInfo(string qadress, int net)
        {
            string html = string.Empty;
            string url;
            if (net == 1) url = "https://api.blockcypher.com/v1/btc/main/addrs/" + qadress + "/balance";
            else url = "https://api.blockcypher.com/v1/btc/test3/addrs/" + qadress + "/balance";

            log("Contacting " + url + "...");
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
                return html;
            }
            catch (Exception)
            {
                return "e";
            }
            
        }
        static string ExtractString(string s, string tag)
        {
            // You should check for errors in real-world code, omitted for brevity
            var startTag = "<" + tag + ">";
            int startIndex = s.IndexOf(startTag) + startTag.Length;
            int endIndex = s.IndexOf("</" + tag + ">", startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }
        static void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit - 1] = newText;
            File.WriteAllLines(fileName, arrLine);
        }
        void log (string info)
        {
            richTextBox1.Text += DateTime.Now.ToString()+ ": " + info + "\n";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int getint;
            if (net == "main") getint = 1;
            else getint = 0;
            string walletInfo = getAddressInfo(adress, getint);
            var jsonArray = JObject.Parse(walletInfo);
            balance = (string)jsonArray["balance"];
            string deci = balance.Substring(balance.Length - 8, balance.Length - 2);
            balance = balance.Remove(balance.IndexOf(deci));
            balance = balance + "." + deci;
            label1.Text = "Adress: " + adress + " Balance: " + balance + " BTC Network: " + net;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}

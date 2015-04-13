using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace DataChecker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());






        }
    }

    //Simple object to hold items in memory.  YAY :(  I think there are better ways if I had been able to use the desearlizer, but that's super broken, so, uh, not sure what to tell you
    public class HotelItem
    {
        public string HotelCode { get; set; }
        public string HotelName { get; set; }
        public string ItemCode { get; set; }
        public string Location { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string StarRating { get; set; }
        public string Category { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryISOCode { get; set; }
        public string CountryName { get; set; }
        public string Map { get; set; }
        public string MapStatus { get; set; }
        public int NumberOfImages { get; set; }

    }


    public class StatusItem
    {
        public string CountryText { get; set; }
        public int CountryValue { get; set; }
        public string CityText { get; set; }
        public int CityValue { get; set; }
        public string HotelText { get; set; }
        public int HotelValue { get; set; }
        public string Status { get; set; }


    }


    //This monstrosity is to make a pretty key/pair for the site links vs the site URLs.  They are located in the app.config file.  So there is that.
    public class ComboboxItem
    {


        public string Text { get; set; }
        public string Value { get; set; }

        public ComboboxItem(string txt, string val)
        {
            this.Text = txt;
            this.Value = val;

        }


        public override string ToString()
        {
            return Text;
        }
    }

}

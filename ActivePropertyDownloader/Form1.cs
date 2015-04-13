using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Xml;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

namespace ActivePropertyDownloader
{
    public partial class Form1 : Form
    {

        //This backgroundworker thing is  to load the properties in the background, fucking threads
        BackgroundWorker bw = new BackgroundWorker();
        
        public Form1()
        {
            InitializeComponent();

            var appSettings = ConfigurationManager.AppSettings;

            vNumber.Text = System.Reflection.Assembly.GetExecutingAssembly()
                                           .GetName()
                                           .Version
                                           .ToString();
            if (appSettings.Count == 0)
            {
                MessageBox.Show("Unable to Load site list");
            }
            else
            {
                foreach (var key in appSettings.AllKeys)
                {
                    SiteList.Items.Add(new ComboboxItem(key, appSettings[key]));
                }
            }

            
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged +=  new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted +=  new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

        }

        private void button1_Click(object sender, EventArgs e)
        {

           




            if (bw.IsBusy != true)
            {

                if(fileSaveLink.Text == "File Save Location")
                {
                    statusText.Text = "ERROR:  Please select A File Save Location";

                    return;

                }


                if (SiteList.SelectedItem == null)
                {
                    statusText.Text = "ERROR:  NO SITE SELECTED";

                    return;

                }
                if (ClientID.Value.ToString()  == "0")
                {
                    statusText.Text = "ERROR:  Invalid Client ID";

                    return;

                }
                if (emailBox.Text == "")
                {
                    statusText.Text = "ERROR:  Please enter an email";

                    return;

                }
                if (passwordBox.Text == "")
                {
                    statusText.Text = "ERROR:  Please enter a password";

                    return;

                }


                //Test Credentials quickly
                var requestItemInformation = new t_Request();
                requestItemInformation.Source = NewHeader(ClientID.Value.ToString(), emailBox.Text, passwordBox.Text);

                var searchiteminfo = new t_SearchItemInformationRequest();

                searchiteminfo.ItemType = t_ItemType.hotel;
                searchiteminfo.ItemTypeSpecified = true;
                searchiteminfo.ItemDestination = new t_ItemDestination();
                searchiteminfo.ItemDestination.DestinationCode = "999";
                searchiteminfo.ItemDestination.DestinationType = t_DestinationType.city;
                searchiteminfo.ItemCode = "999";



                requestItemInformation.RequestDetails.AddItem(ItemsChoiceType.SearchItemInformationRequest, searchiteminfo);



                statusText.Text = "Testing Credentials";
                statusText.Update();

                var request = WebRequest.Create((SiteList.SelectedItem as ComboboxItem).Value);

                request.Method = "POST";

                request.ContentLength = requestItemInformation.Serialize().Length;
                request.ContentType = "text/xml";



                var dataStream = request.GetRequestStream();
                dataStream.Write(System.Text.Encoding.UTF8.GetBytes(requestItemInformation.Serialize()), 0, requestItemInformation.Serialize().Length);
                dataStream.Close();
                try { 
                var response = request.GetResponse();

                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), UTF8Encoding.UTF8))
                {
                    string responseText = reader.ReadToEnd();
                }

                }
                catch (WebException webE) {
                    using (var reader = new System.IO.StreamReader(webE.Response.GetResponseStream(), UTF8Encoding.UTF8))
                    {
                        XmlDocument x1 = new XmlDocument();
                        x1.LoadXml(reader.ReadToEnd());

                        statusText.Text = x1.SelectSingleNode("//ErrorId").InnerText + "\r\n" + x1.SelectSingleNode("//ErrorText").InnerText;
                        return;
                    }


                }



                downloadbutton.Text = "Cancel Load";
                //Lets send the currently values of our setup to the new thread so it doesn't bitch about how I shouldn't cross my streams....
                object[] parameters = new string[] { (SiteList.SelectedItem as ComboboxItem).Value, ClientID.Value.ToString(), emailBox.Text, passwordBox.Text , fileSaveLink.Text};
                bw.RunWorkerAsync(parameters);
            }
            else
            {
                if (bw.WorkerSupportsCancellation == true)
                {
                    downloadbutton.Text = "Attempting to Cancel";
                    bw.CancelAsync();
                }
            }


        }


        //This is the selector thing to say where to save the file.  I mean, it seems to work, so screw it.
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.InitialDirectory = "\\My documents";
            d.Filter = "CSV|*.csv";

            if(DialogResult.OK == d.ShowDialog()) {
            
             fileSaveLink.Text = d.FileName;
                } else {
                    MessageBox.Show("Please choose a valid location");
                    fileSaveLink.Text = "File Save Location";
                }
            
            
            


        }

     


        // This is the magical thing that makes work happen.  This is where I actually do the work so as not to freeze the whole bloody thing.  I don't know, I just do work
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //Thread things I don't fully understand, I just do things
            BackgroundWorker worker = sender as BackgroundWorker;

            ConcurrentBag<HotelItem> Hotels = new ConcurrentBag<HotelItem>();


            //Status object for tracking things
            var status = new StatusItem();

            status.CountryText = "0 / 0";
            status.CityText = "0 / 0";
            status.HotelText = "NA";
            status.CountryValue = 0;
            status.CityValue = 0;
            status.HotelValue = 0;
            status.Status = "Loading";
            worker.ReportProgress( 0, status );

            //Take in the state of the application so it stops thinking the world ends when I cross the threads
            var auths = e.Argument as string[];

            var PostingURL = auths[0];
            var ClientID = auths[1];
            var Email = auths[2];
            var Password = auths[3];
            var FileLocation = auths[4];


            status.Status = "Building SearchCountryRequest (GTA country codes)";
            worker.ReportProgress(0, status);

            var requestCountries = new t_Request();
            requestCountries.Source = NewHeader(ClientID, Email, Password);


            requestCountries.RequestDetails = new t_RequestDetails();

            var SearchCountryRequest = new t_SearchCountryRequest();
            SearchCountryRequest.ISO = false;
            SearchCountryRequest.CountryName = "";
            SearchCountryRequest.CountryCode = "";
            SearchCountryRequest.ISOSpecified = true;


            requestCountries.RequestDetails.AddItem(ItemsChoiceType.SearchCountryRequest ,SearchCountryRequest);

            status.Status = "Sending SearchCountryRequest (GTA country codes)";
            worker.ReportProgress(0, status);
            var XMLresult = Retry.Do(() => SendRequest(requestCountries, PostingURL), TimeSpan.FromSeconds(1));
            XmlNodeList CountryNodes = XMLresult.DocumentElement.SelectNodes("//Country");

            status.Status = "Processing SearchCountryResponse (GTA country codes)";
            worker.ReportProgress(0, status);

            // I will use a table to easily match GTA and ISO codes for the countries,
            // using the country name for matching
            System.Data.DataTable Countries = new DataTable("Countries");
            DataColumn column;
            DataRow row;

            // First column and primary key: GTA country code
            column = new DataColumn();
            column.DataType= System.Type.GetType("System.String");
            column.ColumnName = "CountryCode";
            column.AutoIncrement = false;
            column.Caption = "CountryCode";
            column.ReadOnly = false;
            column.Unique = true;
            Countries.Columns.Add(column);

            // Second column: ISO country code
            column = new DataColumn();
            column.DataType= System.Type.GetType("System.String");
            column.ColumnName = "CountryISOCode";
            column.AutoIncrement = false;
            column.Caption = "CountryISOCode";
            column.ReadOnly = false;
            column.Unique = false;
            Countries.Columns.Add(column);

            // Third and last column: country name
            column = new DataColumn();
            column.DataType= System.Type.GetType("System.String");
            column.ColumnName = "CountryName";
            column.AutoIncrement = false;
            column.Caption = "CountryName";
            column.ReadOnly = false;
            column.Unique = false;
            Countries.Columns.Add(column);

            // Make first column primary key
            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = Countries.Columns["CountryCode"];
            Countries.PrimaryKey = PrimaryKeyColumns;
            
            // Insert into the table the results from the SearchCountryRequest
            foreach (XmlNode country in CountryNodes)
            {
                row = Countries.NewRow();
                row["CountryCode"] = country.Attributes["Code"].Value;
                row["CountryISOCode"] = "";
                row["CountryName"] = country.InnerText;
                Countries.Rows.Add(row);
            }

            // Search for countries again, this time asking for the ISO code
            status.Status = "Building SearchCountryRequest (ISO country codes)";
            worker.ReportProgress(0, status);

            var requestCountriesISO = new t_Request();
            requestCountriesISO.Source = NewHeader(ClientID, Email, Password);


            requestCountriesISO.RequestDetails = new t_RequestDetails();

            var SearchCountryISORequest = new t_SearchCountryRequest();
            SearchCountryISORequest.ISO = true;
            SearchCountryISORequest.CountryName = "";
            SearchCountryISORequest.CountryCode = "";
            SearchCountryISORequest.ISOSpecified = true;


            requestCountriesISO.RequestDetails.AddItem(ItemsChoiceType.SearchCountryRequest ,SearchCountryISORequest);

            status.Status = "Sending SearchCountryRequest (ISO country codes)";
            worker.ReportProgress(0, status);
            XMLresult = Retry.Do(() => SendRequest(requestCountriesISO, PostingURL), TimeSpan.FromSeconds(1));
            XmlNodeList ISOCountryNodes = XMLresult.DocumentElement.SelectNodes("//Country");

            status.Status = "Processing SearchCountryResponse (ISO country codes)";
            worker.ReportProgress(0, status);

            // Update table with the ISO codes
            foreach (XmlNode country in ISOCountryNodes)
            {
                row = Countries.Select(String.Format("CountryName = '{0}'", country.InnerText)).FirstOrDefault();
                row["CountryISOCode"] = country.Attributes["Code"].Value;
            }
            
            //Countries.WriteXml(FileLocation.Replace(".csv", "_countryTable.csv"));
            
            var i_country = 0;
            foreach (DataRow country in Countries.Rows)
            {
                
                if ((worker.CancellationPending == true))
                {
                    status.CountryValue = 0;
                    status.CityValue = 0;
                    status.HotelValue = 0;
                    status.Status = "Operation Canceled";
                    worker.ReportProgress(0, status);
                    e.Cancel = true;
                    break;
                }
                else
                {
                    i_country++;
                    // We're Getting a list of the cities in a country

                    //System.Threading.Thread.Sleep(500);
                    status.CountryText = (i_country.ToString() + " / " + Countries.Rows.Count.ToString() + "   " + country["CountryCode"].ToString());
                    status.CountryValue = (int)((float)i_country / (float)Countries.Rows.Count *100);
                    status.Status = "Building SearchCityRequest";
                    worker.ReportProgress(   status.CountryValue, status );


                    var requestCities = new t_Request();
                    requestCities.Source = NewHeader(ClientID, Email, Password);

                    var citySearchRequest = new t_SearchCityRequest();

                    citySearchRequest.CountryCode = country["CountryCode"].ToString();
                    citySearchRequest.CityCode = "";
                    citySearchRequest.CityName = "";
                    citySearchRequest.ISO = false;
                    citySearchRequest.ISOSpecified = true;

                    requestCities.RequestDetails.AddItem(ItemsChoiceType.SearchCityRequest, citySearchRequest);


                    status.Status = "Sending SearchCityRequest";
                    worker.ReportProgress(status.CountryValue, status);


                    XMLresult = Retry.Do(() => SendRequest(requestCities, PostingURL), TimeSpan.FromSeconds(1));
                    XmlNodeList CityNodes = XMLresult.DocumentElement.SelectNodes("//City");
                    status.Status = "Processing SearchCityResponse";
                    worker.ReportProgress(status.CountryValue, status);

                    var i_city = 0;
                    var i_city_tot = CityNodes.Count;
                        //Lets search for hotels in our city
                    ThreadPool.SetMinThreads(20, 40);
                            Parallel.ForEach(CityNodes.Cast<XmlNode>(),new ParallelOptions { MaxDegreeOfParallelism = 20 }, city =>
                             {
                                 if ((worker.CancellationPending == true))
                                 {
                                     status.CountryValue = 0;
                                     status.CityValue = 0;
                                     status.HotelValue = 0;
                                     status.Status = "Operation Canceled";
                                     worker.ReportProgress(0, status);
                                     e.Cancel = true;
                                     return;
                                 }
                                 else
                                 {
                                     status.CityValue = (int)((float)i_city / i_city_tot * 100);
                                     status.CityText = i_city.ToString() + " / " + i_city_tot.ToString() + "   " + city.Attributes["Code"].Value;
                                     status.Status = "Building SearchItemInformationRequest";
                                     worker.ReportProgress(status.CountryValue, status);



                                     var requestItemInformation = new t_Request();
                                     requestItemInformation.Source = NewHeader(ClientID, Email, Password);

                                     var searchiteminfo = new t_SearchItemInformationRequest();

                                     searchiteminfo.ItemType = t_ItemType.hotel;
                                     searchiteminfo.ItemTypeSpecified = true;
                                     searchiteminfo.ItemDestination = new t_ItemDestination();
                                     searchiteminfo.ItemDestination.DestinationCode = city.Attributes["Code"].Value;
                                     searchiteminfo.ItemDestination.DestinationType = t_DestinationType.city;


                                     requestItemInformation.RequestDetails.AddItem(ItemsChoiceType.SearchItemInformationRequest, searchiteminfo);



                                     status.Status = "Sending SearchItemInformationRequest";
                                     worker.ReportProgress(status.CountryValue, status);

                                     var XMLItemresult = Retry.Do(() => SendRequest(requestItemInformation, PostingURL), TimeSpan.FromSeconds(1));

                                     status.Status = "Proccessing SearchItemInformationResponse";
                                     worker.ReportProgress(status.CountryValue, status);

                                     

                                     //Time to parse the shit out of some Hotel information!  Although, we might want to first see if we can even see this hotel huh?

                                     if (XMLItemresult.SelectSingleNode("//ItemDetail") != null)
                                     {


                                         foreach (XmlNode h in XMLItemresult.SelectNodes("//ItemDetail"))
                                         {

                                             var Hotel = new HotelItem();
                                             //Let's just make sure we don't Null out later.  I just don't trust the data enough to not do this.
                                             Hotel.HotelCode = "";
                                             Hotel.HotelName = "";
                                             Hotel.ItemCode = "";
                                             Hotel.Location = "";
                                             Hotel.AddressLine1 = "";
                                             Hotel.AddressLine2 = "";
                                             Hotel.AddressLine3 = "";
                                             Hotel.AddressLine4 = "";
                                             Hotel.Telephone = "";
                                             Hotel.Fax = "";
                                             Hotel.Email = "";
                                             Hotel.Website = "";
                                             Hotel.StarRating = "";
                                             Hotel.Category = "";
                                             Hotel.Latitude = "";
                                             Hotel.Longitude = "";
                                             Hotel.CityCode = "";
                                             Hotel.CityName = "";
                                             Hotel.CountryCode = "";
                                             Hotel.CountryISOCode = "";
                                             Hotel.CountryName = "";
                                             Hotel.Map = "";
                                             Hotel.MapStatus = "";
                                             Hotel.NumberOfImages = 0;

                                             //Now we check the XPath's one by one and we'll update the hotel object and wham bam thank you ma'm.


                                             //Let's genearate a unique HotelCode
                                             if (h.SelectSingleNode(".//City") != null && h.SelectSingleNode(".//Item") != null)
                                             {
                                                 Hotel.HotelCode = h.SelectSingleNode(".//City").Attributes["Code"].Value + "_" + h.SelectSingleNode(".//Item").Attributes["Code"].Value;
                                             }

                                             //Get the Hotel Name
                                             if (h.SelectSingleNode(".//Item") != null)
                                             {
                                                 Hotel.HotelName = h.SelectSingleNode(".//Item").InnerText;
                                             }

                                             //Retrieve Item Code
                                             if (h.SelectSingleNode(".//Item") != null)
                                             {
                                                 Hotel.ItemCode = h.SelectSingleNode(".//Item").Attributes["Code"].Value;
                                             }

                                             //Create a list of locations
                                             if (h.SelectSingleNode(".//Location") != null)
                                             {
                                                 List<string> locations = new List<string>();
                                                 foreach (XmlNode loc in h.SelectNodes(".//Location"))
                                                 {
                                                     locations.Add(loc.InnerText);
                                                 }
                                                 Hotel.Location = String.Join(" | ", locations);
                                             }

                                             //Addressses
                                             if (h.SelectSingleNode(".//AddressLine1") != null)
                                             {

                                                 Hotel.AddressLine1 = h.SelectSingleNode(".//AddressLine1").InnerText;
                                             }
                                             if (h.SelectSingleNode(".//AddressLine2") != null)
                                             {

                                                 Hotel.AddressLine2 = h.SelectSingleNode(".//AddressLine2").InnerText;
                                             }
                                             if (h.SelectSingleNode(".//AddressLine3") != null)
                                             {

                                                 Hotel.AddressLine3 = h.SelectSingleNode(".//AddressLine3").InnerText;
                                             }
                                             if (h.SelectSingleNode(".//AddressLine4") != null)
                                             {

                                                 Hotel.AddressLine4 = h.SelectSingleNode(".//AddressLine4").InnerText;
                                             }

                                             //Telephone
                                             if (h.SelectSingleNode(".//Telephone") != null)
                                             {

                                                 Hotel.Telephone = h.SelectSingleNode(".//Telephone").InnerText;
                                             }

                                             //Fax
                                             if (h.SelectSingleNode(".//Fax") != null)
                                             {

                                                 Hotel.Fax = h.SelectSingleNode(".//Fax").InnerText;
                                             }

                                             //Website
                                             if (h.SelectSingleNode(".//WebSite") != null)
                                             {

                                                 Hotel.Website = h.SelectSingleNode(".//WebSite").InnerText;
                                             }

                                             //Email
                                             if (h.SelectSingleNode(".//EmailAddress") != null)
                                             {

                                                 Hotel.Email = h.SelectSingleNode(".//EmailAddress").InnerText;
                                             }

                                             //Star Rating
                                             if (h.SelectSingleNode(".//StarRating") != null)
                                             {

                                                 Hotel.StarRating = h.SelectSingleNode(".//StarRating").InnerText;
                                             }

                                             //Category
                                             if (h.SelectSingleNode(".//Category") != null)
                                             {

                                                 Hotel.Category = h.SelectSingleNode(".//Category").InnerText;
                                             }

                                             //Lat&Lon
                                             if (h.SelectSingleNode(".//Latitude") != null)
                                             {

                                                 Hotel.Latitude = h.SelectSingleNode(".//Latitude").InnerText;
                                             }
                                             if (h.SelectSingleNode(".//Longitude") != null)
                                             {

                                                 Hotel.Longitude = h.SelectSingleNode(".//Longitude").InnerText;
                                             }

                                             //Map
                                             if (h.SelectSingleNode(".//MapPageLink") != null)
                                             {

                                                 Hotel.Map = h.SelectSingleNode(".//MapPageLink").InnerText;
                                                 //Check if the map can really be accesed
                                                 HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Hotel.Map);
                                                 request.Method = "HEAD";
                                                 try
                                                 {
                                                     HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                                     if (response == null)
                                                     {
                                                         Hotel.MapStatus = "No response";
                                                     }
                                                     else
                                                     {
                                                         Hotel.MapStatus = response.StatusCode.ToString();
                                                         response.Close();
                                                     }
                                                 }
                                                 catch(Exception ex)
                                                 {
                                                     status.Status = String.Format("Error trying to access map link {0}, exception {1}", Hotel.Map, ex.Message);
                                                     worker.ReportProgress(status.CountryValue, status);
                                                     Hotel.MapStatus = "Error retrieving map";
                                                 }
                                             }

                                             //Retrieve number of images
                                             if (h.SelectSingleNode(".//ImageLink") != null)
                                             {
                                                 Hotel.NumberOfImages = h.SelectNodes(".//ImageLink").Count;
                                             }

                                             Hotel.CityCode = city.Attributes["Code"].Value;
                                             Hotel.CityName = city.InnerText;
                                             Hotel.CountryISOCode = country["CountryISOCode"].ToString();
                                             Hotel.CountryCode = country["CountryCode"].ToString();
                                             Hotel.CountryName = country["CountryName"].ToString();




                                             Hotels.Add(Hotel);




                                         }

                                     }
                                     else
                                     {

                                         status.Status = "No Hotels Available";
                                         worker.ReportProgress(status.CountryValue, status);
                                     }

                                     Interlocked.Add(ref i_city, 1);
                                     status.CityValue = (int)((float)i_city / i_city_tot * 100);
                                     status.CityText = i_city.ToString() + " / " + i_city_tot.ToString() + "   " + city.Attributes["Code"].Value;
                                     status.Status = "Processing Finished SearchItemInformationRequest";
                                     worker.ReportProgress(status.CountryValue, status);
                                 }


                             });

                        }



                    }

            
            //Should not be necessary now
            /*status.Status = "DeDuplicating Hotels";
            worker.ReportProgress(status.CountryValue, status);
            var dedupe = new List<HotelItem>();
            //DeDuplicate the hotel collection!  BLAHRG
            foreach (var h1 in Hotels)
            {
                var add = true;
                foreach (var h2 in dedupe)
                {
                    if (h1.HotelCode == h2.HotelCode)
                        add = false;
                }
                if (add)
                    dedupe.Add(h1);
            }*/

            //TODO: Calculate average distance of each hotel to the rest on the same distance


            //Time to write to the file, Apparently we've somehow survived!
            status.Status = "Writing Hotels to file";
            worker.ReportProgress(status.CountryValue, status);

            //Sort the hotellist before writing for pretty results
            List<HotelItem> OrderedHotels = Hotels.OrderBy(H => H.CountryName).ThenBy(H => H.CityName).ThenBy(H => H.HotelName).ToList();

            using (var writer = new StreamWriter(FileLocation))
            {

                writer.WriteLine("Hotel Code,Hotel Name,Item Code,Location,Address Line 1,Address Line 2,Address Line 3,Address LIne 4,Telephone,Fax,Email,Website,Star Rating,Category,Latitude,Longitude,City Code,City Name,Country Code,Country ISO Code,Country Name,Map,Map Status,Number of Images");

                foreach (var h in OrderedHotels)
                {

                    writer.Write("\"" + h.HotelCode.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.HotelName.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.ItemCode.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Location.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.AddressLine1.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.AddressLine2.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.AddressLine3.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.AddressLine4.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Telephone.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Fax.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Email.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Website.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.StarRating.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Category.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Latitude.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Longitude.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.CityCode.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.CityName.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.CountryCode.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.CountryISOCode.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.CountryName.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.Map.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.MapStatus.Replace("\"", "") + "\",");
                    writer.Write("\"" + h.NumberOfImages.ToString() + "\"");
                    writer.WriteLine();

                }


                status.Status = "Completed Run";
                worker.ReportProgress(status.CountryValue, status);

            }



        }


        //Main progress indicator, I think I'll need more of these
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var s = (e.UserState as StatusItem);

            countryProgress.Value = s.CountryValue;
            countryProgressText.Text = s.CountryText ;
            cityProgress.Value = s.CityValue;
            cityProgressText.Text = s.CityText;
            statusText.Text = s.Status;

        }


        //Main BGWorker completed.  Holy shit, we've done all the work, or we've exploded.  I mean, both are possible.  I hate my life.
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                downloadbutton.Text = "Download Properties";
                cityProgress.Value = 0;
                countryProgress.Value = 0;
                statusText.Text = ("Canceled!");
            }

            else if (!(e.Error == null))
            {
                downloadbutton.Text = "Download Properties";
                cityProgress.Value = 0;
                countryProgress.Value = 0;
                statusText.Text = ("Error: " + e.Error.Message);
            }

            else
            {

                downloadbutton.Text = "Download Properties";
                cityProgress.Value = 100;
                countryProgress.Value = 100;
                statusText.Text = ("Done!");
            }
        }




        //Lets make a sexy sexy header.  YAY.
        public t_Source NewHeader(string ClientID, string emailBox, string passwordBox)
        {
            var s = new t_Source();

            s.RequestorID = new t_RequestorID();
            s.RequestorID.Client = ClientID;
            s.RequestorID.EMailAddress = emailBox;
            s.RequestorID.Password = passwordBox;

            s.RequestorPreferences = new t_RequestorPreferences();
            s.RequestorPreferences.RequestMode = t_RequestMode.SYNCHRONOUS;


            return s;

        } 


        //Abstracted out this sendRequest thing, so this way I can just get a useful XML object.   I wanted to desearlize it back into a t_Response object but I'm apparently a moron and I don't know how.  XML Parsing FTW.
        //No error catching here btw, I'll be doing my error catching in a retry clause and if the same request fails 3 times, well then, buggeritall we'll just crash the application
        public XmlDocument SendRequest(t_Request RequestToSend, string PostingURL)
        {
            var xml = new XmlDocument();
            var request = WebRequest.Create(PostingURL);

            request.Method = "POST";

            request.ContentLength = RequestToSend.Serialize().Length;
            request.ContentType = "text/xml";
            request.Timeout = 180000;

            
            var dataStream = request.GetRequestStream();
            dataStream.Write(System.Text.Encoding.UTF8.GetBytes(RequestToSend.Serialize()), 0, RequestToSend.Serialize().Length);
            dataStream.Close();

            var response = request.GetResponse();
            
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), UTF8Encoding.UTF8))
            {
                string responseText = reader.ReadToEnd();

                xml.LoadXml(responseText);
            }


            return xml;
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

    }


    public static class Retry
    {
        public static void Do(
            Action action,
            TimeSpan retryInterval,
            int retryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }

        public static T Do<T>(
            Func<T> action,
            TimeSpan retryInterval,
            int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }
    }

}

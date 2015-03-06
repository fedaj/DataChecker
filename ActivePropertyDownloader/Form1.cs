﻿using System;
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
                downloadbutton.Text = "Cancel Load";
                //Lets send the currently values of our setup to the new thread so it doesn't bitch about how I shouldn't cross my streams....
                object[] parameters = new string[] { (SiteList.SelectedItem as ComboboxItem).Value, ClientID.Value.ToString(), emailBox.Text, passwordBox.Text };
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
            
             linkLabel1.Text = d.FileName;
                } else {
                    MessageBox.Show("Please choose a valid location");
                    linkLabel1.Text = "Safe File Location";
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


            status.Status = "Building SearchCountryRequest";
            worker.ReportProgress(0, status);

            var requestCountries = new t_Request();
            requestCountries.Source = NewHeader(ClientID, Email, Password);


            requestCountries.RequestDetails = new t_RequestDetails();

            var SearchCountryRequest = new t_SearchCountryRequest();
            SearchCountryRequest.ISO = true;
            SearchCountryRequest.CountryName = "";
            SearchCountryRequest.CountryCode = "";
            SearchCountryRequest.ISOSpecified = true;


            requestCountries.RequestDetails.AddItem(ItemsChoiceType.SearchCountryRequest ,SearchCountryRequest);

            status.Status = "Sending SearchCountryRequest";
            worker.ReportProgress(0, status);
            var XMLresult = Retry.Do(() => SendRequest(requestCountries, PostingURL), TimeSpan.FromSeconds(1));
            XmlNodeList CountryNodes = XMLresult.DocumentElement.SelectNodes("//Country");

            status.Status = "Processing SearchCountryResponse";
            worker.ReportProgress(0, status);

            var i_country = 0;
            foreach (XmlNode country in CountryNodes)
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
                    status.CountryText = (i_country.ToString() + " / " + CountryNodes.Count.ToString() + "   " + country.Attributes["Code"].Value);
                    status.CountryValue = (int)((float)i_country / (float)CountryNodes.Count *100);
                    status.Status = "Building SearchCityRequest";
                    worker.ReportProgress(   status.CountryValue, status );


                    var requestCities = new t_Request();
                    requestCities.Source = NewHeader(ClientID, Email, Password);

                    var citySearchRequest = new t_SearchCityRequest();

                    citySearchRequest.CountryCode = country.Attributes["Code"].Value;
                    citySearchRequest.CityCode = "";
                    citySearchRequest.CityName = "";
                    citySearchRequest.ISO = true;
                    citySearchRequest.ISOSpecified = true;

                    requestCities.RequestDetails.AddItem(ItemsChoiceType.SearchCityRequest, citySearchRequest);


                    status.Status = "Sending SearchCityRequest";
                    worker.ReportProgress(status.CountryValue, status);


                    XMLresult = Retry.Do(() => SendRequest(requestCities, PostingURL), TimeSpan.FromSeconds(1));
                    XmlNodeList CityNodes = XMLresult.DocumentElement.SelectNodes("//City");
                    status.Status = "Processing SearchCityResponse";
                    worker.ReportProgress(status.CountryValue, status);

                    var i_city = 0;
                    foreach (XmlNode city in CityNodes)
                    {
                        //Lets search for hotels in our city
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
                            i_city++;

                            status.CityText = (i_city.ToString() + " / " + CityNodes.Count.ToString() + "   " + city.Attributes["Code"].Value);
                            status.CityValue = (int)((float)i_city / (float)CityNodes.Count * 100);
                            status.Status = "Building SearchItemRequest";
                            worker.ReportProgress(status.CountryValue, status);


                            var requestItems = new t_Request();
                            requestItems.Source = NewHeader(ClientID, Email, Password);

                            var searchitemreq = new t_SearchItemRequest();


                            searchitemreq.ItemType = t_ItemType.hotel;
                            searchitemreq.ItemDestination = new t_ItemDestination();
                            searchitemreq.ItemDestination.DestinationCode = city.Attributes["Code"].Value;
                            searchitemreq.ItemDestination.DestinationType = t_DestinationType.city;


                            requestItems.RequestDetails.AddItem(ItemsChoiceType.SearchItemRequest, searchitemreq);



                             status.Status = "Sending SearchItemRequest";
                             worker.ReportProgress(status.CountryValue, status);


                             XMLresult = Retry.Do(() => SendRequest(requestItems, PostingURL), TimeSpan.FromSeconds(1));
                             XmlNodeList ItemNodes = XMLresult.DocumentElement.SelectNodes("//Item");
                             status.Status = "Processing SearchItemResponse";
                             worker.ReportProgress(status.CountryValue, status);

                             var i_item = 0;
                             var i_item_tot = (float)ItemNodes.Count;
                             Parallel.ForEach(ItemNodes.Cast<XmlNode>(), new ParallelOptions { MaxDegreeOfParallelism = 10 }, item =>
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
                                     Interlocked.Add(ref i_item, 1);
                                     status.HotelValue = (int)((float)i_item / i_item_tot * 100);
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
                                     searchiteminfo.ItemCode = item.Attributes["Code"].Value;



                                     requestItemInformation.RequestDetails.AddItem(ItemsChoiceType.SearchItemInformationRequest, searchiteminfo);

                                     var test = requestItemInformation.Serialize();
                                     Console.WriteLine(test);


                                     status.Status = "Sending SearchItemInformationRequest";
                                     worker.ReportProgress(status.CountryValue, status);

                                     var XMLItemresult = Retry.Do(() => SendRequest(requestItemInformation, PostingURL), TimeSpan.FromSeconds(1));

                                     status.Status = "Proccessing SearchItemInformationResponse";
                                     worker.ReportProgress(status.CountryValue, status);

                                     var Hotel = new HotelItem();

                                     //Time to parse the shit out of some Hotel information!  Although, we might want to first see if we can even see this hotel huh?

                                     if (XMLItemresult.SelectSingleNode("//ItemDetail") != null)
                                     {

                                         XmlNode h = XMLItemresult.SelectSingleNode("//ItemDetail");

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

                                         //Now we check the XPath's one by one and we'll update the hotel object and wham bam thank you ma'm.


                                         //Let's genearate a unique HotelCode
                                         if (h.SelectSingleNode("//City") != null && h.SelectSingleNode("//Item") != null)
                                         {
                                             Hotel.HotelCode = h.SelectSingleNode("//City").Attributes["Code"].Value + "_" + h.SelectSingleNode("//Item").Attributes["Code"].Value;
                                         }
                                         
                                         //Get the Hotel Name
                                         if (h.SelectSingleNode("//Item") != null)
                                         {
                                             Hotel.HotelName = h.SelectSingleNode("//Item").InnerText;
                                         }

                                         //Retrieve Item Code
                                         if (h.SelectSingleNode("//Item") != null)
                                         {
                                             Hotel.ItemCode = h.SelectSingleNode("//Item").Attributes["Code"].Value;
                                         }

                                         //Create a list of locations
                                         if (h.SelectSingleNode("//Location") != null)
                                         {
                                             foreach(XmlNode loc in h.SelectNodes("//Location") )
                                             {
                                                 Hotel.Location = Hotel.Location + loc.InnerText +  " | ";
                                             }
                                         }

                                         //Addressses
                                         if (h.SelectSingleNode("//AddressLine1") != null)
                                         {

                                             Hotel.AddressLine1 = h.SelectSingleNode("//AddressLine1").InnerText;
                                         }
                                         if (h.SelectSingleNode("//AddressLine2") != null)
                                         {

                                             Hotel.AddressLine2 = h.SelectSingleNode("//AddressLine2").InnerText;
                                         }
                                         if (h.SelectSingleNode("//AddressLine3") != null)
                                         {

                                             Hotel.AddressLine3 = h.SelectSingleNode("//AddressLine3").InnerText;
                                         }
                                         if (h.SelectSingleNode("//AddressLine4") != null)
                                         {

                                             Hotel.AddressLine4 = h.SelectSingleNode("//AddressLine4").InnerText;
                                         }

                                         //Telephone
                                         if (h.SelectSingleNode("//Telephone") != null)
                                         {

                                             Hotel.Telephone = h.SelectSingleNode("//Telephone").InnerText;
                                         }

                                         //Fax
                                         if (h.SelectSingleNode("//Fax") != null)
                                         {

                                             Hotel.Fax = h.SelectSingleNode("//Fax").InnerText;
                                         }

                                         //Email
                                         if (h.SelectSingleNode("//EmailAddress") != null)
                                         {

                                             Hotel.Email = h.SelectSingleNode("//EmailAddress").InnerText;
                                         }

                                         //Star Rating
                                         if (h.SelectSingleNode("//StarRating") != null)
                                         {

                                             Hotel.StarRating = h.SelectSingleNode("//StarRating").InnerText;
                                         }

                                         //Category
                                         if (h.SelectSingleNode("//Category") != null)
                                         {

                                             Hotel.Category = h.SelectSingleNode("//Category").InnerText;
                                         }

                                         //Lat&Lon
                                         if (h.SelectSingleNode("//Latitude") != null)
                                         {

                                             Hotel.Latitude = h.SelectSingleNode("//Latitude").InnerText;
                                         }
                                         if (h.SelectSingleNode("//Longitude") != null)
                                         {

                                             Hotel.Longitude = h.SelectSingleNode("//Longitude").InnerText;
                                         }


                                         Hotel.CityCode = city.Attributes["Code"].Value;
                                         Hotel.CityName = city.InnerText;
                                         Hotel.CountryISOCode = country.Attributes["Code"].Value;
                                         Hotel.CountryCode = country.Attributes["Code"].Value;
                                         Hotel.CountryName = country.InnerText;




                                         Hotels.Add(Hotel);






                                     }
                                     else
                                     {

                                         status.Status = "Hotel Not Available :(";
                                         worker.ReportProgress(status.CountryValue, status);
                                     }


                                 }


                             });

                        }



                    }


                }

            }
            
            //Time to write to the file, Apparently we've somehow survived!


        }


        //Main progress indicator, I think I'll need more of these
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var s = (e.UserState as StatusItem);

            countryProgress.Value = s.CountryValue;
            countryProgressText.Text = s.CountryText ;
            cityProgress.Value = s.CityValue;
            cityProgressText.Text = s.CityText;
            hotelProgress.Value = s.HotelValue;
            hotelProgressText.Text = s.HotelText;
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
                hotelProgress.Value = 0;
                statusText.Text = ("Canceled!");
            }

            else if (!(e.Error == null))
            {
                downloadbutton.Text = "Download Properties";
                cityProgress.Value = 0;
                countryProgress.Value = 0;
                hotelProgress.Value = 0;
                statusText.Text = ("Error: " + e.Error.Message);
            }

            else
            {

                downloadbutton.Text = "Download Properties";
                cityProgress.Value = 100;
                countryProgress.Value = 100;
                hotelProgress.Value = 100;
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
            s.RequestorPreferences.Country = "US";
            s.RequestorPreferences.Currency = "USD";
            s.RequestorPreferences.Language = "en";
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
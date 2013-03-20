using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using LiveCurrencyConvertor.AppCode;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LiveCurrencyConvertor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public class SampleData
        {
            public string Day { get; set; }
            public string Temperature { get; set; }
            public int Number { get; set; }
        }

        public CurrencyConvertor_ServiceReference.CurrencyConvertorSoapClient serCli = new CurrencyConvertor_ServiceReference.CurrencyConvertorSoapClient();
        public CurrencyList allCurrency = new CurrencyList();

        public double CurrencyRate = 0.0;
        public string From = "...";
        public string To = "...";
        public MainPage()
        {
            this.InitializeComponent();

            From_listPicker.ItemsSource = allCurrency.cur;
            To_listPicker.ItemsSource = allCurrency.cur;
            loadData();
            
            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ConversionTitle"))
                convertionTitle.Text = ApplicationData.Current.LocalSettings.Values["ConversionTitle"].ToString();
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ConversionRate"))
                CurrencyRate = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["ConversionRate"].ToString());
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("From"))
                To = ApplicationData.Current.LocalSettings.Values["From"].ToString();
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("To"))
                From = ApplicationData.Current.LocalSettings.Values["To"].ToString();


        }

        private async void loadData()
        {
            IMobileServiceTable<CurrencySelected> curTable = App.MobileService.GetTable<CurrencySelected>();
            bool chk = false;
            try
            {
                var res = await curTable.Where(x => x.UIdentifier == getUniqueid()).ToListAsync();

                if (res.Count != 0)
                {
                    Currency fCur = allCurrency.cur.Where(x => x.curSf == res[res.Count-1].from).First();
                    From_listPicker.SelectedIndex = allCurrency.cur.IndexOf(fCur);

                    Currency tCur = allCurrency.cur.Where(x => x.curSf == res[res.Count-1].to).First();
                    To_listPicker.SelectedIndex = allCurrency.cur.IndexOf(tCur);

                    var selectedItem = (Currency)From_listPicker.SelectedItem;
                    if (selectedItem != null)
                    {
                        var currency = (Currency)To_listPicker.SelectedItem;
                        if (currency != null)
                        {
                            ApplicationData.Current.LocalSettings.Values["ConversionTitle"] = selectedItem.curSf + " to " + currency.curSf;
                            ApplicationData.Current.LocalSettings.Values["From"] = selectedItem.curSf;
                            ApplicationData.Current.LocalSettings.Values["To"] = currency.curSf;
                            await curTable.InsertAsync(new CurrencySelected { UIdentifier = getUniqueid(), to = currency.curSf, from = selectedItem.curSf });

                        }

                    }
                    ApplicationData.Current.LocalSettings.Values["ConversionRate"] = await serCli.ConversionRateAsync((CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)From_listPicker.SelectedItem).curSf),
                        (CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)To_listPicker.SelectedItem).curSf));
                }
                else
                {

                    await curTable.InsertAsync(new CurrencySelected { UIdentifier = getUniqueid(), to = "USD", from = "PKR" });
                    Currency fCur = allCurrency.cur.Where(x => x.curSf == "PKR").First();
                    From_listPicker.SelectedIndex = allCurrency.cur.IndexOf(fCur);
                    ApplicationData.Current.LocalSettings.Values["From"] = fCur.curSf;
                    Currency tCur = allCurrency.cur.Where(x => x.curSf == "USD").First();
                    To_listPicker.SelectedIndex = allCurrency.cur.IndexOf(tCur);
                    ApplicationData.Current.LocalSettings.Values["To"] = tCur.curSf;
                    ApplicationData.Current.LocalSettings.Values["ConversionTitle"] = ((Currency)From_listPicker.SelectedItem).curSf + " to " + ((Currency)To_listPicker.SelectedItem).curSf;

                    ApplicationData.Current.LocalSettings.Values["ConversionRate"] = await serCli.ConversionRateAsync((CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)From_listPicker.SelectedItem).curSf),
                        (CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)To_listPicker.SelectedItem).curSf));

                }
                chk = true;
            }
            catch (Exception)
            {


            }

            if (!chk)
            {
                await curTable.InsertAsync(new CurrencySelected { UIdentifier = getUniqueid(), to = "USD", from = "PKR" });
            }
        }

        public static string getUniqueid()
        {
            object DeviceUniqueID;
            byte[] DeviceIDbyte = null;
            DeviceIDbyte = (byte[])GetHardwareId();
            return Convert.ToBase64String(DeviceIDbyte);
        }

        public static byte[] GetHardwareId()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var stream = token.Id.AsStream();
            using (var reader = new BinaryReader(stream))
            {
                var bytes = reader.ReadBytes((int)stream.Length);
                return bytes;
            }
        }

        private async void From_listPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IMobileServiceTable<CurrencySelected> curTable = App.MobileService.GetTable<CurrencySelected>();
            try
            {
                ApplicationData.Current.LocalSettings.Values["ConversionTitle"] = ((Currency)From_listPicker.SelectedItem).curSf + " to " + ((Currency)To_listPicker.SelectedItem).curSf;
                ApplicationData.Current.LocalSettings.Values["ConversionRate"] = await serCli.ConversionRateAsync((CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)From_listPicker.SelectedItem).curSf),
                (CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)To_listPicker.SelectedItem).curSf));

                ApplicationData.Current.LocalSettings.Values["From"] = ((Currency)From_listPicker.SelectedItem).curSf;
                ApplicationData.Current.LocalSettings.Values["To"] = ((Currency)To_listPicker.SelectedItem).curSf;

                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ConversionTitle"))
                    convertionTitle.Text = ApplicationData.Current.LocalSettings.Values["ConversionTitle"].ToString();
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ConversionRate"))
                    CurrencyRate = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["ConversionRate"].ToString());
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("From"))
                    To = ApplicationData.Current.LocalSettings.Values["From"].ToString();
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("To"))
                    From = ApplicationData.Current.LocalSettings.Values["To"].ToString();

                CurrencyRate = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["ConversionRate"]);
                if (CurrencyRate != 0.0)
                {
                    resultOutput.Text = amountInput.Text + " " + To + " = " + (Double.Parse(amountInput.Text) * CurrencyRate) + " " + From;
                }
                else
                {
                    resultOutput.Text = amountInput.Text + " " + To + " = " + "N/A" + " " + From;
                }
                await curTable.InsertAsync(new CurrencySelected { UIdentifier = getUniqueid(), to = From, from = To });

            }
            catch (Exception)
            {

            }
        }

        private async void To_listPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IMobileServiceTable<CurrencySelected> curTable = App.MobileService.GetTable<CurrencySelected>();
            try
            {
                ApplicationData.Current.LocalSettings.Values["ConversionTitle"] = ((Currency)From_listPicker.SelectedItem).curSf + " to " + ((Currency)To_listPicker.SelectedItem).curSf;
                ApplicationData.Current.LocalSettings.Values["ConversionRate"] = await serCli.ConversionRateAsync((CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)From_listPicker.SelectedItem).curSf),
                (CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)To_listPicker.SelectedItem).curSf));

                ApplicationData.Current.LocalSettings.Values["From"] = ((Currency)From_listPicker.SelectedItem).curSf;
                ApplicationData.Current.LocalSettings.Values["To"] = ((Currency)To_listPicker.SelectedItem).curSf;

                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ConversionTitle"))
                    convertionTitle.Text = ApplicationData.Current.LocalSettings.Values["ConversionTitle"].ToString();
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ConversionRate"))
                    CurrencyRate = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["ConversionRate"].ToString());
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("From"))
                    To = ApplicationData.Current.LocalSettings.Values["From"].ToString();
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("To"))
                    From = ApplicationData.Current.LocalSettings.Values["To"].ToString();

                CurrencyRate = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["ConversionRate"]);
                if (CurrencyRate != 0.0)
                {
                    resultOutput.Text = amountInput.Text + " " + To + " = " + (Double.Parse(amountInput.Text) * CurrencyRate) + " " + From;
                }
                else
                {
                    resultOutput.Text = amountInput.Text + " " + To + " = " + "N/A" + " " + From;
                }
            }
            catch (Exception)
            {

            }
            await curTable.InsertAsync(new CurrencySelected { UIdentifier = getUniqueid(), to = From, from = To });
        }

        void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            bool afound = false;
            bool sfound = false;
            bool pfound = false;
            foreach (var command in args.Request.ApplicationCommands.ToList())
            {
                if (command.Label == "About")
                {
                    afound = true;
                }
                if (command.Label == "Settings")
                {
                    sfound = true;
                }
                if (command.Label == "Policy")
                {
                    pfound = true;
                }
            }
            if (!afound)
                args.Request.ApplicationCommands.Add(new SettingsCommand("s", "About", (p) => { cfoAbout.IsOpen = true; }));
            if (!sfound)
                args.Request.ApplicationCommands.Add(new SettingsCommand("s", "Settings", (p) => { cfoSettings.IsOpen = true; }));
            
            //    args.Request.ApplicationCommands.Add(new SettingsCommand("s", "Policy", (p) => { cfoPolicy.IsOpen = true; }));
            if (!pfound)
            args.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy policy", OpenPrivacyPolicy));

        }

        private async void OpenPrivacyPolicy(IUICommand command)
        {
            var uri = new Uri("http://www.thatslink.com/privacy-statment/ ");
            await Launcher.LaunchUriAsync(uri);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ConversionTitle"))
                convertionTitle.Text = ApplicationData.Current.LocalSettings.Values["ConversionTitle"].ToString();
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("ConversionRate"))
                CurrencyRate = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["ConversionRate"].ToString());
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("From"))
                To = ApplicationData.Current.LocalSettings.Values["From"].ToString();
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("To"))
                From = ApplicationData.Current.LocalSettings.Values["To"].ToString();

            if (CurrencyRate != 0.0)
            {
                resultOutput.Text = amountInput.Text + " " + To + " = " + (Double.Parse(amountInput.Text) * CurrencyRate) + " " + From;
            }
            else
            {
                resultOutput.Text = amountInput.Text + " " + To + " = " + "N/A" + " " + From;
            }

        }

        private void BtnShowSettings_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }

        private void Load_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Settings));
        }
    }
}

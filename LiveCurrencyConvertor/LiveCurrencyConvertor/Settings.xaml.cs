using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using LiveCurrencyConvertor.AppCode;
using LiveCurrencyConvertor.Common;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace LiveCurrencyConvertor
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Settings : LiveCurrencyConvertor.Common.LayoutAwarePage
    {
        public CurrencyConvertor_ServiceReference.CurrencyConvertorSoapClient serCli = new CurrencyConvertor_ServiceReference.CurrencyConvertorSoapClient();
        public CurrencyList allCurrency = new CurrencyList();

        public Settings()
        {
            this.InitializeComponent();
            From_listPicker.ItemsSource = allCurrency.cur;
            To_listPicker.ItemsSource = allCurrency.cur;
            loadData();
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
                    Currency fCur = allCurrency.cur.Where(x => x.curSf == res[0].from).First();
                    From_listPicker.SelectedIndex = allCurrency.cur.IndexOf(fCur);

                    Currency tCur = allCurrency.cur.Where(x => x.curSf == res[0].to).First();
                    To_listPicker.SelectedIndex = allCurrency.cur.IndexOf(tCur);

                    var selectedItem = (Currency) From_listPicker.SelectedItem;
                    if (selectedItem != null)
                    {
                        var currency = (Currency) To_listPicker.SelectedItem;
                        if (currency != null)
                        {
                           ApplicationData.Current.LocalSettings.Values["ConversionTitle"]  = selectedItem.curSf + " to " + currency.curSf;
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




        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private async void From_listPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["ConversionTitle"] = ((Currency)From_listPicker.SelectedItem).curSf + " to " + ((Currency)To_listPicker.SelectedItem).curSf;
                ApplicationData.Current.LocalSettings.Values["ConversionRate"] = await serCli.ConversionRateAsync((CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)From_listPicker.SelectedItem).curSf),
                (CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)To_listPicker.SelectedItem).curSf));
            }
            catch (Exception)
            {

            }
        }

        private async void To_listPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ApplicationData.Current.LocalSettings.Values["ConversionTitle"] = ((Currency)From_listPicker.SelectedItem).curSf + " to " + ((Currency)To_listPicker.SelectedItem).curSf;
                ApplicationData.Current.LocalSettings.Values["ConversionRate"] = await serCli.ConversionRateAsync((CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)From_listPicker.SelectedItem).curSf),
                (CurrencyConvertor_ServiceReference.Currency)System.Enum.Parse(typeof(CurrencyConvertor_ServiceReference.Currency), ((Currency)To_listPicker.SelectedItem).curSf));
            }
            catch (Exception)
            {

            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (MainPage));
        }
    }
}

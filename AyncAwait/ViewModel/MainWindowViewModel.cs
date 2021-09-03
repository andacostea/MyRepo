using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AyncAwait.Commands;
using AyncAwait.DataModel;

namespace AyncAwait.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
         
        private CancellationTokenSource _cts = new CancellationTokenSource();

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _result;
        private int _progressValue;

        public string ResultWindow
        {
            get => _result;
            set
            {
                if (value != _result)
                {
                    _result = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (value != _progressValue)
                {
                    _progressValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RunDownloadSyncCommand => new RelayCommand(RunDownloadSync, () => true);

        public ICommand RunDownloadAsyncCommand => new AsyncCommand(await RunDownloadAsync(_cts),  () => true);

        public ICommand CancelOperationCommand => new RelayCommand(CancelOperation, () => true);

        private void CancelOperation()
        {
            _cts.Cancel();
        } 

        public ICommand RunDownloadWhenAllCommand => new AsyncCommand( RunDownloadWhenAllAsync, () => true);

        public void RunDownloadSync()
        {
            ResultWindow = string.Empty;

            var watch = new Stopwatch();
            watch.Start();

            List<string> webSites = PrepareData();

            foreach (var site in webSites)
            {
                WebSiteDataModel result = DownloadWebSite(site);
                ReportWebSiteInfo(result);
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            ResultWindow += $"Total execution time: {elapsedMs}";
        }

        public async Task RunDownloadAsync()
        {
            try
            {
                ResultWindow = string.Empty;
                ProgressValue = 0;

                var watch = new Stopwatch();
                watch.Start();

                List<string> webSites = PrepareData();
                var output = new List<WebSiteDataModel>();

                foreach (var site in webSites)
                {
                    // It can be done either with await Task.Run or with the version of async
                    //WebSiteDataModel result = await Task.Run(() => DownloadWebSite(site));
                    WebSiteDataModel result = await DownloadWebSiteAsync(site);
                    output.Add(result);
                    ReportWebSiteInfo(result);

                    ProgressValue = (output.Count * 100) / webSites.Count;

                    _cts.Token.ThrowIfCancellationRequested();
                }

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                ResultWindow += $"Total execution time: {elapsedMs}";
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            
        }

        public async Task RunDownloadWhenAllAsync()
        {
            ResultWindow = string.Empty;

            var watch = new Stopwatch();
            watch.Start();

            List<string> webSites = PrepareData();
            var tasks = new List<Task<WebSiteDataModel>>();

            foreach (var site in webSites)
            {
                tasks.Add(DownloadWebSiteAsync(site));
            }

            await Task.WhenAll(tasks);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            ResultWindow += $"Total execution time: {elapsedMs}";
        }

        //public async Task RunDownloadParallelAsync()
        //{
        //    List<string> webSites = PrepareData();
        //    List<Task<WebSiteDataModel>> tasks = new List<Task<WebSiteDataModel>>();

        //    foreach (var site in webSites)
        //    {
                
        //    }
        //}

        public WebSiteDataModel DownloadWebSite(string websiteURL)
        {
            var output = new WebSiteDataModel
            {
                WebSiteUrl = websiteURL
            };

            var webClient = new WebClient();
            output.WebSiteData = webClient.DownloadString(websiteURL);

            return output;
        }

        public async Task<WebSiteDataModel> DownloadWebSiteAsync(string websiteURL)
        {
            var output = new WebSiteDataModel
            {
                WebSiteUrl = websiteURL
            };

            var webClient = new WebClient();
            output.WebSiteData = await webClient.DownloadStringTaskAsync(websiteURL);

            return output;
        }

        private List<string> PrepareData()
        {
            var output = new List<string>
            {
                "https://www.yahoo.com",
                "https://www.google.com",
                "https://www.microsoft.com",
                "https://www.cnn.com",
                "https://www.codeproject.com",
                "https://www.stackoverflow.com"
            };

            return output;
        }

        private void ReportWebSiteInfo(WebSiteDataModel webSiteData)
        {
            ResultWindow +=
                $"{webSiteData.WebSiteUrl} downloaded  {webSiteData.WebSiteData.Length} characters long {Environment.NewLine}";
        }
    }
}

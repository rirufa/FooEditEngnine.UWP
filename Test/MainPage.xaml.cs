using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using FooEditEngine.UWP;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Activation;
using System.Linq;
using Windows.Storage.AccessCache;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace Test
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MainViewModel vm = new MainViewModel();
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this.vm;
            //this.fooTextBox.AllowFocusOnInteraction = true;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var fileargs = e.Parameter as FileActivatedEventArgs;
            if (fileargs != null)
            {
                await this.LoadManyFiles(fileargs);
            }
            else if(this.vm.DocumentList.Count == 0)
            {
                this.vm.Initalize();
            }

            PrintManager.GetForCurrentView().PrintTaskRequested += MainPage_PrintTaskRequested;
            InputPane currentView = InputPane.GetForCurrentView();
            currentView.Showing += currentView_Showing;
            currentView.Hiding += currentView_Hiding;
        }

        public async Task LoadManyFiles(FileActivatedEventArgs fileargs)
        {
            var filepaths = from file in fileargs.Files
                            select file.Path;

            //MRUに追加しないと後で開けない
            foreach (var file in fileargs.Files)
            {
                StorageApplicationPermissions.MostRecentlyUsedList.Add(file, "mrufile");
                await this.vm.AddDocumentFromFile(file as IStorageFile);
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PrintManager.GetForCurrentView().PrintTaskRequested -= MainPage_PrintTaskRequested;
            InputPane currentView = InputPane.GetForCurrentView();
            currentView.Showing -= currentView_Showing;
            currentView.Hiding -= currentView_Hiding;
        }

        void currentView_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            this.Margin = new Thickness(0);
            args.EnsuredFocusedElementInView = true;
        }

        void currentView_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            this.Margin = new Thickness(0, 0, 0, args.OccludedRect.Height);
            args.EnsuredFocusedElementInView = true;
        }

        void MainPage_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            IAsyncAction async = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var doc = this.vm.CurrentDocument;
                var source = new DocumentSource(doc, new FooEditEngine.Padding(20, 20, 20, 20), this.FontFamily.Source, this.FontSize);
                source.ParseHF = (s,e)=> { return e.Original; };
                source.Header = "header";
                source.Fotter = "footer";
                source.Forground = new Windows.UI.Color() { R = 0, A = 255, B = 0, G = 0};
                source.Keyword1 = new Windows.UI.Color() { R = 0, A = 255, B = 255, G = 0 };
                source.Keyword2 = new Windows.UI.Color() { R = 128, A = 255, B = 255, G = 0 };
                source.Literal = new Windows.UI.Color() { R = 128, A = 255, B = 128, G = 0 };
                source.Comment = new Windows.UI.Color() { R = 255, A = 255, B = 0, G = 0 };
                source.Url = new Windows.UI.Color() { R = 0, A = 255, B = 255, G = 0 };
                source.LineBreak = doc.LineBreak;
                source.LineBreakCount = doc.LineBreakCharCount;

                PrintTask task = null;
                task = args.Request.CreatePrintTask("test", (e) =>
                {
                    e.SetSource(source);
                });
                task.Completed += async (s, e) => {
                    source.Dispose();
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        System.Diagnostics.Debug.WriteLine("finished printing");
                    });
                };
                PrintOptionBuilder<DocumentSource> builder = new PrintOptionBuilder<DocumentSource>(source);
                builder.BuildPrintOption(PrintTaskOptionDetails.GetFromPrintTaskOptions(task.Options));
            });
            Task t = WindowsRuntimeSystemExtensions.AsTask(async);
            t.Wait();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var filepicker = new FileOpenPicker();
            filepicker.FileTypeFilter.Add(".txt");
            var file = await filepicker.PickSingleFileAsync();
            await this.vm.AddDocumentFromFile(file);
        }

        private async void Print_Button_Click(object sender, RoutedEventArgs e)
        {
            await PrintManager.ShowPrintUIAsync();
        }

        private void Button_Click_New(object sender, RoutedEventArgs e)
        {
            this.vm.AddDocument();
        }
    }
}

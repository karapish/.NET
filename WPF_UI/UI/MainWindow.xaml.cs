
namespace TellusResourceAllocatorManagement.UI
{
    using Config;
    using Data;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using Outlook = Microsoft.Office.Interop.Outlook;
    using TellusResourceAllocatorManagement.ViewModels;
    using TellusResourceAllocatorManagement.Config;


    /// <summary>
    /// Window for displaying Resource Allocation Management Chains and Requests 
    /// </summary>
    public sealed partial class MainWindow
    {                  
        /// <summary>
        /// View model which helps accessing model and allows binding to UI controls
        /// </summary>         
        private MainViewModel mainViewModel;

        /// <summary>
        /// Selected environment (PROD, PPE and etc.)
        /// </summary>
        private EnvironmentConfigElement selectedEnvironment;

        public MainWindow()
        {            
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;            
        }

        /// <summary>
        /// Subscribe for unhandled exception message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            MessageBox.Show(
                owner: this,
                messageBoxText: string.Format(
                    "Unhandled exception occured: {0} \r\n {1}",
                    exception.Message, exception.StackTrace),
                caption: exception.GetType().ToString(),
                button: MessageBoxButton.OK); 
        }

        private IEnumerable GetEnvironmentsFromConfigManager()
        {
            EnvironmentConfigSection environments = ConfigurationManager.GetSection("sharedSettings") as EnvironmentConfigSection;            
            return environments.Environments;
        }

        private void PerformAction(Func<Request, string, bool> actionCode, string actionName)
        {
             // Get the list of Selected requests...
            List<Request> requests = this.GetSelectedRequestsInMainTable();
            if (requests.Count == 0)
            {
                return;
            }

            try
            {
                string fileName;

                if (!this.ShowActionConfirmationUI(
                        string.Format(
                            "Are you sure you want to {0} for selected requests?",
                            actionName),
                        null, // Don't show file dialog
                        out fileName)) return;

                List<string> failedUpdates = new List<string>();

                failedUpdates.AddRange(
                    (from request in requests
                     where !actionCode(request, fileName)
                     select request.FrTedPath));

                if (failedUpdates.Count == 0)
                {
                    MessageBox.Show(
                        string.Format(
                        "All of selected requests allocation were {0}ed.\r\n\r\n" +
                        "Note: the change will take a while to propagate through the system." +
                        "You may want to refresh the UI in a minute or so.", actionName),
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK);
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append(
                        string.Format(
                            "Unable to {0} allocation request for the following FrTeds:\r\n\r\n", 
                            actionName));

                    foreach (string frted in failedUpdates)
                        stringBuilder.AppendLine(frted);

                    stringBuilder.Append("\r\nLook at the application log for more info.");

                    MessageBox.Show(
                        stringBuilder.ToString(),
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("{0} failed. {1}", actionName, ex.Message),
                    ex.GetType().ToString(),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
        }


        /// <summary>
        /// Prompts the user to confirm whether they desire to continue with the Selected
        /// Action.
        /// </summary>
        /// <param name="statement">The Action statement to display to the user</param>
        /// <param name="showSaveFileDialog">
        /// True will display the SaveFileDialog.
        /// False will display the OpenFileDialog
        /// Null will not display any File Selection Dialog.
        /// </param>
        /// <param name="fileName">The output file name Selected</param>
        /// <returns>True if the user confirm the desire to continue with the Action</returns>
        public bool ShowActionConfirmationUI(string statement, bool? showSaveFileDialog, out string fileName)
        {
            MessageBoxResult selection = MessageBox.Show(
                statement,
                "Are you sure",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (selection != MessageBoxResult.Yes)
            {
                fileName = null;
                return false;
            }

            bool didUserConfirm = false;
            fileName = string.Empty;

            if (showSaveFileDialog.HasValue)
            {
                FileDialog fileDialog;
                if (showSaveFileDialog.Value == false)
                {
                    // Prompt the user with an Open File Dialog
                    fileDialog = new OpenFileDialog
                    {
                        Multiselect = false,
                    };
                }
                else
                {
                    // Prompt the user with a Save File Dialog
                    fileDialog = new SaveFileDialog();
                }

                fileDialog.DefaultExt = Path.GetExtension(this.mainViewModel.RamManager.RamFile);

                bool? dialogResult = fileDialog.ShowDialog();
                if (dialogResult == true)
                {
                    fileName = fileDialog.FileName;
                    didUserConfirm = true;
                }
            }
            else
            {
                // In case of null, the prompting is complete. Return true for confirmed.
                didUserConfirm = true;
            }

            return didUserConfirm;
        }

        /// <summary>
        /// Returns the list of selected requests in the grid.
        /// </summary>
        /// <returns>The list of selected requests</returns>
        private List<Request> GetSelectedRequestsInMainTable()
        {
            List<Request> result = new List<Request>();

            if (this.MainTable.ItemsSource == null)
            {
                return result;
            }

            IEnumerable<Request> selectedRequests =
                from Request r in MainTable.ItemsSource
                where r.Selected
                select r;

            IEnumerable<Request> requests = selectedRequests as IList<Request> ?? selectedRequests.ToList();

            if (requests.Any())
                return requests.ToList();

            MessageBox.Show(
                "No requests selected",
                "Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning,
                MessageBoxResult.OK,
                MessageBoxOptions.None);

            return result;
        }

        #region Buttons
        /// <summary>
        /// Handler for the Save button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            const string fileNameMSecRldTimestampFormat = "yyMMdd_HH-mm-ss-ffff";

            // Prompt the user with a dialog
            SaveFileDialog saveFileDialog =
                new SaveFileDialog
                {
                    DefaultExt = Path.GetExtension(this.mainViewModel.RamManager.RamFile),
                    OverwritePrompt = true,
                    ValidateNames = true,
                    AddExtension = true,
                    Filter = "XML documents (.xml)|*.xml",
                    Title = "Enter where you want to copy the RAM file to...",
                    FileName = "ResourceAllocationManagement_" + DateTime.Now.ToString(fileNameMSecRldTimestampFormat) + ".xml"
                };

            bool? showDialogResult = saveFileDialog.ShowDialog();
            if (!showDialogResult.HasValue || !showDialogResult.Value)  
                return;

            try
            {
                this.mainViewModel.RamManager.CopyRamFile(selectedEnvironment.RamFileLocation, saveFileDialog.FileName);

                StringCollection text = new StringCollection();
                text.Add(saveFileDialog.FileName);
                Clipboard.Clear();
                Clipboard.SetFileDropList(text);

                MessageBox.Show(
                    string.Format(
                       "RAM file has been copied to {0} and to clipboard", 
                       saveFileDialog.FileName),
                    "Success", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information, 
                    MessageBoxResult.OK);
            }
            catch(Exception ex)
            {
                MessageBox.Show(
                    string.Format(
                       "RAM file has not been copied to {0} and to clipboard",
                       saveFileDialog.FileName),
                    string.Format("{0} TypeOf {1}", ex.Message, ex.GetType()),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// Update the view model
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Button context click</param>
        public void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateViewModel();
        }

        /// <summary>
        /// Handler for the Copy button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            // Get selected requests
            List<Request> requests = this.GetSelectedRequestsInMainTable();
            if (requests.Count == 0)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            
            // Dump the header.
            stringBuilder.AppendLine("IsHeadOfChain\tChainID\tRequestID\tFrTedPath\tMinimumAllocationSize\tMaximumAllocationSize");
            
            // Dump the rows.
            foreach (Request request in requests)
            {
                stringBuilder.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", 
                    request.IsHeadOfChain,
                    request.ChainId,
                    request.Id,
                    request.FrTedPath,
                    request.MinimumAllocationSize,
                    request.MaximumAllocationSize);

                stringBuilder.AppendLine();
            }

            // Put it into the clipboard
            Clipboard.Clear();
            Clipboard.SetText(stringBuilder.ToString(), TextDataFormat.Text);

            // Tell the user we are done
            MessageBox.Show(
                "Information has been copied to clipboard", 
                "Information", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information, 
                MessageBoxResult.OK, 
                MessageBoxOptions.None);
        }

        /// <summary>
        /// Handler for the Email button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            FileInfo fileInfo = new FileInfo(this.mainViewModel.RamManager.LogFile);

            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                MessageBox.Show(
                    string.Format(
                        "Log file '{0}' does not exist or empty", 
                        fileInfo.FullName));

                return;
            }

            // Start email program (Outlook)

            // Get a handle to an existing Outlook.exe
            Func<Outlook.Application> activateOutlookObject = () =>
            {
                try
                {
                    return Marshal.GetActiveObject("Outlook.Application") as Outlook.Application;
                }
                catch
                {
                    return null;
                }
            };

            // Use existing or, if one is not available, try to start Outlook.exe
            Outlook.Application outlookInstance = activateOutlookObject(); 
            if (outlookInstance == null)
            {
                ProcessStartInfo psi = new ProcessStartInfo("outlook.exe")
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(startInfo: psi);

                for (int i = 0; i < 10 && outlookInstance == null; ++i)
                {
                    outlookInstance = activateOutlookObject();
                    Thread.Sleep(1000);
                }
            }

            // Do we finally have a handle to Outlook?
            if (outlookInstance == null)
            {
                MessageBox.Show(
                    "Unable to get a hold of Outlook to send email. You may need to manually start it!", 
                    "Information", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information, 
                    MessageBoxResult.OK);
            }

            // Create an email.
            Outlook.MailItem email = (Outlook.MailItem)(outlookInstance.CreateItem(Outlook.OlItemType.olMailItem));

            // Hook up common properties like subject, body, etc...
            email.Subject = "RAM UI debugging - application log (" + DateTime.Now + ")";
            email.BodyFormat = Outlook.OlBodyFormat.olFormatPlain;
            email.Body = "--\r\nThis is an automatically generated e/mail that captures the log file for your application. Send it to the developer Team, as directed.";
            email.Recipients.Add("OSG - BnB - Test System Technologies");
            email.CC = "Matteo Taveggia; Gleb Karapish";
            email.Recipients.ResolveAll();

            // Attach application log file.
            email.Attachments.Add(
                fileInfo.FullName, 
                Outlook.OlAttachmentType.olByValue, 
                email.Body.Length + 1, 
                fileInfo.Name);

            // Make it visible so the user can actually customize and send.
            email.Display();
        }

        /// <summary>
        /// Handler for the Help button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBoxResult.OK == MessageBox.Show(
                "You are about to open Team's OneNote. Select OK to proceed",
                "Redirection to OneNote", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Warning,
                MessageBoxResult.OK))
            {
                Process.Start(
                    "IExplore.exe", 
                    @"https://microsoft.sharepoint.com/teams/osg_core_bnb/ex/_layouts/15/WopiFrame.aspx?sourcedoc={5ccdd99e-5d39-4fb7-a408-3319570dabc3}&action=edit&wd=target%28Resources%2Eone%7C229D3CF5%2D025E%2D4BB7%2D92A5%2D7C290329CC6C%2FResource%20Allocator%20Console%20UI%7C01867BBF%2D24AC%2D40F2%2D8FDB%2D80A634BA6B20%2F%29onenote%3Ahttps%3A%2F%2Fmicrosoft%2Esharepoint%2Ecom%2Fteams%2Fosg%5Fcore%5Fbnb%2Fex%2FShared%20Documents%2FBackend%20Execution%2FBackend%2FResources%2Eone#Resource%20Allocator%20Console%20UI&section-id={229D3CF5-025E-4BB7-92A5-7C290329CC6C}&page-id={01867BBF-24AC-40F2-8FDB-80A634BA6B20}&end");
            }
        }

        /// <summary>
        /// Handler for the Priority update button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void ChangePriorityButton_Click(object sender, RoutedEventArgs e)
        {
            PriorityPicker priorityPickerDlg = new PriorityPicker();

            // If the user clicks on Cancel, we bail out... (last chance)
            if (priorityPickerDlg.ShowDialog() != true)
                return;

            // Retrieve the Selected priority from the dialog
            ushort newPriority = (ushort)priorityPickerDlg.PrioritySlider.Value;

            this.PerformAction((request, filename) => request.UpdatePriority(newPriority), "update priority");
        }

        /// <summary>
        /// Handler for the Filter button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handler for the Import button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            this.PerformAction(
                (request, filename) =>
                {
                    return request.Import(this.selectedEnvironment.RamFileLocation, filename);
                },
                "import");
        }

        /// <summary>
        /// Handler for the Cancel button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            List<Request> requests = GetSelectedRequestsInMainTable();
            if (requests.Count == 0)
            {
                return;
            }

            try
            {
                string fileName;

                if (!ShowActionConfirmationUI(
                        "Are you sure you want to cancel the Selected requests?",
                        null, // Don't show file dialog
                        out fileName)) 
                    return;

                List<string> failedUpdates = 
                    (from request in requests where !request.Cancel() 
                     select request.FrTedPath).ToList();

                if (failedUpdates.Count == 0)
                {
                    MessageBox.Show(
                        "All FrTeds corresponding to the selected requests were cancelled.\r\n\r\n" +
                        "Note: the change will take a while to propagate through the system." +
                        "You may want to refresh the UI in a minute or so.", 
                        "Info", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information, 
                        MessageBoxResult.OK);
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    stringBuilder.Append("Unable to cancel the following FrTeds:\r\n\r\n");

                    foreach (string frted in failedUpdates)
                        stringBuilder.AppendLine(frted);

                    stringBuilder.Append("\r\nLook at the application log for more info.");

                    MessageBox.Show(
                        stringBuilder.ToString(), 
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error, 
                        MessageBoxResult.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Cancel Requests: " + ex.GetType(),
                    "Action failed: " + ex.Message,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// Handler for the Stop button
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Button settings when pressed</param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            this.PerformAction((request, filename) => request.StopAllocation(), "stop");
        }
        #endregion

        #region UI events
        /// <summary>
        /// Handler for the main table selection check box
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">event arguments</param>
        public void SelectedRequest_Click(object sender, RoutedEventArgs e)
        {
            Request selection = MainTable.SelectedItem as Request;
            if (selection != null)
            {
                CheckBox checkBox = sender as CheckBox;
                if ((checkBox != null) && checkBox.IsChecked.HasValue)
                {
                    selection.Selected = checkBox.IsChecked.Value;
                }
            }
        }

        private void MainTable_MouseRightButtonDown(object sender, MouseButtonEventArgs mouseInfo)
        {
            DataGrid dataGrid = sender as DataGrid;
            Point pt = mouseInfo.GetPosition(dataGrid);
            GroupItem groupWithRows = null;

            // Do the hit test to find the group container
            VisualTreeHelper.HitTest(dataGrid, null, result =>
            {                
                groupWithRows = CommonUIOperations.FindVisualParent<GroupItem>(result.VisualHit);
                
                if (groupWithRows != null)
                {                    
                    return HitTestResultBehavior.Stop;
                }
                
                return HitTestResultBehavior.Continue;
            },
            new PointHitTestParameters(pt));

            if(groupWithRows == null || !(groupWithRows.HasContent))
            {
                return;                           
            }

            ReadOnlyObservableCollection<object> items = ((CollectionViewGroup)(groupWithRows.DataContext)).Items;
            foreach(object r in items)
            {
                Request request = r as Request;
                request.Selected = !request.Selected;
            }
        }

        private void MainTable_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollerViewer = CommonUIOperations.FindVisualChild<ScrollViewer>(this.MainTable);
            if (scrollerViewer == null)
                return; 

            if (VerticalScrollingSelector.IsChecked.HasValue &&
                VerticalScrollingSelector.IsChecked.Value)
            {
                 scrollerViewer.ScrollToVerticalOffset(scrollerViewer.VerticalOffset - e.Delta);
            }
            else
            {
                scrollerViewer.ScrollToHorizontalOffset(scrollerViewer.HorizontalOffset - e.Delta);
            }

            e.Handled = true;
        }

        private void EnvironmentSelector_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            combobox.ItemsSource = GetEnvironmentsFromConfigManager();
            combobox.SelectedIndex = 0;            
        }

        private void EnvironmentSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            this.selectedEnvironment = combobox.SelectedItem as EnvironmentConfigElement;

            if (this.mainViewModel == null)
            {
                this.mainViewModel = new MainViewModel(selectedEnvironment.RamFileLocation, selectedEnvironment.TmsBindingName);
                this.ModelUpdatedDateTime.DataContext = mainViewModel;
                this.TotalRequests.DataContext = mainViewModel;
                this.mainViewModel.UpdateCompleted += OnUpdatedCompleted;
                this.mainViewModel.UpdateStarted += OnUpdateStarted;     
            }
            else
            {
                if (this.selectedEnvironment != null)
                    this.mainViewModel.Setup(selectedEnvironment.RamFileLocation, selectedEnvironment.TmsBindingName);
            }
            
            this.mainViewModel.ClearView();
        }

        /// <summary>
        /// Navigates Explorer to a FrTed location on net share.
        /// Opens a browser and navigates to the test pass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrTedColumn_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = e.OriginalSource as Hyperlink;
            if (link == null) 
                return;

            string frTedPath = link.NavigateUri.OriginalString.ToLowerInvariant();
            
            if (string.IsNullOrWhiteSpace(frTedPath))
                return;

            // Launch File manager
            Process.Start("explorer", frTedPath);

            if(string.IsNullOrWhiteSpace(selectedEnvironment.WebPortal))
                return;

            const string scheduleText = "schedule_";
            int hitBeginning = frTedPath.IndexOf(scheduleText, StringComparison.InvariantCultureIgnoreCase) + scheduleText.Length;
            const int numCharsInGuid = 36;
            string guid = frTedPath.Substring(hitBeginning, numCharsInGuid);

            string urlToOpen = selectedEnvironment.WebPortal + "/Schedule.aspx?ScheduleId={0}";
            Process.Start("IExplore", string.Format(urlToOpen, guid));
        }

        private void GroupingSelector_Checked(object sender, RoutedEventArgs e)        
        {
            if(mainViewModel != null)
                mainViewModel.ResetView();
        }

        private void GroupingSelector_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mainViewModel != null)
                mainViewModel.ClearView();
        }

        private void SetFontButton_OnClick(object sender, RoutedEventArgs e)
        {
            double newSize;

            if (Double.TryParse(this.FontSizeInput.Text, out newSize))
            {
                this.FontSize = newSize;
            }
        }
        #endregion

        #region VM related
        /// <summary>
        /// Request to update the view model.
        /// </summary>
        public void UpdateViewModel()
        {
            // This is async call.
            this.mainViewModel.UpdateAsync(cancelPreviousIfQueued: false);
        }
      
        private void OnUpdateStarted()
        {
            // Call from UI thread
            Dispatcher.Invoke(() =>
            {
                this.ProgressBar.Visibility = Visibility.Visible;
                this.EnvironmentSelector.IsEnabled = false;
            });
        }

        private void OnUpdatedCompleted()
        {
            // Call from UI thread
            Dispatcher.Invoke(() =>
            {
                if (this.MainTable.ItemsSource == null)
                {
                    this.MainTable.ItemsSource = mainViewModel.RequestsView;
                }

                this.ProgressBar.Visibility = Visibility.Hidden;
                this.EnvironmentSelector.IsEnabled = true;                   
            });
        }
        #endregion       
    }
}

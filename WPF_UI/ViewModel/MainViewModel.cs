// Things to do:
//
// TODO - Add telemetry information
// TODO - Track all actions.
// TODO - Support changing text size.
// TODO - Add the Machine Query when hovering over to see what is in a chain.
// TODO - Add the Current Query Status "NoAvailableResources" the amount in Assignable, Available.
//             These are properties of the Top Level Requests in a CHAIN, and a CHAIN can have a single
//             Top level request. So these are properties of a CHAIN.
//

namespace TellusResourceAllocatorManagement.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Data;
    using TellusResourceAllocatorManagement.Data;

    /// <summary>
    /// MainViewModel which allows binding of UI and data model behind
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel(string ramFile, string tmsBindingName)
        {
            // Ask ram file manager to create logger 
            this.ramFileManager = new RamFileManager(logger: null);

            this.Setup(ramFile, tmsBindingName);
            this.ResetView();
            this.SetupBackgroundThread();

            this.Requests.CollectionChanged += OnRequestsCollectionChanged;
        }

        /// <summary>
        /// Thread to pull data from backend
        /// </summary>
        private BackgroundWorker backgroundWorker;

        #region Public props and supporting privates
        /// <summary>
        /// Data model
        /// </summary>
        private readonly RamFileManager ramFileManager;
        public RamFileManager RamManager
        {
            get { return this.ramFileManager; }
        }
        
        /// <summary>
        /// Time when view model was refreshed the last time
        /// </summary>
        private string lastUpdated;
        public string LastUpdated
        {
            private set
            {
                this.lastUpdated = value;
                this.OnPropertyChanged("LastUpdated");
            }

            get
            {
                return this.lastUpdated;
            }
        }

        /// <summary>
        /// The view which shows grouped requests
        /// </summary>
        public ICollectionView RequestsView
        {
            get;
            private set;
        }

        /// <summary>
        /// Raw chains data
        /// </summary>
        public ObservableCollection<Chain> Chains
        {
            get 
            {
                return this.ramFileManager.Chains;
            }
        }

        /// <summary>
        /// Raw requests data
        /// </summary>
        public ObservableCollection<Request> Requests
        {
            get
            {
                return this.ramFileManager.Requests;
            }
        }

        /// <summary>
        /// Number of total requests as string (to bind to UI)
        /// </summary>
        public string TotalRequests
        {
            get
            {
                return string.Format(
                    "Total request(s) {0}", 
                    this.Requests != null ? this.Requests.Count : 0);            
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Events all clients may subscribe for
        /// </summary>
        public event Action UpdateCompleted;
        public event Action UpdateStarted;
        
        /// <summary>
        /// Update binded UI control when the corresponding property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void OnRequestsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged("TotalRequests");
        }

        #region Public methods
        public bool UpdateAsync(bool cancelPreviousIfQueued)
        {
            if (this.backgroundWorker.IsBusy)
            {
                if(!cancelPreviousIfQueued)
                    return false;
// ReSharper disable once RedundantIfElseBlock
                else 
                {
                    this.backgroundWorker.Dispose();
                    this.backgroundWorker = null;
                    this.SetupBackgroundThread();                    
                }
            }

            if (this.backgroundWorker != null)
                this.backgroundWorker.RunWorkerAsync();

            return true;
        }

        public void Setup(string ramFile, string tmsBindingName)
        {
            IRequestAction handler = new RealRequestActionHandler(tmsBindingName, this.ramFileManager.Logger);
            this.ramFileManager.SetSettings(ramFile, handler);
        }

        public void ResetView()
        {
            this.RequestsView = CollectionViewSource.GetDefaultView(this.Requests);
            this.RequestsView.GroupDescriptions.Add(new PropertyGroupDescription("ChainId"));
        }

        public void ClearView()
        {
            this.RequestsView.GroupDescriptions.Clear();
        }
        #endregion

        #region Update model
        private void SetupBackgroundThread()
        {
            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += BackgroundThreadBody;
            this.backgroundWorker.RunWorkerCompleted += OnBackgroundThreadCompletion;
        }

        void BackgroundThreadBody(object sender, DoWorkEventArgs e)                
        {
            Debug.WriteLine("Bkground th #{0} has started exec", Thread.CurrentThread.ManagedThreadId);
            
            if (this.UpdateStarted != null)
            {
                this.UpdateStarted();
            }

            this.ramFileManager.Update(Application.Current.Dispatcher);            
        }

        void OnBackgroundThreadCompletion(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("Bkground th #{0} has completed exec", Thread.CurrentThread.ManagedThreadId);

            if (e.Cancelled)
            {
                return;
            }

            if (e.Error != null)
            {
                throw e.Error;
            }
            
            this.LastUpdated = string.Format("Last updated {0}", DateTime.Now);

            if(this.UpdateCompleted !=null)
            {
                this.UpdateCompleted();
            }
        #endregion
        }
    }
}

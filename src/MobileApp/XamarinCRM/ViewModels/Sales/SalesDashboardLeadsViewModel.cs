﻿using System;
using XamarinCRM.ViewModels.Base;
using XamarinCRM.Models;
using System.Collections.ObjectModel;
using XamarinCRM.Clients;
using Xamarin.Forms;
using XamarinCRM.Statics;
using System.Linq;
using System.Threading.Tasks;
using XamarinCRM.Extensions;

namespace XamarinCRM
{
    public class SalesDashboardLeadsViewModel : BaseViewModel
    {
        ICustomerDataClient _CustomerDataClient;

        Command loadSeedDataCommand;

        Command loadLeadsCommand;

        ObservableCollection<Account> _Leads;

        readonly Command _PushTabbedLeadPageCommand;
        public Command PushLeadDetailsTabbedPageCommand 
        { 
            get { return _PushTabbedLeadPageCommand; } 
        }

        public bool NeedsRefresh { get; set; }

        public SalesDashboardLeadsViewModel(Command pushTabbedLeadPageCommand, INavigation navigation = null) : base(navigation)
        {
            _PushTabbedLeadPageCommand = pushTabbedLeadPageCommand;

            _CustomerDataClient = DependencyService.Get<ICustomerDataClient>();

            Leads = new ObservableCollection<Account>();

            MessagingCenter.Subscribe<Account>(this, MessagingServiceConstants.SAVE_ACCOUNT, (account) =>
                {
                    var index = Leads.IndexOf(account);
                    if (index >= 0)
                    {
                        Leads[index] = account;
                    }
                    else
                    {
                        Leads.Add(account);
                    }
                    Leads = new ObservableCollection<Account>(Leads.OrderBy(l => l.Company));
                });

            IsInitialized = false;
        }

        public Command LoadSeedDataCommand
        {
            get
            {
                return loadSeedDataCommand ?? (loadSeedDataCommand = new Command(async () => await ExecuteLoadSeedDataCommand()));
            }
        }

        /// <summary>
        /// Used for pull-to-refresh of Leads list
        /// </summary>
        /// <value>The load leads command, used for pull-to-refresh.</value>
        public Command LoadLeadsCommand
        { 
            get
            { 
                return loadLeadsCommand ?? (loadLeadsCommand = new Command(ExecuteLoadLeadsCommand, () => !IsBusy)); 
            } 
        }

        public async Task ExecuteLoadSeedDataCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await _CustomerDataClient.SeedData();

            var leads = await _CustomerDataClient.GetAccountsAsync(true);
            Leads = leads.ToObservableCollection();

            IsBusy = false;
        }

        /// <summary>
        /// Executes the LoadLeadsCommand.
        /// </summary>
        async void ExecuteLoadLeadsCommand()
        { 
            if (IsBusy)
                return; 

            IsBusy = true;
            LoadLeadsCommand.ChangeCanExecute(); 

            Leads.Clear();
            Leads.AddRange(await _CustomerDataClient.GetAccountsAsync(true));

            IsBusy = false;
            LoadLeadsCommand.ChangeCanExecute(); 
        }

        public ObservableCollection<Account> Leads
        {
            get { return _Leads; }
            set
            {
                _Leads = value;
                OnPropertyChanged("Leads");
            }
        }
    }
}


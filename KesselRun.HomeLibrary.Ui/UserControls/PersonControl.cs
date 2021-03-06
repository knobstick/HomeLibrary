﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using KesselRun.HomeLibrary.Ui.Core;
using KesselRun.HomeLibrary.Ui.CustomControls;
using KesselRun.HomeLibrary.Ui.CustomControls.EventArgs;
using KesselRun.HomeLibrary.Ui.Forms;
using KesselRun.HomeLibrary.UiLogic.EventArgs;
using KesselRun.HomeLibrary.UiLogic.Presenters;
using KesselRun.HomeLibrary.UiLogic.Views;
using System;
using KesselRun.HomeLibrary.Ui.Assets.Resources;
using KesselRun.HomeLibrary.Ui.Messaging;
using KesselRun.HomeLibrary.UiModel;
using KesselRun.HomeLibrary.UiModel.Models;
using KesselRun.HomeLibrary.UiModel.ViewModels;
using WinFormsMvp;
using WinFormsMvp.Binder;
using WinFormsMvp.Forms;
using WinFormsMvp.Messaging;

namespace KesselRun.HomeLibrary.Ui.UserControls
{
    [PresenterBinding(typeof(PeoplePresenter))]
    public partial class PersonControl : MvpUserControl, IPeopleView, IHydrateOnFocus
    {
        private readonly Navigator _navigator = Navigator.SingleNavigator;
        private readonly Lazy<MainForm> _mainWindow;
        private int NoSelectedItem = -1;

        public PersonControl()
        {
            _mainWindow = new Lazy<MainForm>(() => (MainForm)ParentForm, LazyThreadSafetyMode.None);

            InitializeComponent();

            dgvPeople.AutoGenerateColumns = false;
        }

        public event EventHandler ViewClosing;
        public event EventHandler CloseControl;
        public event EventHandler<SearchPeopleEventArgs> ReloadView;


        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            PresenterBinder.MessageBus.Register(
                this,
                MessageIds.SearchPeopleMessage,
                new Action<GenericMessage<SearchPeopleViewModel>>(GetResultSetWithNewSearchParameters)
                );
            PresenterBinder.MessageBus.Register(
                this,
                MessageIds.GetFilterParametersPeopleResponse,
                new Action<GenericMessage<SearchPeopleViewModel>>(GetResultSetWithNewSearchParameters)
                );
        }

        public void CloseView()
        {
            ViewClosing(this, System.EventArgs.Empty);
        }

        public void LogEventToView(UiModel.LogEvent logEvent)
        {
            _mainWindow.Value.MainViewModel.MainViewLogItems.Add(logEvent);
        }

        public void HydrateWithDataOnFocus()
        {
            //var SearchPeopleViewModel = GetSearchParameters(NoSelectedItem);
            GetResultSetWithNewSearchParameters(new GenericMessage<SearchPeopleViewModel>(new SearchPeopleViewModel
            {
                FilterMetaDataList = new List<FilterMetaData>
                {
                    new FilterMetaData
                        {
                            FilterBy = "LastName",
                            FilterValue = ""
                        }
                }
            }));

            //ReSyncGridAndPager();

            dgvPager.AdjustPreviousNextButtons(Constants.StartUp);
        }

        private void dgvPager_PageSizeChanged(object sender, PagedEventArgs e)
        {
            var selectedGridLending = GetSelectedPerson();

            if (!ReferenceEquals(selectedGridLending, null))
            {
                var selectedGridLendingId = selectedGridLending.Id;

                GetSearchParameters(selectedGridLendingId);
            }

            //ReSyncGridAndPager();

            dgvPager.AdjustPreviousNextButtons(e.EventRaised);
        }

        private void dgvPager_PageChanged(object sender, PagedEventArgs e)
        {
            var selectedGridLending = GetSelectedPerson();
            int selectedLendingId = -1;

            if (ReferenceEquals(null, selectedGridLending))
            {
                selectedLendingId = NoSelectedItem;
            }
            else
            {
                selectedLendingId = selectedGridLending.Id;
            }

            var SearchPeopleViewModel = GetSearchParameters(selectedLendingId);

            //ReSyncGridAndPager();

            dgvPager.AdjustPreviousNextButtons(e.EventRaised);
        }

        private void dgvPeople_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            int columnIndex = e.ColumnIndex;

            if (columnIndex > -1)
            {

                var columnClicked = dgvPeople.Columns[e.ColumnIndex];

                //  if less than 0, must be the Header column.
                if (rowIndex < 0)
                {
                    if (columnClicked.SortMode == DataGridViewColumnSortMode.Programmatic)
                    {
                        SortOutSorting(columnClicked);

                        var selectedGridLending = GetSelectedPerson();
                        var selectedLendingId = selectedGridLending.Id;

                        var SearchPeopleViewModel = GetSearchParameters(selectedLendingId);

                        //ReSyncGridAndPager();
                    }
                }
                else
                {
                    if (PresenterBinder.ApplicationState.HasItem(Constants.SelectedGridLending))
                        PresenterBinder.ApplicationState.RemoveItem<int>(Constants.SelectedGridLending);
                }
            }
        }

        private Person GetSelectedPerson()
        {
            Person selectedPerson = default(Person);
            int selectedRowIndex = -1;

            if (!ReferenceEquals(null, dgvPeople.CurrentCell))
            {
                selectedRowIndex = dgvPeople.CurrentCell.RowIndex;
                selectedPerson = dgvPeople.Rows[selectedRowIndex].DataBoundItem as Person;

                if (selectedPerson != null)
                {
                    if (!PresenterBinder.ApplicationState.HasItem(Constants.SelectedGridPeople))
                        PresenterBinder.ApplicationState.AddItem(Constants.SelectedGridPeople, selectedPerson.Id);
                }
            }
            return selectedPerson;
        }

        private static SearchPeopleViewModel GetSearchParameters(int selectedGridPersonId)
        {
            var SearchPeopleViewModel = new SearchPeopleViewModel
            {
                SelectedGridPersonId = selectedGridPersonId
            };

            PresenterBinder.MessageBus.Send(
                new GenericMessage<SearchPeopleViewModel>(SearchPeopleViewModel),
                MessageIds.GetFilterParametersRequestPeople
                );

            return SearchPeopleViewModel;
        }

        private void ReSyncGridAndPager()
        {
            dgvPeople.DataSource = PeopleViewModel.People;

            dgvPager.PageCount = PeopleViewModel.PagerData.NumberOfPages;
            dgvPager.PageIndex = PeopleViewModel.PagerData.PageNumber;
            dgvPager.PageSize = PeopleViewModel.PagerData.PageSize;
            dgvPager.SortOrder = PeopleViewModel.PagerData.SortOrder;
        }

        private void SortOutSorting(DataGridViewColumn columnClicked)
        {

            if (dgvPeople.Tag == columnClicked.DataPropertyName)
            //if (dgvPager.SortByColumn == columnClicked.DataPropertyName)
            {
                dgvPager.SortOrder = dgvPager.SortOrder == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                dgvPager.SortByColumn = GetSortByParameter(columnClicked.DataPropertyName);
                dgvPager.SortOrder = ListSortDirection.Ascending;
            }

            dgvPeople.Tag = columnClicked.DataPropertyName;
        }

        private string GetSortByParameter(string dataPropertyName)
        {

            return "LastName";
            switch (dataPropertyName)
            {
                //case "Email":
                //    return "Borrower.Email";
                //case "Borrower":
                //    return "Borrower.Email";
                //case "Title":
                //    return "Book.Title";
                //case "DateLent":
                //    return "DateLent";
                default:
                    throw new NotImplementedException("Column not added yet. WIP.");
            }
        }

        private void GetResultSetWithNewSearchParameters(GenericMessage<SearchPeopleViewModel> message)
        {
            ReloadView(this, new SearchPeopleEventArgs(
                message.Content.FilterMetaDataList,
                message.Content.Operation,
                dgvPager.PageSize,
                dgvPager.PageIndex,
                dgvPager.SortByColumn,
                dgvPager.SortOrder
                )//message.Content.SelectedGridLendingId)
                );

            ReSyncGridAndPager();

            dgvPager.AdjustPreviousNextButtons(DataGridViewPager.PageSizeChangeSubmittedEvent);
        }

        public PeopleViewModel PeopleViewModel { get; set; }

        private void dgvPeople_CellFormatting(object sender, DataGridViewCellFormattingEventArgs dataGridViewCellFormattingEventArgs)
        {
            int rowIndex = dataGridViewCellFormattingEventArgs.RowIndex;
            int columnIndex = dataGridViewCellFormattingEventArgs.ColumnIndex;

            dgvPeople.Rows[rowIndex].Cells[columnIndex].ToolTipText = "Is Author";

            switch (dgvPeople.Columns[columnIndex].Name)
            {
                case "dgvtcIsAuthor":
                    bool isAuthor;

                    if (bool.TryParse(dgvPeople.Rows[dataGridViewCellFormattingEventArgs.RowIndex].Cells["dgvtcIsAuthor"].Value.ToString(),
                        out isAuthor))
                    {
                        dataGridViewCellFormattingEventArgs.Value = ImageResources.Tick;
                    }
                    else
                    {
                        dataGridViewCellFormattingEventArgs.Value = ImageResources.Cross;
                    }
                    break;
            }
        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);

            PresenterBinder.MessageBus.Unregister<GenericMessage<SearchPeopleViewModel>>(this, MessageIds.SearchPeopleMessage);
            PresenterBinder.MessageBus.Unregister<GenericMessage<SearchPeopleViewModel>>(this, MessageIds.GetFilterParametersPeopleResponse);

        }
    }
}

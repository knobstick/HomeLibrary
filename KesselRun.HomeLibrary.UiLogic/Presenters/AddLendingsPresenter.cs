﻿using System;
using System.ComponentModel;
using KesselRun.HomeLibrary.Service.Commands;
using KesselRun.HomeLibrary.Service.Infrastructure;
using KesselRun.HomeLibrary.Service.Queries;
using KesselRun.HomeLibrary.UiLogic.Views;
using KesselRun.HomeLibrary.UiLogic.Views.ViewModels;
using KesselRun.HomeLibrary.UiModel.Models;
using WinFormsMvp;

namespace KesselRun.HomeLibrary.UiLogic.Presenters
{
    public class AddLendingsPresenter : Presenter<IAddLendingsView>, IDisposable
    {
        private readonly ICommandProcessor _commandProcessor;
        private readonly IQueryProcessor _queryProcessor;

        public AddLendingsPresenter(IAddLendingsView view, ICommandProcessor commandProcessor, IQueryProcessor queryProcessor) : base(view)
        {
            _commandProcessor = commandProcessor;
            _queryProcessor = queryProcessor;
            View.ViewClosing += View_ViewClosing;
            View.Close += view_Close;
            View.Load += View_Load;
            View.AddNewLending += View_AddNewLending;
        }

        void View_Load(object sender, System.EventArgs e)
        {
            var getPeopleSortedQuery = new GetPeopleSortedQuery {SortBy = "id"};
            var getBooksSorted = new GetBooksSorted {SortBy = "id"};

            var addLendingViewModel = new AddLendingViewModel
            {
                Books = new BindingList<Book>(_queryProcessor.Process(getBooksSorted)),
                People = new BindingList<Person>(_queryProcessor.Process(getPeopleSortedQuery))
            };

            View.AddLendingViewModel = addLendingViewModel;
        }

        void View_AddNewLending(object sender, EventArgs.AddLendingEventArgs e)
        {
            var addLendingCommand = new AddLendingCommand
            {
                BookId = e.bookId,
                BorrowerId = e.borrowerId,
                DateLent = e.dateLent,
                DateDue = e.dateDue
            };

            _commandProcessor.Execute(addLendingCommand);
        }

        void View_ViewClosing(object sender, System.EventArgs e)
        {
            View.ReleasePresenter(this);
        }

        void view_Close(object sender, System.EventArgs e)
        {
            View.CloseView();
        }

        public void Dispose()
        {
            //  to implement
        }
    }
}

﻿using System;
using KesselRun.HomeLibrary.Service.Infrastructure;

namespace KesselRun.HomeLibrary.Service.Commands
{
    public class AddLendingCommand
    {
        public int BookId { get; set; }
        public int BorrowerId { get; set; }
        public DateTime DateLent { get; set; }
        public DateTime? DateDue { get; set; }
    }
}

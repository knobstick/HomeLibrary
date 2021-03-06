﻿using System;

namespace KesselRun.HomeLibrary.UiModel.Models
{
    public class Lending
    {
        public int Id { get; set; }
        
        public DateTime DateLent { get; set; }
        public DateTime? DueDate { get; set; }
        public int Duration { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Title { get; set; }
    }
}

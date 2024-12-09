﻿using RightVisionBotDb.Text.Interfaces;

namespace RightVisionBotDb.Text.Sections
{
    public class HoursFormat : ITimeFormat
    {
        public string Singular { get; set; }
        public string Plural { get; set; }
        public string Genitive { get; set; }
    }
}
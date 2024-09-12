﻿using RightVisionBotDb.Enums;

namespace RightVisionBotDb.Models
{
    public class RightVisionDbProperties
    {
        public int Id { get; set; }
        public RightVisionStatus RightVisionStatus { get; set; } = RightVisionStatus.Relevant;
        public EnrollmentStatus EnrollmentStatus { get; set; } = EnrollmentStatus.Closed;
    }
}
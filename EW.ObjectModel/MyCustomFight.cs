﻿using System;

namespace EW.ObjectModel
{
    public class MyCustomFight : AMyFight
    {
        readonly public MyResourses Prize;

        public string Description { get; set; }

        public MyCustomFight(string attackersTag, string defendersTag, DateTime startTime, MyResourses prize) : base(attackersTag, defendersTag, startTime) => Prize = prize;
    }
}

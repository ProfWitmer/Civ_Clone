using System;
using System.Collections.Generic;

namespace CivClone.Simulation.Core
{
    public sealed class TurnSystem
    {
        private readonly List<YearStepRule> yearSteps = new()
        {
            new YearStepRule(-4000, -1000, 20),
            new YearStepRule(-1000, -500, 10),
            new YearStepRule(-500, 0, 5),
            new YearStepRule(0, 1800, 2),
            new YearStepRule(1800, 1950, 1),
            new YearStepRule(1950, 9999, 1)
        };

        private int currentTurn = 1;
        private int currentYear = -4000;

        public int CurrentTurn => currentTurn;
        public int CurrentYear => currentYear;
        public string CurrentYearLabel => currentYear < 0 ? $"{-currentYear} BC" : $"{currentYear} AD";

        public void AdvanceTurn()
        {
            currentTurn++;
            currentYear += GetYearStep(currentYear);
        }

        private int GetYearStep(int year)
        {
            foreach (YearStepRule rule in yearSteps)
            {
                if (year >= rule.StartYear && year < rule.EndYear)
                {
                    return rule.Step;
                }
            }

            return 1;
        }

        private readonly struct YearStepRule
        {
            public int StartYear { get; }
            public int EndYear { get; }
            public int Step { get; }

            public YearStepRule(int startYear, int endYear, int step)
            {
                if (endYear <= startYear)
                {
                    throw new ArgumentException("End year must be greater than start year.");
                }

                StartYear = startYear;
                EndYear = endYear;
                Step = step;
            }
        }
    }
}

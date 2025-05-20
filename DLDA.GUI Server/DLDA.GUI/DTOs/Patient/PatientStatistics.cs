namespace DLDA.GUI.DTOs.Patient
{
    // DTO för patientens statistik i en bedömning
    public class PatientStatistics
    {
        public int AssessmentId { get; set; } // ID för bedömningen

        public DateTime CreatedAt { get; set; } // Datum och tid då bedömningen skapades

        public List<PatientAnswerStatsDto> Answers { get; set; } = new(); // Lista med statistik för varje svar
    }

    // DTO för statistik över patientens svar på en fråga
    public class PatientAnswerStatsDto
    {
        public int QuestionId { get; set; } // ID för frågan

        public string QuestionText { get; set; } = string.Empty; // Text för frågan, som är tom som standard

        public int? Answer { get; set; } // Patientens svar på frågan
    }

    // DTO för översikt över förändringar i patientens bedömning över tid
    public class PatientChangeOverviewDto
    {
        public List<ImprovementDto> Förbättringar { get; set; } = new(); // Lista med förbättringar

        public DateTime PreviousDate { get; set; } // Datum för föregående bedömning

        public DateTime CurrentDate { get; set; } // Datum för aktuell bedömning

        // Automatisk beräkning: hur många färre frågor hoppades över?
        public int FärreHoppadeFrågor =>
            Förbättringar.Count(f => f.SkippedPrevious) - Förbättringar.Count(f => f.SkippedCurrent);

        // Unika kategorier med förbättringar
        public List<string> FörbättradeKategorier =>
            Förbättringar
                .Where(f => !string.IsNullOrWhiteSpace(f.Category))
                .Select(f => f.Category)
                .Distinct()
                .OrderBy(k => k)
                .ToList();
    }


    // DTO för förbättring i en fråga mellan två bedömningar
    public class ImprovementDto
    {
        public string? Question { get; set; } = string.Empty; // Text för frågan, som är tom som standard

        public int Previous { get; set; } // Resultat för föregående bedömning

        public int Current { get; set; } // Resultat för aktuell bedömning

        public int Change => Previous - Current; // Förändring i resultat mellan föregående och aktuell bedömning

        public string Category { get; set; } = string.Empty; // Kategori för frågan, som är tom som standard

        public int QuestionId { get; set; } // ID för frågan

        // Hoppade över-data för både föregående och aktuell bedömning
        public bool SkippedPrevious { get; set; } // Indikerar om frågan hoppades över i föregående bedömning

        public bool SkippedCurrent { get; set; } // Indikerar om frågan hoppades över i aktuell bedömning
    }

    namespace DLDA.GUI.DTOs.Patient
    {
        // DTO för API-anrop av förbättring i en fråga mellan två bedömningar
        public class ImprovementApiDto
        {
            public string Question { get; set; } = string.Empty; // Text för frågan, som är tom som standard

            public int Previous { get; set; } // Resultat för föregående bedömning

            public int Current { get; set; } // Resultat för aktuell bedömning

            public int Change => Previous - Current; // Förändring i resultat mellan föregående och aktuell bedömning

            public string Category { get; set; } = string.Empty; // Kategori för frågan, som är tom som standard

            public bool SkippedPrevious { get; set; } // Indikerar om frågan hoppades över i föregående bedömning

            public bool SkippedCurrent { get; set; } // Indikerar om frågan hoppades över i aktuell bedömning

            public int QuestionID { get; set; } // ID för frågan
        }
    }
}

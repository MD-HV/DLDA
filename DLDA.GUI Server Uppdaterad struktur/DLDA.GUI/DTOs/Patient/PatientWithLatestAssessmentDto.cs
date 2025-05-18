namespace DLDA.GUI.DTOs.Patient
{
    // Hämta visa användarnamn och senaste bedömningens datum till personal
    public class PatientWithLatestAssessmentDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = "";
        public DateTime? LastAssessmentDate { get; set; }
    }

}

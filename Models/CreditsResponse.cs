namespace ReelTime.Models
{
    public class CreditsResponse
    {
        public int Id { get; set; }
        public List<Cast> Cast { get; set; } = [];
        public List<Crew> Crew { get; set; } = [];
    }

}

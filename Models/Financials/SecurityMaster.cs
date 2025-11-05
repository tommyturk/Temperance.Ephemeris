namespace Temperance.Ephemeris.Models.Financials
{
    public class SecurityMaster
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Exchange { get; set; }
        public string AssetType { get; set; }
        public string Status { get; set; }
        public DateTime IpoDate{ get; set; }
        public DateTime? DelistingDate { get; set; }
        public int DataProviderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace CoreBookingPlatform.AdapterService.Adapters.AbcAdapter
{
    public class AbcAvailabilityDto
    {
        public bool IsAvailable { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

         
    }
}



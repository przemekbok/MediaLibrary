namespace MediaLibrary.Models
{
    public class MediaTag
    {
        public int MediaId { get; set; }
        public Media Media { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
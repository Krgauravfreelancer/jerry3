using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace Sqllite_Library.Models
{
    public class CBVHlsts
    {
        public int hlsts_id { get; set; }
        public int fk_hlsts_project { get; set; }
        public int hlsts_version { get; set; }
        public string hlsts_comments { get; set; }
        public byte[] hlsts_media480 { get; set; }
        public byte[] hlsts_media720 { get; set; }
        public byte[] hlsts_media1080 { get; set; }
        public byte[] hlsts_media1280 { get; set; }
        public byte[] hlsts_master { get; set; }
        public byte[] hlsts_encryption { get; set; }
        public DateTime hlsts_createdate { get; set; }
        public DateTime hlsts_modifydate { get; set; }
        public List<CBVStreamts> streamts_data { get; set; }
        public CBVHlsts()
        {
        }

        public override string ToString()
        {
            return $"{hlsts_id} \t {fk_hlsts_project} [projectId] \t {hlsts_version} [version] \t {hlsts_comments} [comments] " +
                $"\t {hlsts_media480.Length} [media480]\t {hlsts_media720.Length} [media720]\t {hlsts_media1080.Length} [media1080]\t {hlsts_media1280.Length} [media1280]" +
                $"\t {hlsts_master.Length} [master]\t {hlsts_encryption.Length} [encryption]";
        }
    }
}

using System;

namespace Sqllite_Library.Models
{
    public class CBVFinalMp4
    {
        public int finalmp4_id { get; set; }
        public int fk_finalmp4_project { get; set; }
        public int finalmp4_version { get; set; }
        public string finalmp4_comments { get; set; }
        public byte[] finalmp4_media { get; set; }
        public DateTime finalmp4_createdate { get; set; }
        public DateTime finalmp4_modifydate { get; set; }
        public bool finalmp4_isdeleted { get; set; }
        public CBVFinalMp4()
        {
        }

        public override string ToString()
        {
            return $"{finalmp4_id} \t {fk_finalmp4_project} [projectId] \t {finalmp4_version} [version] \t {finalmp4_comments} [comments] \t {finalmp4_media.Length} [media]";
        }
    }
}

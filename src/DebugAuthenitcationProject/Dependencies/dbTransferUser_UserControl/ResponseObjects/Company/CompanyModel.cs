using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.Company
{
    public class CompanyModel
    {
        [JsonPropertyName("company_id")]
        public int company_id { get; set; }

        [JsonPropertyName("company_name")]
        public string company_name { get; set; }
        
        public override string ToString()
        {
            return $@"company_id - {company_id}, company_name - {company_name} \n\n";
        }
    }
}

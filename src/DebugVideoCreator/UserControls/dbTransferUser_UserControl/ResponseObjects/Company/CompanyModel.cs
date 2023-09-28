namespace dbTransferUser_UserControl.ResponseObjects.Company
{
    public class CompanyModel
    {
        public int company_id { get; set; }
        public string company_name { get; set; }
        
        public override string ToString()
        {
            return $@"company_id - {company_id}, company_name - {company_name} \n\n";
        }
    }
}

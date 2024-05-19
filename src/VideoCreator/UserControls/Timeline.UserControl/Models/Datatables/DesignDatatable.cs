using System;
using System.Data;
using Timeline.UserControls.Config;

namespace Timeline.UserControls.Models.Datatables
{
    public class DesignDatatable : DataTable
    {

        public DesignDatatable() {
            Build();
        }

        public void Build()
        {
            this.Clear();
            Columns.Add(nameof(TimelineDesign.design_id), typeof(int));
            Columns.Add(nameof(TimelineDesign.fk_design_videoevent), typeof(int));
            Columns.Add(nameof(TimelineDesign.fk_design_screen), typeof(int));
            Columns.Add(nameof(TimelineDesign.design_xml), typeof(string));
            Columns.Add(nameof(TimelineDesign.design_createdate), typeof(string));
            Columns.Add(nameof(TimelineDesign.design_modifydate), typeof(string));
        }


        public void AddDesign(int videoEventId, int screenId)
        {
            DataRow dRow = this.NewRow();

            dRow[nameof(TimelineDesign.design_id)] = -1;
            dRow[nameof(TimelineDesign.fk_design_videoevent)] = videoEventId;
            dRow[nameof(TimelineDesign.fk_design_screen)] = screenId;
            dRow[nameof(TimelineDesign.design_xml)] = ""; //setting empty for now
            dRow[nameof(TimelineDesign.design_createdate)] = DateTime.Now.ToString(TimelineDefaultConfig.DateTimeStringFormat);
            dRow[nameof(TimelineDesign.design_createdate)] = DateTime.Now.ToString(TimelineDefaultConfig.DateTimeStringFormat);

            this.Rows.Add(dRow);
        }
    }
}

#region NameSpaces
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using VirtualAdvocate.Common;
using VirtualAdvocate.DAL;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region CustomerHistoryController
    public class CustomerHistoryController : BaseController
    {
        #region Index
        // GET: CustomerHistory
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region CustomerHistory
        public ActionResult CustomerHistory(int id)
        {
            Session["CustHistoryID"] = id;
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            int userId = Convert.ToInt32(Session["UserId"]);
            try
            {
                var objFilledTemp = (from obj in VAEDB.FilledTemplateDetails
                                     join doc in VAEDB.DocumentTemplates on obj.TemplateId equals doc.TemplateId into g
                                     from subset in g.DefaultIfEmpty()
                                     where obj.UserId == userId && (obj.ArchiveStatus == null || obj.ArchiveStatus == false) && obj.CustomerId == id
                                     select new FilledFormDetailModel { DocumentTitle = (subset == null ? "Template Deleted" : subset.DocumentTitle), Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId }
                    );
                objForm = objFilledTemp.OrderByDescending(x => x.RowId).ToList();
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
            }

            //var clientID = db.SelectedAccountServices.Where(s => s.UserId == userId).FirstOrDefault();
            //ViewBag.ClientID =clientID.ServiceId;
            ViewBag.customerID = id;
            ViewBag.active = VAEDB.CustomerDetails.Where(c => c.CustomerId == id).FirstOrDefault().IsEnabled;

            return View("CustomerHistory", objForm);

        }
        #endregion

        #region ExportCustomerHistory
        public JsonResult ExportCustomerHistory()
        {
            var id = (int)Session["CustHistoryID"];
            List<FilledFormDetailModel> objForm = new List<FilledFormDetailModel>();
            int userId = Convert.ToInt32(Session["UserId"]);
            try
            {

                var objFilledTemp = (from obj in VAEDB.FilledTemplateDetails
                                     join doc in VAEDB.DocumentTemplates on obj.TemplateId equals doc.TemplateId into g
                                     from subset in g.DefaultIfEmpty()
                                     where obj.UserId == userId && (obj.ArchiveStatus == null || obj.ArchiveStatus == false) && obj.CustomerId == id
                                     select new FilledFormDetailModel { DocumentTitle = (subset == null ? "Template Deleted" : subset.DocumentTitle), Amount = obj.Amount, CreatedDate = obj.CreatedDate, FilledTemplateName = obj.FilledTemplateName, GroupId = obj.GroupId, RowId = obj.RowId }
                    );
                objForm = objFilledTemp.OrderByDescending(x => x.RowId).ToList();
                var customerName = VAEDB.CustomerDetails.Where(c => c.CustomerId == id).FirstOrDefault().CustomerName;
                ListToDataTable objTable = new ListToDataTable();
                System.Data.DataTable dt = objTable.ToDataTable(objForm);
                dt.Columns.Remove("FilledTemplateName");
                dt.Columns.Remove("PaidStatus");
                dt.Columns.Remove("GroupId");
                dt.Columns.Remove("UserId");
                dt.Columns.Remove("OrgId");
                dt.Columns.Remove("CustomerName");
                if (dt.Rows.Count > 0)
                {
                    string filename = "CustomerHistory.xls";
                    System.IO.StringWriter tw = new System.IO.StringWriter();
                    System.Web.UI.HtmlTextWriter hw = new System.Web.UI.HtmlTextWriter(tw);
                    hw.Write("<table><tr><td colspan='3'>CustomerDetails-</td></tr>");
                    hw.Write("<table><tr><td colspan='3'>Name:" + customerName + "</td></tr>");
                    DataGrid dgGrid = new DataGrid();
                    dgGrid.DataSource = dt;
                    dgGrid.DataBind();

                    //Get the HTML for the control.
                    dgGrid.RenderControl(hw);
                    //Write the HTML back to the browser.
                    //Response.ContentType = application/vnd.ms-excel;
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename + "");
                    Response.Write(tw.ToString());
                    Response.End();
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLog.LogThisError(ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        } 
        #endregion
    } 
    #endregion
} 
#endregion
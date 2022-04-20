#region NameSpaces
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using VirtualAdvocate.Models;
#endregion
#region VirtualAdvocate.Controllers
namespace VirtualAdvocate.Controllers
{
    #region BaseController
    public class BaseController : Controller
    {
        #region Global Variables
        private VirtualAdvocateEntities _db; 
        #endregion

        #region OnActionExecuting
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (Session["UserID"] == null)
            //{

            //    var url = new UrlHelper(filterContext.RequestContext);
            //    var loginUrl = url.Content("~/Login/Index");
            //    filterContext.Result = new RedirectResult(loginUrl);
            //    return;

            //}
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            var user = session["UserID"];

            if (((user == null) && (!session.IsNewSession)) || (session.IsNewSession))
            {
                //send them off to the login page
                var url = new UrlHelper(filterContext.RequestContext);
                var loginUrl = url.Content("~/Login/Index");
                session.RemoveAll();
                session.Clear();
                session.Abandon();
                filterContext.HttpContext.Response.Redirect(loginUrl, true);
            }
            //string actionName = filterContext.ActionDescriptor.ActionName;

            //if (filterContext.HttpContext.Request.IsAjaxRequest() && actionName.Equals("Login"))
            //{
            //    JsonResult jsonRes = new JsonResult();
            //    jsonRes.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //    jsonRes.Data = "Session timeout. Redirecting...";

            //    filterContext.Result = jsonRes;
            //}

            base.OnActionExecuting(filterContext);
        }
        #endregion

        #region VAEDB
        public VirtualAdvocateEntities VAEDB
        {
            get
            {
                if (_db == null)
                {
                    _db = new VirtualAdvocateEntities();
                }
                return _db;
            }
        }
        #endregion

        #region ReplaceSpireDocText
        internal void ReplaceSpireDocText(TextRangeLocation location, IList<Paragraph> replacement)
        {
            //will be replaced
            TextRange textRange = location.Text;

            //textRange index
            int index = location.Index;

            //owener paragraph
            Paragraph paragraph = location.Owner;

            //owner text body
            Body sectionBody = paragraph.OwnerTextBody;

            //get the index of paragraph in section
            int paragraphIndex = sectionBody.ChildObjects.IndexOf(paragraph);

            int replacementIndex = -1;
            if (index == 0)
            {
                //remove
                paragraph.ChildObjects.RemoveAt(0);

                replacementIndex = sectionBody.ChildObjects.IndexOf(paragraph);
            }
            else if (index == paragraph.ChildObjects.Count - 1)
            {
                paragraph.ChildObjects.RemoveAt(index);
                replacementIndex = paragraphIndex + 1;
            }
            else
            {

                //split owner paragraph
                Paragraph paragraph1 = (Paragraph)paragraph.Clone();
                while (paragraph.ChildObjects.Count > index)
                {
                    paragraph.ChildObjects.RemoveAt(index);
                }
                for (int i = 0, count = index + 1; i < count; i++)
                {
                    paragraph1.ChildObjects.RemoveAt(0);
                }
                sectionBody.ChildObjects.Insert(paragraphIndex + 1, paragraph1);

                replacementIndex = paragraphIndex + 1;
            }

            //insert replacement
            for (int i = 0; i < replacement.Count; i++)
            {
                sectionBody.ChildObjects.Insert(replacementIndex + i, replacement[i].Clone());
            }
        }
        #endregion


    }
    #endregion

    #region TextRangeLocation
    internal class TextRangeLocation
    {
        #region TextRangeLocation
        public TextRangeLocation(TextRange text)
        {
            this.Text = text;
        }
        #endregion

        public TextRange Text { get; set; }

        public Paragraph Owner
        {
            get
            {
                return this.Text.OwnerParagraph;
            }
        }

        public int Index
        {
            get
            {
                return this.Owner.ChildObjects.IndexOf(this.Text);
            }
        }

        public int CompareTo(TextRangeLocation other)
        {
            return -(this.Index - other.Index);
        }
    } 
    #endregion
} 
#endregion
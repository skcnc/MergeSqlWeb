using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebToolsBox.Controllers
{
    public class MergeSQLBizController : Controller
    {
        //
        // GET: /MergeSQLBiz/

        public ActionResult Index()
        {
            return View();
        }

        //
        // POST: /MergeSQLBiz/

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration=120)]
        public ActionResult Index(string boo)
        {
            //if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            //{
            //    return RedirectToLocal(returnUrl);
            //}

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            //ModelState.AddModelError("", "提供的用户名或密码不正确。");

            QueryEntity model = new QueryEntity();

            CalCode caculator = new CalCode();
            if (caculator.GetSQL(Request.Form["searchText"], ref model).Key == 0)
            {
                return View(model);
            }
            else
            {
                return View();
            }

        }

    }
}

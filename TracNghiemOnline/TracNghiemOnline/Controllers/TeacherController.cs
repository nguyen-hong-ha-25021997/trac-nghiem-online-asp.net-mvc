using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TracNghiemOnline.Common;
using TracNghiemOnline.Models;
namespace TracNghiemOnline.Controllers
{
    public class TeacherController : Controller
    {
        User user = new User();
        TeacherDA Model = new TeacherDA();
        // GET: Teacher
        public ActionResult Index()
        {
            if (!user.IsTeacher())
                return View("Error");
            Model.UpdateLastLogin();
            Model.UpdateLastSeen("Trang Chủ", Url.Action("Index"));
            return View(Model.GetListTest());
        }
        public ActionResult EditTeacher()
        {
            if (!user.IsTeacher())
                return View("Error");
            int id_teacher = Convert.ToInt32(user.ID);
            try
            {
                teacher teacher = Model.GetTeacher(id_teacher);
                Model.UpdateLastSeen("Sửa Giảng Viên " + teacher.name, Url.Action("EditTeacher/" + user.ID));
                ViewBag.ListSpecialities = Model.GetSpecialities();
                return View(teacher);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditTeacher(FormCollection form)
        {
            if (!user.IsTeacher())
                return View("Error");
            int id_teacher = Convert.ToInt32(form["id_teacher"]);
            string name = form["name"];
            string username = form["username"];
            string password = form["password"];
            string email = form["email"];
            string gender = form["gender"];
            string birthday = form["birthday"];
            int id_speciality = Convert.ToInt32(form["id_speciality"]);
            bool edit = Model.EditTeacher(id_teacher, name, username, password, gender, email, birthday, id_speciality);
            if (edit)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Sửa Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Sửa Thất Bại";
            }
            return RedirectToAction("EditTeacher/" + id_teacher);
        }
        public ActionResult Preview(int id)
        {
            if (!user.IsTeacher())
                return View("Error");
            var list = Model.GetListScore(id);
            ViewBag.test_code = id;
            ViewBag.total = list.Count;
            return View(list);
        }
        public ActionResult Logout()
        {
            if (!user.IsTeacher())
                return View("Error");
            user.Reset();
            return RedirectToAction("Index", "Login");
        }
    }
}
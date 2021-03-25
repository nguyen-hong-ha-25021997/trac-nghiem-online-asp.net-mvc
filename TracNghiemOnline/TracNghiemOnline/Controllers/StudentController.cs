using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TracNghiemOnline.Models;
using TracNghiemOnline.Common;
namespace TracNghiemOnline.Controllers
{
    public class StudentController : Controller
    {
        User user = new User();
        StudentDA Model = new StudentDA();
        // GET: Student
        public ActionResult Index()
        {
            if (!user.IsStudent())
                return View("Error");
            if (user.IsTesting())
                return RedirectToAction("DoingTest");
            Model.UpdateLastLogin();
            Model.UpdateLastSeen("Trang Chủ", Url.Action("Index"));
            ViewBag.score = Model.GetStudentTestcode();
            return View(Model.GetDashboard());
        }
        [HttpPost]
        public ActionResult CheckPassword(FormCollection form)
        {
            if (!user.IsStudent())
                return View("Error");
            if (user.IsTesting())
                return RedirectToAction("DoingTest");
            string test_code = form["test_code"];
            int code = Convert.ToInt32(test_code);
            string password = Encryptor.MD5Hash(form["password"]);
            string test_password = Model.GetTest(code).password;
            if (!password.Equals(test_password))
            {
                TempData["status_id"] = false;
                TempData["status"] = "Mật khẩu không đúng!";
                return RedirectToAction("Index");
            } else
            {
                Model.CreateStudentQuestion(code);
                Model.UpdateStatus(code, Model.GetTest(code).time_to_do + ":00");
                return RedirectToAction("DoingTest");
            }
        }
        public ActionResult DoingTest()
        {
            if (!user.IsStudent())
                return View("Error");
            if (!user.IsTesting())
                return View("Error");
            if(user.TIME != null)
            {
                string[] time = Regex.Split(user.TIME, ":");
                ViewBag.min = time[0];
                ViewBag.sec = time[1];
            }
            return View(Model.GetListQuest(user.TESTCODE));
        }
        public ActionResult SubmitTest()
        {
            if (!user.IsStudent())
                return View("Error");
            if (!user.IsTesting())
                return View("Error");
            var list = Model.GetListQuest(user.TESTCODE);
            int total_quest = list.First().test.total_questions;
            int test_code = list.First().test.test_code;
            double coefficient = 10.0 / (double)total_quest;
            int count_correct = 0;
            foreach (var item in list)
            {
                if (item.student_test.student_answer != null && item.student_test.student_answer.Trim().Equals(item.question.correct_answer.Trim()))
                    count_correct++;
            }
            double score = coefficient * count_correct;
            string detail = count_correct + "/" + total_quest;
            Model.InsertScore(score, detail);
            Model.FinishTest();
            return RedirectToAction("PreviewTest/" + test_code);
        }
        public ActionResult PreviewTest(int id)
        {
            if (!user.IsStudent())
                return View("Error");
            if (user.IsTesting())
                return RedirectToAction("DoingTest");
            if(Model.GetStudentTestcode().IndexOf(id) == -1)
                return View("Error");
            ViewBag.score = Model.GetScore(id);
            return View(Model.GetListQuest(id));
        } 
        [HttpPost]
        public void UpdateTiming(FormCollection form)
        {
            string min = form["min"];
            string sec = form["sec"];
            string time = min + ":" + sec;
            Model.UpdateTiming(time);
        }
        [HttpPost]
        public void UpdateStudentTest(FormCollection form)
        {
            int id_quest = Convert.ToInt32(form["id"]);
            string answer = form["answer"];
            answer = answer.Trim();
            string time = form["min"] + ":" + form["sec"];
            Model.UpdateStudentTest(id_quest, answer);
            Model.UpdateTiming(time);
        }
        public ActionResult EditStudent()
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_student = Convert.ToInt32(user.ID);
            try
            {
                student student = Model.GetStudent(id_student);
                Model.UpdateLastSeen("Sửa Sinh Viên " + student.name, Url.Action("EditStudent/" + user.ID));
                ViewBag.ListSpecialities = Model.GetSpecialities();
                ViewBag.ListClass = Model.GetClasses();
                return View(student);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditStudent(FormCollection form)
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_student = Convert.ToInt32(form["id_student"]);
            string name = form["name"];
            string username = form["username"];
            string password = form["password"];
            string email = form["email"];
            string gender = form["gender"];
            string birthday = form["birthday"];
            int id_speciality = Convert.ToInt32(form["id_speciality"]);
            int id_class = Convert.ToInt32(form["id_class"]);
            bool edit = Model.EditStudent(id_student, name, username, password, gender, email, birthday, id_speciality, id_class);
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
            return RedirectToAction("EditStudent/" + id_student);
        }
        public ActionResult Logout()
        {
            if (!user.IsStudent())
                return View("Error");
            user.Reset();
            return RedirectToAction("Index", "Login");
        }
    }
}
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
        public ActionResult CheckPassword(FormCollection form)                                  // ktra mk bài thi
        {
            if (!user.IsStudent())
                return View("Error");
            if (user.IsTesting())
                return RedirectToAction("DoingTest");                                           // trả về view làm bài thi
            string test_code = form["test_code"];
            int code = Convert.ToInt32(test_code);
            string password = Encryptor.MD5Hash(form["password"]);
            string test_password = Model.GetTest(code).password;
            if (!password.Equals(test_password))                                                // ktra mật khẩu
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
        public ActionResult DoingTest()                                                         // làm bài thi
        {
            if (!user.IsStudent())
                return View("Error");
            if (!user.IsTesting())
                return View("Error");
            if(user.TIME != null)                                                               // nếu còn thời gian thi
            {
                // truyền vào view để hiển thị
                string[] time = Regex.Split(user.TIME, ":");
                ViewBag.min = time[0];
                ViewBag.sec = time[1];
            }
            return View(Model.GetListQuest(user.TESTCODE));
        }
        public ActionResult SubmitTest()                                                        // nộp bài thi
        {
            if (!user.IsStudent())
                return View("Error");
            if (!user.IsTesting())
                return View("Error");
            var list = Model.GetListQuest(user.TESTCODE);
            int total_quest = list.First().test.total_questions;
            int test_code = list.First().test.test_code;
            double coefficient = 10.0 / (double)total_quest;                                    // số điểm 1 câu
            int count_correct = 0;                                                              // số câu đúng khởi tạo = 0
            foreach (var item in list)                                                          // với mỗi câu
            {
                // nếu đáp án đúng
                if (item.student_test.student_answer != null && item.student_test.student_answer.Trim().Equals(item.question.correct_answer.Trim()))
                    count_correct++;                                                            // tăng số câu đúng thêm 1
            }
            double score = coefficient * count_correct;                                         // điểm bằng điểm 1 câu nhân số câu đúng
            string detail = count_correct + "/" + total_quest;                                  // số câu đúng trên tổng số câu
            Model.InsertScore(score, detail);
            Model.FinishTest();
            return RedirectToAction("PreviewTest/" + test_code);
        }
        public ActionResult PreviewTest(int id)                                                 // xem bài đã thi
        {
            if (!user.IsStudent())
                return View("Error");
            if (user.IsTesting())
                return RedirectToAction("DoingTest");
            if(Model.GetStudentTestcode().IndexOf(id) == -1)                                    // nếu không có bài thi với id truyền vào trả về lỗi
                return View("Error");
            ViewBag.score = Model.GetScore(id);
            return View(Model.GetListQuest(id));
        } 
        [HttpPost]
        public void UpdateTiming(FormCollection form)                                           // cập nhật thời gian
        {
            // truyền vào thông tin
            string min = form["min"];
            string sec = form["sec"];
            string time = min + ":" + sec;
            Model.UpdateTiming(time);
        }
        [HttpPost]
        public void UpdateStudentTest(FormCollection form)                                      // cập nhật câu trả lời
        {
            // truyền vào thông tin
            int id_quest = Convert.ToInt32(form["id"]);
            string answer = form["answer"];
            answer = answer.Trim();                                                             // xóa khoảng trắng thừa
            string time = form["min"] + ":" + form["sec"];
            Model.UpdateStudentTest(id_quest, answer);
            Model.UpdateTiming(time);
        }
        public ActionResult Logout()                                                            // đăng xuất
        {
            if (!user.IsStudent())
                return View("Error");
            user.Reset();
            return RedirectToAction("Index", "Login");
        }
    }
}
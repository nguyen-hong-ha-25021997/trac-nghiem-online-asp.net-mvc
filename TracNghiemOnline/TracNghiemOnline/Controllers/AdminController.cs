using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TracNghiemOnline.Models;
using TracNghiemOnline.Common;
using System.Data;
using OfficeOpenXml;

namespace TracNghiemOnline.Controllers
{
    public class AdminController : Controller
    {
        User user = new User();                                                         // Tạo biến lưu thông tin đăng nhập
        // GET: Admin
        AdminDA Model = new AdminDA();                                                  // Tạo biến lưu Admin

        public ActionResult Index()
        {
            if (!user.IsAdmin())
                return View("Error");                                                   // Nếu không sẽ báo lỗi

            // Nếu đúng
            Model.UpdateLastLogin();
            Model.UpdateLastSeen("Trang Chủ", Url.Action("Index"));
            Dictionary<string, int> ListCount = Model.GetDashBoard();                   // Đếm sô lượng
            return View(ListCount);                                                     // Trả về giao diện số lượng đếm được
        }
        public ActionResult Logout()
        {
            if (!user.IsAdmin())
                return View("Error");
            user.Reset();
            return RedirectToAction("Index", "Login");                                  // Trở về trang login
        }
        public ActionResult AdminManager()
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Admin", Url.Action("AdminManager"));
            return View(Model.GetAdmins());
        }
        [HttpPost]
        public ActionResult AddAdmin(FormCollection form)                               // thêm admin
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Admin", Url.Action("AddAdmin"));
            // truyền dữ liệu vào
            string name = form["name"];
            string username = form["username"];
            string password = form["password"];
            string email = form["email"];
            string gender = form["gender"];
            string birthday = form["birthday"];
            bool add = Model.AddAdmin(name, username, password, gender, email, birthday);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("AdminManager");                                        // trở về trang quản lý admin
        }
        public ActionResult DeleteAdmin(string id)                                          // xóa admin
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Admin", Url.Action("DeleteAdmin"));
            int id_admin = Convert.ToInt32(id);                                             // chuyển đổi id truyền vào thành int
            bool del = Model.DeleteAdmin(id_admin);                                         // xóa admin với id đã truyền vào
            if (del)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Xóa Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Xóa Thất Bại";
            }
            return RedirectToAction("AdminManager");
        }
        [HttpPost]
        public ActionResult DeleteAdmin(FormCollection form)
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Admin", Url.Action("DeleteAdmin"));
            string[] ids = Regex.Split(form["checkbox"], ",");
            TempData["status_id"] = true;
            TempData["status"] = "Xóa Thất Bại ID: ";
            foreach (string id in ids)
            {
                int id_admin = Convert.ToInt32(id);
                bool del = Model.DeleteAdmin(id_admin);
                if (!del)
                {
                    TempData["status_id"] = false;
                    TempData["status"] += id_admin.ToString() + ",";
                }
            }
            if ((bool)TempData["status_id"])
            {
                TempData["status"] = "Xóa Thành Công";
            }
            return RedirectToAction("AdminManager");
        }
        public ActionResult EditAdmin(string id)                                            // sửa thông tin admin(lấy thông tin cũ)
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_admin = Convert.ToInt32(id);
            try
            {
                admin admin = Model.GetAdmin(id_admin);
                Model.UpdateLastSeen("Sửa Admin " + admin.name, Url.Action("EditAdmin/" + id));
                return View(admin);                                                         // trả về View dữ liệu lấy được
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditAdmin(FormCollection form)                                  // thực thi sửa thông tin admin
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_admin = Convert.ToInt32(form["id_admin"]);
            string name = form["name"];
            string username = form["username"];
            string password = form["password"];
            string email = form["email"];
            string gender = form["gender"];
            string birthday = form["birthday"];
            bool edit = Model.EditAdmin(id_admin, name, username, password, gender, email, birthday);
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
            return RedirectToAction("EditAdmin/" + id_admin);                                 // trờ về View sửa thông tin admin
        }
        public ActionResult TeacherManager()
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Giáo Viên", Url.Action("TeacherManager"));
            ViewBag.ListSpecialities = Model.GetSpecialities();
            return View(Model.GetTeachers());
        }
        [HttpPost]
        public ActionResult AddTeacher(FormCollection form)
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Giảng Viên", Url.Action("AddTeacher"));
            string name = form["name"];
            string username = form["username"];
            string password = form["password"];
            string email = form["email"];
            string gender = form["gender"];
            string birthday = form["birthday"];
            int id_speciality = Convert.ToInt32(form["id_speciality"]);
            bool add = Model.AddTeacher(name, username, password, gender, email, birthday, id_speciality);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("TeacherManager");
        }
        public ActionResult DeleteTeacher(string id)                                    // xóa giáo viên
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Giảng Viên", Url.Action("DeleteTeacher"));
            int id_teacher = Convert.ToInt32(id);
            bool del = Model.DeleteTeacher(id_teacher);
            if (del)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Xóa Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Xóa Thất Bại";
            }
            return RedirectToAction("TeacherManager");
        }
        [HttpPost]
        public ActionResult DeleteTeacher(FormCollection form)
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Giảng Viên", Url.Action("DeleteTeacher"));
            string[] ids = Regex.Split(form["checkbox"], ",");
            TempData["status_id"] = true;
            TempData["status"] = "Xóa Thất Bại ID: ";
            foreach (string id in ids)
            {
                int id_teacher = Convert.ToInt32(id);
                bool del = Model.DeleteTeacher(id_teacher);
                if (!del)
                {
                    TempData["status_id"] = false;
                    TempData["status"] += id_teacher.ToString() + ",";
                }
            }
            if ((bool)TempData["status_id"])
            {
                TempData["status"] = "Xóa Thành Công";
            }
            return RedirectToAction("TeacherManager");
        }
        public ActionResult EditTeacher(string id)                                   // sửa thông tin giáo viên(lấy thông tin cũ)
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_teacher = Convert.ToInt32(id);
            try
            {
                teacher teacher = Model.GetTeacher(id_teacher);
                Model.UpdateLastSeen("Sửa Giảng Viên " + teacher.name, Url.Action("EditTeacher/" + id));
                ViewBag.ListSpecialities = Model.GetSpecialities();
                return View(teacher);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditTeacher(FormCollection form)                          // sửa thông tin giáo viên
        {
            if (!user.IsAdmin())
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
        public ActionResult StudentManager()                                        // Quản lý học sinh (lấy danh sách trả về view)
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Sinh Viên", Url.Action("StudentManager"));
            ViewBag.ListSpecialities = Model.GetSpecialities();
            ViewBag.ListClass = Model.GetClasses();
            return View(Model.GetStudents());
        }
        [HttpPost]
        public ActionResult AddStudent(FormCollection form)                         // thêm học sinh
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Sinh Viên", Url.Action("AddStudent"));
            string name = form["name"];
            string username = form["username"];
            string password = form["password"];
            string email = form["email"];
            string gender = form["gender"];
            string birthday = form["birthday"];
            int id_speciality = Convert.ToInt32(form["id_speciality"]);
            int id_class = Convert.ToInt32(form["id_class"]);
            bool add = Model.AddStudent(name, username, password, gender, email, birthday, id_speciality, id_class);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("StudentManager");
        }
        public ActionResult DeleteStudent(string id)                                    // xóa học sinh
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Sinh Viên", Url.Action("DeleteStudent"));
            int id_student = Convert.ToInt32(id);
            bool del = Model.DeleteStudent(id_student);
            if (del)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Xóa Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Xóa Thất Bại";
            }
            return RedirectToAction("StudentManager");
        }
        [HttpPost]
        public ActionResult DeleteStudent(FormCollection form)                          // xóa học sinh
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Sinh Viên", Url.Action("DeleteStudent"));
            string[] ids = Regex.Split(form["checkbox"], ",");
            TempData["status_id"] = true;
            TempData["status"] = "Xóa Thất Bại ID: ";
            foreach (string id in ids)
            {
                int id_student = Convert.ToInt32(id);
                bool del = Model.DeleteStudent(id_student);
                if (!del)
                {
                    TempData["status_id"] = false;
                    TempData["status"] += id_student.ToString() + ",";
                }
            }
            if ((bool)TempData["status_id"])
            {
                TempData["status"] = "Xóa Thành Công";
            }
            return RedirectToAction("StudentManager");
        }
        public ActionResult EditStudent(string id)                                      // sửa thông tin học sinh(lấy thông tin cũ0
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_student = Convert.ToInt32(id);
            try
            {
                student student = Model.GetStudent(id_student);
                Model.UpdateLastSeen("Sửa Sinh Viên " + student.name, Url.Action("EditStudent/" + id));
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
        public ActionResult EditStudent(FormCollection form)                            // sửa thông tin học sinh
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
        public ActionResult ClassManager()                                          // quản lý lớp
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Khóa/Lớp", Url.Action("ClassManager"));
            ViewBag.ListSpecialities = Model.GetSpecialities();
            ViewBag.ListGrades = Model.GetGrades();
            return View(Model.GetClassesJoin());
        }
        [HttpPost]
        public ActionResult AddGrade(FormCollection form)                           // thêm khóa
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Khóa", Url.Action("AddGrade"));
            string grade_name = form["grade_name"];
            bool add = Model.AddGrade(grade_name);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("ClassManager");
        }
        [HttpPost]
        public ActionResult AddClass(FormCollection form)                           // thêm lớp
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Khóa", Url.Action("AddGrade"));
            string class_name = form["class_name"];
            int id_speciality = Convert.ToInt32(form["id_speciality"]);
            int id_grade = Convert.ToInt32(form["id_grade"]);
            bool add = Model.AddClass(class_name, id_grade, id_speciality);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("ClassManager");
        }
        public ActionResult DeleteClass(string id)                                  // xóa lớp
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Lớp", Url.Action("DeleteClass"));
            int id_class = Convert.ToInt32(id);
            bool del = Model.DeleteClass(id_class);
            if (del)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Xóa Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Xóa Thất Bại";
            }
            return RedirectToAction("ClassManager");
        }
        [HttpPost]
        public ActionResult DeleteClass(FormCollection form)                        // xóa lớp
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Lớp", Url.Action("DeleteClass"));
            string[] ids = Regex.Split(form["checkbox"], ",");
            TempData["status_id"] = true;
            TempData["status"] = "Xóa Thất Bại ID: ";
            foreach (string id in ids)
            {
                int id_class = Convert.ToInt32(id);
                bool del = Model.DeleteClass(id_class);
                if (!del)
                {
                    TempData["status_id"] = false;
                    TempData["status"] += id_class.ToString() + ",";
                }
            }
            if ((bool)TempData["status_id"])
            {
                TempData["status"] = "Xóa Thành Công";
            }
            return RedirectToAction("ClassManager");
        }
        public ActionResult EditClass(string id)                                    // xóa lớp
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_class = Convert.ToInt32(id);
            try
            {
                @class cl = Model.GetClass(id_class);
                Model.UpdateLastSeen("Sửa Lớp " + cl.class_name, Url.Action("EditClass/" + id));
                ViewBag.ListSpecialities = Model.GetSpecialities();
                ViewBag.ListGrades = Model.GetGrades();
                return View(cl);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditClass(FormCollection form)                          // sửa thông tin lớp
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_class = Convert.ToInt32(form["id_class"]);
            string class_name = form["class_name"];
            int id_speciality = Convert.ToInt32(form["id_speciality"]);
            int id_grade = Convert.ToInt32(form["id_grade"]);
            bool edit = Model.EditClass(id_class, class_name, id_speciality, id_grade);
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
            return RedirectToAction("EditClass/" + id_class);
        }
        public ActionResult SpecialityManager()                                     // quản lý ngành
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Ngành", Url.Action("SpecialityManager"));
            return View(Model.GetSpecialities());
        }
        [HttpPost]
        public ActionResult AddSpeciality(FormCollection form)                      // thêm ngành
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Ngành", Url.Action("AddSpeciality"));
            string speciality_name = form["speciality_name"];
            bool add = Model.AddSpeciality(speciality_name);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("SpecialityManager");
        }
        public ActionResult DeleteSpeciality(string id)                             // xóa ngành
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Ngành", Url.Action("DeleteSpeciality"));
            int id_speciality = Convert.ToInt32(id);
            bool del = Model.DeleteSpeciality(id_speciality);
            if (del)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Xóa Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Xóa Thất Bại";
            }
            return RedirectToAction("SpecialityManager");
        }
        [HttpPost]
        public ActionResult DeleteSpeciality(FormCollection form)                   // xóa ngành
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Ngành", Url.Action("DeleteSpeciality"));
            string[] ids = Regex.Split(form["checkbox"], ",");
            TempData["status_id"] = true;
            TempData["status"] = "Xóa Thất Bại ID: ";
            foreach (string id in ids)
            {
                int id_speciality = Convert.ToInt32(id);
                bool del = Model.DeleteSpeciality(id_speciality);
                if (!del)
                {
                    TempData["status_id"] = false;
                    TempData["status"] += id_speciality.ToString() + ",";
                }
            }
            if ((bool)TempData["status_id"])
            {
                TempData["status"] = "Xóa Thành Công";
            }
            return RedirectToAction("SpecialityManager");
        }
        public ActionResult EditSpeciality(string id)                               // sửa thông tin ngành(lấy thông tin cũ)
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_speciality = Convert.ToInt32(id);
            try
            {
                speciality speciality = Model.GetSpeciality(id_speciality);
                Model.UpdateLastSeen("Sửa Ngành " + speciality.speciality_name, Url.Action("EditSpeciality/" + id));
                return View(speciality);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditSpeciality(FormCollection form)                     // sửa thông tin ngành
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_speciality = Convert.ToInt32(form["id_speciality"]);
            string speciality_name = form["speciality_name"];
            bool edit = Model.EditSpeciality(id_speciality, speciality_name);
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
            return RedirectToAction("EditSpeciality/" + id_speciality);
        }
        public ActionResult SubjectManager()                                        // quản lý môn
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Môn", Url.Action("SubjectManager"));
            return View(Model.GetSubjects());
        }
        [HttpPost]
        public ActionResult AddSubject(FormCollection form)                         // thêm môn
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Môn", Url.Action("AddSubject"));
            string subject_name = form["subject_name"];
            bool add = Model.AddSubject(subject_name);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("SubjectManager");
        }
        public ActionResult DeleteSubject(string id)                                // xóa môn
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Môn", Url.Action("DeleteSubject"));
            int id_subject = Convert.ToInt32(id);
            bool del = Model.DeleteSubject(id_subject);
            if (del)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Xóa Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Xóa Thất Bại";
            }
            return RedirectToAction("SubjectManager");
        }
        [HttpPost]
        public ActionResult DeleteSubject(FormCollection form)                      // xóa môn
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Môn", Url.Action("DeleteSubject"));
            string[] ids = Regex.Split(form["checkbox"], ",");
            TempData["status_id"] = true;
            TempData["status"] = "Xóa Thất Bại ID: ";
            foreach (string id in ids)
            {
                int id_subject = Convert.ToInt32(id);
                bool del = Model.DeleteSubject(id_subject);
                if (!del)
                {
                    TempData["status_id"] = false;
                    TempData["status"] += id_subject.ToString() + ",";
                }
            }
            if ((bool)TempData["status_id"])
            {
                TempData["status"] = "Xóa Thành Công";
            }
            return RedirectToAction("SubjectManager");
        }
        public ActionResult EditSubject(string id)                                  // sửa thông tin môn(lấy thông tin cũ)
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_subject = Convert.ToInt32(id);
            try
            {
                subject subject = Model.GetSubject(id_subject);
                Model.UpdateLastSeen("Sửa Môn " + subject.subject_name, Url.Action("EditSubject/" + id));
                return View(subject);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditSubject(FormCollection form)                        // sửa thông tin môn
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_subject = Convert.ToInt32(form["id_subject"]);
            string subject_name = form["subject_name"];
            bool edit = Model.EditSubject(id_subject, subject_name);
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
            return RedirectToAction("EditSubject/" + id_subject);
        }
        public ActionResult QuestionManager()                                   // quản lý câu hỏi
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Câu Hỏi", Url.Action("QuestionManager"));
            ViewBag.ListSubject = Model.GetSubjects();
            return View(Model.GetQuestions());
        }
        [HttpPost]
        public ActionResult AddQuestion(FormCollection form, HttpPostedFileBase File)           // thêm câu hỏi
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Câu Hỏi", Url.Action("AddQuestion"));
            int id_subject = Convert.ToInt32(form["id_subject"]);
            int unit = Convert.ToInt32(form["unit"]);
            // truyền vào dữ liệu đáp án
            string content = form["content"];
            string[] answer = new string[] {
                form["answer_a"],
                form["answer_b"],
                form["answer_c"],
                form["answer_d"]
            };
            answer = Common.ShuffleArray.Randomize(answer);
            string answer_a = answer[0];
            string answer_b = answer[1];
            string answer_c = answer[2];
            string answer_d = answer[3];
            // truyền vào hình ảnh
            string correct_answer = form["correct_answer"];
            string img_content = "noimage.png";

            try
            {
                string fileName = Path.GetFileName(File.FileName);                                  // lấy đường dẫn file
                //Upload image
                string path = Server.MapPath("~/Assets/img_questions/");
                //Đuối hỗ trợ
                var allowedExtensions = new[] { ".png", ".jpg" };
                //Lấy phần mở rộng của file
                string extensionName = Path.GetExtension(File.FileName).ToLower();
                //Kiểm tra đuôi file
                if (!allowedExtensions.Contains(extensionName))
                {
                    TempData["status_id"] = false;
                    TempData["status"] = "Chỉ chọn file ảnh đuôi .PNG .png .JPG .jpg";
                    return RedirectToAction("QuestionManager");
                }
                else
                {
                    // Tạo tên file ngẫu nhiên
                    img_content = DateTime.Now.Ticks.ToString() + extensionName;
                    // Upload file lên server
                    File.SaveAs(path + img_content);
                }

            }
            catch (Exception) { }
            bool add = Model.AddQuestion(id_subject, unit, content, img_content, answer_a, answer_b, answer_c, answer_d, correct_answer);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("QuestionManager");
        }
        public ActionResult DeleteQuestion(string id)                               // xóa câu hỏi
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Câu Hỏi", Url.Action("DeleteQuestion"));
            int id_question = Convert.ToInt32(id);
            bool del = Model.DeleteQuestion(id_question);
            if (del)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Xóa Thành Công";
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Xóa Thất Bại";
            }
            return RedirectToAction("QuestionManager");
        }
        [HttpPost]
        public ActionResult DeleteQuestion(FormCollection form)                     // xóa câu hỏi
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Xóa Câu Hỏi", Url.Action("DeleteQuestion"));
            string[] ids = Regex.Split(form["checkbox"], ",");
            TempData["status_id"] = true;
            TempData["status"] = "Xóa Thất Bại ID: ";
            foreach (string id in ids)
            {
                int id_question = Convert.ToInt32(id);
                bool del = Model.DeleteQuestion(id_question);
                if (!del)
                {
                    TempData["status_id"] = false;
                    TempData["status"] += id_question.ToString() + ",";
                }
            }
            if ((bool)TempData["status_id"])
            {
                TempData["status"] = "Xóa Thành Công";
            }
            return RedirectToAction("QuestionManager");
        }
        public ActionResult EditQuestion(string id)                                 // sửa câu hỏi( lấy thông tin cũ)
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_question = Convert.ToInt32(id);
            try
            {
                question question = Model.GetQuestion(id_question);
                Model.UpdateLastSeen("Sửa Câu Hỏi " + question.id_question, Url.Action("EditQuestion/" + id));
                ViewBag.ListSubject = Model.GetSubjects();
                return View(question);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditQuestion(FormCollection form, HttpPostedFileBase File)      // sửa câu hỏi
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_question = Convert.ToInt32(form["id_question"]);
            int id_subject = Convert.ToInt32(form["id_subject"]);
            int unit = Convert.ToInt32(form["unit"]);
            string content = form["content"];
            string[] answer = new string[] {
                form["answer_a"],
                form["answer_b"],
                form["answer_c"],
                form["answer_d"]
            };
            //Không cần đảo thứ tự đáp án trong phần sửa
            //answer = Common.ShuffleArray.Randomize(answer);
            string answer_a = answer[0];
            string answer_b = answer[1];
            string answer_c = answer[2];
            string answer_d = answer[3];
            string correct_answer = form["correct_answer"];
            string img_content = "noimage.png";

            try
            {
                string fileName = Path.GetFileName(File.FileName);
                //Upload image
                string path = Server.MapPath("~/Assets/img_questions/");
                //Đuối hỗ trợ
                var allowedExtensions = new[] { ".png", ".jpg" };
                //Lấy phần mở rộng của file
                string extensionName = Path.GetExtension(File.FileName).ToLower();
                //Kiểm tra đuôi file
                if (!allowedExtensions.Contains(extensionName))
                {
                    TempData["status_id"] = false;
                    TempData["status"] = "Chỉ chọn file ảnh đuôi .PNG .png .JPG .jpg";
                    return RedirectToAction("QuestionManager");
                }
                else
                {
                    // Tạo tên file ngẫu nhiên
                    img_content = DateTime.Now.Ticks.ToString() + extensionName;
                    // Upload file lên server
                    File.SaveAs(path + img_content);
                }

            }
            catch (Exception) { }
            bool edit = Model.EditQuestion(id_question, id_subject, unit, content, img_content, answer_a, answer_b, answer_c, answer_d, correct_answer);
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
            return RedirectToAction("EditQuestion/" + id_question);
        }
        public ActionResult TestManager()                                                   // quản lý bài thi
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Bài Thi", Url.Action("TestManager"));
            ViewBag.ListSubject = Model.GetSubjects();
            return View(Model.Tests());
        }
        public JsonResult GetJsonUnits(int id)
        {
            return Json(Model.GetUnits(id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddTest(FormCollection form)
        {
            if (!user.IsAdmin())
                return View("Error");
            Model.UpdateLastSeen("Thêm Đề Thi", Url.Action("AddTest"));
            //tạo đề thi
            string test_name = form["test_name"];
            string password = Common.Encryptor.MD5Hash(form["password"]);                               // mã hóa mật khẩu
            //sinh số test code ngẫu nhiên
            Random rnd = new Random();
            int test_code = rnd.Next(111111, 999999);                                                    // tạo mã thi
            int id_subject = Convert.ToInt32(form["id_subject"]);
            int total_question = Convert.ToInt32(form["total_question"]);
            int time_to_do = Convert.ToInt32(form["time_to_do"]);
            string note = "";
            if (form["note"] != "")
                note = form["note"];
            bool add = Model.AddTest(test_name, password, test_code, id_subject, total_question, time_to_do, note);
            if (add)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Thêm Thành Công";
                //tạo bộ câu hỏi cho đề thi
                List<UnitViewModel> list_unit = Model.GetUnits(id_subject);
                foreach (UnitViewModel unit in list_unit)
                {
                    int quest_of_unit = Convert.ToInt32(form["unit-" + unit.Unit]);
                    List<question> list_question = Model.GetQuestionsByUnit(id_subject, unit.Unit, quest_of_unit);
                    foreach (question item in list_question)
                    {
                        Model.AddQuestionsToTest(test_code, item.id_question);
                    }
                }
            }
            else
            {
                TempData["status_id"] = false;
                TempData["status"] = "Thêm Thất Bại";
            }
            return RedirectToAction("TestManager");
        }
        public ActionResult EditTest(string id)                                         // sửa bài thi(lấy dữ liệu cũ)
        {
            if (!user.IsAdmin())
                return View("Error");
            int test_code = Convert.ToInt32(id);
            try
            {
                test test = Model.GetTest(test_code);
                Model.UpdateLastSeen("Sửa Đề Thi " + test.test_code, Url.Action("EditTest/" + id));
                return View(test);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult EditTest(FormCollection form)                               // sửa bài thi
        {
            if (!user.IsAdmin())
                return View("Error");
            int test_code = Convert.ToInt32(form["test_code"]);
            string test_name = form["test_name"];
            string password = "";
            if (form["password"] != "")
                password = Common.Encryptor.MD5Hash(form["password"]);
            int id_subject = Convert.ToInt32(form["id_subject"]);
            int time_to_do = Convert.ToInt32(form["time_to_do"]);
            string note = "";
            if (form["note"] != "")
                note = form["note"];
            bool edit = Model.EditTest(test_code, test_name, password, time_to_do, note);
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
            return RedirectToAction("EditTest/" + test_code);
        }
        public ActionResult ToggleStatus(int id)                                // sửa trạng thái mở hay đóng của bài thi
        {
            if (!user.IsAdmin())
                return View("Error");
            int id_test = Convert.ToInt32(id);
            bool toggle = Model.ToggleStatus(id_test);
            if (toggle)
            {
                TempData["status_id"] = true;
                TempData["status"] = "Đã Thay Đổi Trạng Thái Đề Thi " + id_test;
            }
            return RedirectToAction("TestManager/" + id_test);
        }
        public ActionResult TestDetail(string id)                               // lấy thông tin bài thi
        {
            if (!user.IsAdmin())
                return View("Error");
            int test_code = Convert.ToInt32(id);
            try
            {
                Model.UpdateLastSeen("Chi Tiết Đề Thi " + test_code, Url.Action("TestDetail/" + test_code));
                ViewBag.test_code = test_code;
                return View(Model.GetQuestionsOfTest(test_code));
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        
        public ActionResult Upload(FormCollection formCollection)
        {
            var questionList = new List<question>();                                // tạo 1 danh sách có phần tử có kiểu question
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // cấp phép để sử dụng
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        // tạo 1 trang tính Excel
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)    // tạo 1 vòng lặp để lưu dữ liệu từng dòng của Excel
                        {
                            var question = new question();                  //tạo 1 biến có kiểu question
                            // lấy dữ liệu từ excel lưu vào biến vừa tạo(1 biến lưu 1 dữ liệu của 1 dòng Excel)
                            question.id_question = Convert.ToInt32(workSheet.Cells[rowIterator, 1].Value);
                            question.id_subject = Convert.ToInt32(workSheet.Cells[rowIterator, 2].Value);
                            question.unit = Convert.ToInt32(workSheet.Cells[rowIterator, 3].Value);
                            question.img_content = workSheet.Cells[rowIterator, 4].Value.ToString();
                            question.content = workSheet.Cells[rowIterator, 5].Value.ToString();
                            question.answer_a = workSheet.Cells[rowIterator, 6].Value.ToString();
                            question.answer_b = workSheet.Cells[rowIterator, 7].Value.ToString();
                            question.answer_c = workSheet.Cells[rowIterator, 8].Value.ToString();
                            question.answer_d = workSheet.Cells[rowIterator, 9].Value.ToString();
                            question.correct_answer = workSheet.Cells[rowIterator, 10].Value.ToString();
                            questionList.Add(question);                                 // lưu biến vào danh sách đã tạo ở trên
                        }
                    }
                }
            }
            using (trac_nghiem_onlineEntities trac_nghiem_onlineEntities = new trac_nghiem_onlineEntities())   // gọi đến CSDL
            {
                foreach (var item in questionList)                      // tạo vào lặp để xử lý dữ liệu trong danh sách
                {
                    trac_nghiem_onlineEntities.questions.Add(item);     // thêm phần tử của danh sách vào bảng
                }
                trac_nghiem_onlineEntities.SaveChanges();               // lưu thay đổi
            }
            return RedirectToAction("QuestionManager");                 // trả về View quản lý câu hỏi
        }
    }
}
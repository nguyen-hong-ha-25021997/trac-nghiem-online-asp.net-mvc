using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
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

        public ActionResult TestManager()
        {
            if (!user.IsTeacher())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Bài Thi", Url.Action("TestManager"));
            ViewBag.ListSubject = Model.GetSubjects();
            return View(Model.GetListTest());
        }
        public JsonResult GetJsonUnits(int id)
        {
            return Json(Model.GetUnits(id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddTest(FormCollection form)
        {
            if (!user.IsTeacher())
                return View("Error");
            Model.UpdateLastSeen("Thêm Đề Thi", Url.Action("AddTest"));
            //tạo đề thi
            string test_name = form["test_name"];
            string password = Common.Encryptor.MD5Hash(form["password"]);                               // mã hóa mật khẩu
            //sinh số test code ngẫu nhiên
            Random rnd = new Random();
            int test_code = rnd.Next(111111,999999);                                                    // tạo mã thi
            int id_subject = Convert.ToInt32(form["id_subject"]);
            int total_question = Convert.ToInt32(form["total_question"]);
            int time_to_do = Convert.ToInt32(form["time_to_do"]);
            string note = "";
            if (form["note"]!="")
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
                        Model.AddQuestionsToTest(test_code,item.id_question);
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
        public ActionResult EditTest(string id)
        {
            if (!user.IsTeacher())
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
        public ActionResult EditTest(FormCollection form)
        {
            if (!user.IsTeacher())
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
        public ActionResult ToggleStatus(int id)
        {
            if (!user.IsTeacher())
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
        public ActionResult TestDetail(string id)                               // lấy thông tin vài thi
        {
            if (!user.IsTeacher())
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
        public ActionResult QuestionManager()
        {
            if (!user.IsTeacher())
                return View("Error");
            Model.UpdateLastSeen("Quản Lý Câu Hỏi", Url.Action("QuestionManager"));
            ViewBag.ListSubject = Model.GetSubjects();
            return View(Model.GetQuestions());
        }
        [HttpPost]
        public ActionResult AddQuestion(FormCollection form, HttpPostedFileBase File)
        {
            if (!user.IsTeacher())
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
        public ActionResult DeleteQuestion(string id)
        {
            if (!user.IsTeacher())
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
        public ActionResult DeleteQuestion(FormCollection form)
        {
            if (!user.IsTeacher())
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
        public ActionResult EditQuestion(string id)
        {
            if (!user.IsTeacher())
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
        public ActionResult EditQuestion(FormCollection form, HttpPostedFileBase File)
        {
            if (!user.IsTeacher())
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
        //[HttpPost]
        //public FileResult ExportToExcel(FormCollection form)                        // xuất điểm ra file Excel
        //{
        //    int test_code = Convert.ToInt32(form["test_code"]);                     // lấy mã bài test từ form
        //    DataTable dt = new DataTable("Grid");                                   // tạo datatable để lưu dữ liệu
        //    dt.Columns.AddRange(new DataColumn[4] { new DataColumn("Tài khoản"),    // thêm các cột vào datatable
        //                                    new DataColumn("Tên"),
        //                                    new DataColumn("Lớp"),
        //                                    new DataColumn("Điểm")});
        //    var scores = Model.GetListScore(test_code);                             // lấy dữ liệu về điểm với bài thi đc chọn
        //    // với mỗi bải ghi score của scores add dữ liệu đó vào datatable
        //    foreach (var score in scores)
        //    {
        //        dt.Rows.Add(score.student.username, score.student.name, score.student.@class.class_name, score.score.score_number);
        //    }
        //    // xuất dữ liệu trong datatable ra file excel
        //    using (XLWorkbook wb = new XLWorkbook())
        //    {
        //        wb.Worksheets.Add(dt);
        //        using (MemoryStream stream = new MemoryStream())
        //        {
        //            wb.SaveAs(stream);
        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.xlsx");
        //        }
        //    }
        //}

        [HttpPost]
        public ActionResult ExportToExcel(int id)                           // xuất điểm ra file Excel
        {
            var scores = Model.GetListScore(id);                            // lấy dữ liệu về điểm với bài thi đc chọn
            // Excel
            try
            {
                DataTable Dt = new DataTable();                             // tạo DataTable để lưu dữ liệu
                // thêm các cột cho DataTable
                Dt.Columns.Add("Tài khoản", typeof(string));
                Dt.Columns.Add("Tên", typeof(string));
                Dt.Columns.Add("Lớp", typeof(string));
                Dt.Columns.Add("Điểm", typeof(string));
                foreach(var data in scores)                                 // với mỗi bản ghi cso trong danh sách điểm
                {
                    // lưu dữ liệu bản ghi đó vào 1 dòng
                    DataRow row = Dt.NewRow();
                    row[0] = data.student.username;
                    row[1] = data.student.name;
                    row[2] = data.student.@class.class_name;
                    row[3] = data.score.score_number;
                    Dt.Rows.Add(row);                                       // thêm dòng có dữ liệu vào DataTable
                }
                var memoryStream = new MemoryStream();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // cấp phép để sử dụng
                using (var excelPackage = new ExcelPackage(memoryStream))
                {
                    var worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1"].LoadFromDataTable(Dt, true, TableStyles.None);
                    worksheet.Cells["A1:AN1"].Style.Font.Bold = true;
                    worksheet.DefaultRowHeight = 18;


                    worksheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    worksheet.Column(6).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Column(7).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.DefaultColWidth = 20;
                    worksheet.Column(2).AutoFit();
                    // lưu trang excel thành session để gọi cho phương thức khác
                    Session["DownloadExcel_FileManager"] = excelPackage.GetAsByteArray();
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult Download()
        {

            if (Session["DownloadExcel_FileManager"] != null)
            {
                byte[] data = Session["DownloadExcel_FileManager"] as byte[];
                return File(data, "application/octet-stream", "Điểm.xlsx");
            }
            else
            {
                return new EmptyResult();
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
            using ( trac_nghiem_onlineEntities trac_nghiem_onlineEntities = new trac_nghiem_onlineEntities())   // gọi đến CSDL
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
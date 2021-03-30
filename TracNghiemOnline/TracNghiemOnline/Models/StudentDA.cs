using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TracNghiemOnline.Common;
namespace TracNghiemOnline.Models
{
    public class StudentDA
    {
        User user = new User();
        trac_nghiem_onlineEntities db = new trac_nghiem_onlineEntities();

        public void UpdateLastLogin()
        {
            var update = (from x in db.students where x.id_student == user.ID select x).Single();
            update.last_login = DateTime.Now;
            db.SaveChanges();
        }
        public void UpdateLastSeen(string name, string url)
        {
            var update = (from x in db.students where x.id_student == user.ID select x).Single();
            update.last_seen = name;
            update.last_seen_url = url;
            db.SaveChanges();
        }
        public List<TestViewModel> GetDashboard()
        {
            List<TestViewModel> tests = (from x in db.tests
                                         join s in db.subjects on x.id_subject equals s.id_subject
                                         join stt in db.statuses on x.id_status equals stt.id_status
                                         select new TestViewModel { test = x, subject = s, status = stt }).ToList();
            return tests;
        }
        public student GetStudent(int id)                                           // lấy thông tin học sinh dc chọn trong CSDL
        {
            student student = new student();
            try
            {
                student = db.students.SingleOrDefault(x => x.id_student == id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return student;
        }
        public List<speciality> GetSpecialities()                                   // lấy danh sách ngành
        {
            return db.specialities.ToList();
        }
        public List<@class> GetClasses()                                            // lấy danh sách lớp
        {
            return db.classes.ToList();
        }
        // sửa thông tin học sinh trong CSDL
        public bool EditStudent(int id_student, string name, string username, string password, string gender, string email, string birthday, int id_speciality, int id_class)
        {
            try
            {
                var update = (from x in db.students where x.id_student == id_student select x).Single();
                update.name = name;
                update.username = username;
                update.email = email;
                update.gender = gender;
                update.id_speciality = id_speciality;
                update.id_class = id_class;
                update.birthday = Convert.ToDateTime(birthday);
                if (password != null)
                    update.password = Common.Encryptor.MD5Hash(password);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
        public test GetTest(int test_code)                                                  // lấy bài thi
        {
            test test = new test();
            try
            {
                test = db.tests.SingleOrDefault(x => x.test_code == test_code);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return test;
        }

        // cập nhật trạng thái đang thi và thời gian còn lại
        public void UpdateStatus(int test_code, string time_remaining)
        {
            var update = (from x in db.students where x.id_student == user.ID select x).Single();
            update.is_testing = test_code;
            update.time_start = DateTime.Now;
            update.time_remaining = time_remaining;
            db.SaveChanges();
            HttpContext.Current.Session[UserSession.TESTCODE] = test_code;
            user.TESTCODE = test_code;
            HttpContext.Current.Session[UserSession.TIME] = time_remaining;
            user.TIME = time_remaining;
        }

        public void CreateStudentQuestion(int code)                                        // tạo 1 câu hỏi cho học sinh
        {
            List<quests_of_test> qs = (from x in db.quests_of_test                         // lấy các ra câu hỏi của bài thi theo mã bài thi
                                       where x.test_code == code
                                       select x).OrderBy(x => Guid.NewGuid()).ToList();
            foreach(var item in qs)                                                        // với mỗi câu hỏi truyền vào ai làm và đáp án
            {
                // truyền vào các thông tin
                var StudentTest = new student_test_detail();
                StudentTest.id_question = item.id_question;                                // mã câu hỏi
                StudentTest.test_code = code;                                              // mã bài thi
                StudentTest.id_student = user.ID;                                          // mã học sinh làm
                question q = db.questions.SingleOrDefault(x => x.id_question == item.id_question);
                // truyền vào các lựa chọn của đáp án
                string[] answer = {q.answer_a, q.answer_b, q.answer_c, q.answer_d};
                answer = ShuffleArray.Randomize(answer);
                StudentTest.answer_a = answer[0];
                StudentTest.answer_b = answer[1];
                StudentTest.answer_c = answer[2];
                StudentTest.answer_d = answer[3];
                db.student_test_detail.Add(StudentTest);
                db.SaveChanges();
            }

        }
        public List<StudentQuestViewModel> GetListQuest(int test_code)                      // lấy danh sách câu hỏi của bài thi đc chọn
        {
            List<StudentQuestViewModel> list = new List<StudentQuestViewModel>();
            try
            {
                list = (from x in db.student_test_detail
                        join t in db.tests on x.test_code equals t.test_code
                        join q in db.questions on x.id_question equals q.id_question
                        where x.test_code == test_code &&
                        x.id_student == user.ID
                        select new StudentQuestViewModel { test = t, student_test = x, question = q }).OrderBy(x => x.student_test.ID).ToList();
            } catch(Exception) { }
            return list;
        }
        public void UpdateTiming(string time)                                           // cập nhật thời gian (để lấy thời gian online)
        {
            var update = (from x in db.students where x.id_student == user.ID select x).Single();
            update.time_remaining = time;
            HttpContext.Current.Session[UserSession.TIME] = time;
            user.TIME = time;
            db.SaveChanges();
        }
        public void UpdateStudentTest(int id_question, string answer)                   // cập nhật câu trả lời
        {
            var update = (from x in db.student_test_detail where x.id_student == user.ID && x.test_code == user.TESTCODE && x.id_question == id_question select x).Single();
            update.student_answer = answer;
            db.SaveChanges();
        }
        public void InsertScore(double score, string detail)                                            // lưu điểm
        {
            var s = new score();
            s.id_student = user.ID;
            s.test_code = user.TESTCODE;
            s.score_number = score;
            s.detail = detail;
            s.time_finish = DateTime.Now;
            db.scores.Add(s);                                                                           // thêm bản ghi vào CSDL
            db.SaveChanges();                                                                           // lưu
        }
        public void FinishTest()                                                 // sau khi kết thúc bài thi làm mới lại thông tin
        {
            var update = (from x in db.students where x.id_student == user.ID select x).Single();
            update.is_testing = null;
            update.time_remaining = null;
            update.time_start = null;
            db.SaveChanges();
            HttpContext.Current.Session[UserSession.TESTCODE] = 0;
            user.TESTCODE = 0;
            HttpContext.Current.Session[UserSession.TIME] = null;
            user.TIME = null;
        }
        public score GetScore(int test_code)                                        // lấy điểm của bài thi đc chọn
        {
            score score = new score();
            try
            {
                score = db.scores.SingleOrDefault(x => x.test_code == test_code && x.id_student == user.ID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return score;
        }
        public List<int> GetStudentTestcode()                                                       // lấy danh sách các bài thi
        {
            List<int> score = new List<int>();
            try
            {
                score = (from x in db.scores where x.id_student == user.ID select x.test_code).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return score;
        }
    }
}
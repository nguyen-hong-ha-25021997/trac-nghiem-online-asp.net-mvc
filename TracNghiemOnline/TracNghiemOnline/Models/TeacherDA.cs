using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TracNghiemOnline.Common;
namespace TracNghiemOnline.Models
{
    public class TeacherDA
    {
        User user = new User();
        trac_nghiem_onlineEntities db = new trac_nghiem_onlineEntities();

        public void UpdateLastLogin()
        {
            var update = (from x in db.teachers where x.id_teacher == user.ID select x).Single();
            update.last_login = DateTime.Now;
            db.SaveChanges();
        }
        public teacher GetTeacher(int id)
        {
            teacher teacher = new teacher();
            try
            {
                teacher = db.teachers.SingleOrDefault(x => x.id_teacher == id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return teacher;
        }
        public bool EditTeacher(int id_teacher, string name, string username, string password, string gender, string email, string birthday, int id_speciality)
        {
            try
            {
                var update = (from x in db.teachers where x.id_teacher == id_teacher select x).Single();
                update.name = name;
                update.username = username;
                update.email = email;
                update.gender = gender;
                update.id_speciality = id_speciality;
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
        public List<speciality> GetSpecialities()
        {
            return db.specialities.ToList();
        }
        public void UpdateLastSeen(string name, string url)
        {
            var update = (from x in db.teachers where x.id_teacher == user.ID select x).Single();
            update.last_seen = name;
            update.last_seen_url = url;
            db.SaveChanges();
        }
        public List<subject> GetSubjects()                                          // lấy danh sách môn học
        {
            return db.subjects.ToList();
        }
 
        public List<TestViewModel> GetListTest()                                    // lấy danh sách bài thi
        {
            List<TestViewModel> tests = (from x in db.tests
                                         join s in db.subjects on x.id_subject equals s.id_subject
                                         join stt in db.statuses on x.id_status equals stt.id_status
                                         select new TestViewModel { test = x, subject = s, status = stt }).ToList();
            return tests;
        }
        public List<UnitViewModel> GetUnits(int id)                                 // lấy các chương theo môn được chọn
        {
            List<UnitViewModel> tests = db.questions
                   .Where(p => p.id_subject == id)
                   .GroupBy(p => p.unit)
                   .Select(g => new UnitViewModel { Unit = g.Key, Total = g.Count() }).ToList();
            return tests;
        }
        // thêm bài thi trong CSDL
        public bool AddTest(string test_name, string password, int test_code, int id_subject, int total_question, int time_to_do, string note)
        {
            var test = new test();
            test.test_name = test_name;
            test.password = password;
            test.test_code = test_code;
            test.id_subject = id_subject;
            test.id_status = 2;
            test.total_questions = total_question;
            test.time_to_do = time_to_do;
            test.note = note;
            try
            {
                db.tests.Add(test);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
                return false;
            }
            return true;
        }
        public List<question> GetQuestionsByUnit(int id_subject, int unit, int quest_of_unit)           // lấy câu hỏi theo chương đc chọn
        {
            List<question> q = (from x in db.questions
                                where x.id_subject == id_subject && x.unit == unit
                                select x).OrderBy(x => Guid.NewGuid()).Take(quest_of_unit).ToList();
            return q;
        }
        public bool AddQuestionsToTest(int test_code, int id_question)                          // thêm câu hỏi cho bài thi
        {
            var quest_of_test = new quests_of_test();
            quest_of_test.test_code = test_code;
            quest_of_test.id_question = id_question;
            try
            {
                db.quests_of_test.Add(quest_of_test);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
                return false;
            }
            return true;
        }
        // sửa trạng thái mở hay đóng của bài thi
        public bool ToggleStatus(int id_test)
        {
            try
            {
                var update = (from x in db.tests where x.test_code == id_test select x).Single();
                if (update.id_status == 1)
                    update.id_status = 2;
                else
                    update.id_status = 1;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
        public test GetTest(int test_code)
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
        // sửa bài thi trong CSDL
        public bool EditTest(int test_code, string test_name, string password, int time_to_do, string note)
        {
            try
            {
                var update = (from x in db.tests where x.test_code == test_code select x).Single();
                update.test_name = test_name;
                update.time_to_do = time_to_do;
                update.note = note;
                if (password != "")
                    update.password = password;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
        // lấy câu hỏi của bài thi được chọn
        public List<question> GetQuestionsOfTest(int test_code)
        {
            List<int> id_quest = (from x in db.quests_of_test
                                  where x.test_code == test_code
                                  select x.id_question).ToList();
            List<question> list_quest = new List<question>();
            foreach (int item in id_quest)
            {
                question q = (from x in db.questions
                              where x.id_question == item
                              select x).Single();
                list_quest.Add(q);
            }
            return list_quest;
        }

        public List<QuestionViewModel> GetQuestions()                           // lấy danh sách câu hỏi theo môn đc chọn
        {
            List<QuestionViewModel> questions = (from x in db.questions
                                                 join s in db.subjects on x.id_subject equals s.id_subject
                                                 select new QuestionViewModel { question = x, subject = s }).ToList();
            return questions;
        }

        // thêm câu hỏi vào CSDL
        public bool AddQuestion(int id_subject, int unit, string content, string img_content, string answer_a, string answer_b, string answer_c, string answer_d, string correct_answer)
        {
            var question = new question();
            question.id_subject = id_subject;
            question.unit = unit;
            question.content = content;
            question.img_content = img_content;
            question.answer_a = answer_a;
            question.answer_b = answer_b;
            question.answer_c = answer_c;
            question.answer_d = answer_d;
            question.correct_answer = correct_answer;
            try
            {
                db.questions.Add(question);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        public bool DeleteQuestion(int id)                                      // xóa câu hỏi
        {
            try
            {
                var delete = (from x in db.questions where x.id_question == id select x).Single();
                db.questions.Remove(delete);                                    // xóa
                db.SaveChanges();                                               // lưu thay đổi
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
        public question GetQuestion(int id)                                         // lấy câu hỏi dc chọn
        {
            question question = new question();
            try
            {
                question = db.questions.SingleOrDefault(x => x.id_question == id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return question;
        }

        // sửa câu hỏi trong CSDL
        public bool EditQuestion(int id_question, int id_subject, int unit, string content, string img_content, string answer_a, string answer_b, string answer_c, string answer_d, string correct_answer)
        {
            try
            {
                var update = (from x in db.questions where x.id_question == id_question select x).Single();
                update.id_subject = id_subject;
                update.unit = unit;
                update.content = content;
                update.img_content = img_content;
                update.answer_a = answer_a;
                update.answer_b = answer_b;
                update.answer_c = answer_c;
                update.answer_d = answer_d;
                update.correct_answer = correct_answer;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
        public List<ScoreViewModel> GetListScore(int test_code)                         // lấy danh sách điểm
        {
            List<ScoreViewModel> score = new List<ScoreViewModel>();
            try
            {
                score = (from x in db.scores
                         join s in db.students on x.id_student equals s.id_student
                         where x.test_code == test_code select new ScoreViewModel { score = x, student = s }).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return score;
        }
    }
}
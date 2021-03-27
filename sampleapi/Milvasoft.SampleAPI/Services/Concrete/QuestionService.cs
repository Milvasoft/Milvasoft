using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs.MentorDTOs;
using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.DTOs.StudentDTOs;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Services.Abstract;
using Milvasoft.SampleAPI.Spec;
using Milvasoft.SampleAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Concrete
{

    /// <summary>
    /// Question service.
    /// </summary>
    public class QuestionService : IQuestionService
    {

        private readonly IBaseRepository<Question, Guid, EducationAppDbContext> _questionService;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="questionService"></param>
        public QuestionService(IBaseRepository<Question, Guid, EducationAppDbContext> questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Get all questions for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuestionDTO>> GetEntitiesForAdminAsync(QuestionSpec spec)
        {

            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var questions = await _questionService.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var questionsDTOList = from question in questions
                                   select new QuestionDTO
                                   {
                                       Title = question.Title,
                                       QuestionContent = question.QuestionContent,
                                       MentorReply = question.MentorReply,
                                       IsUseful = question.IsUseful,
                                       WillShown = question.WillShown,
                                       ProfessionId = question.ProfessionId,
                                       Mentor = question.Mentor.CheckObject(i => new MentorDTO
                                       {
                                           Id = (Guid)question.MentorId
                                       }),
                                       Student = question.Student.CheckObject(i => new StudentDTO
                                       {
                                           Id = i.Id
                                       }),
                                       CreationDate = question.CreationDate,
                                       CreatorUser = question.CreatorUser,
                                       LastModifierUser = question.LastModifierUser,
                                       Id = question.Id
                                   };

            return questionsDTOList.ToList();

        }

        /// <summary>
        /// Get all questions for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuestionDTO>> GetEntitiesForMentorAsync(QuestionSpec spec)
        {

            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);


            var questions = await _questionService.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var questionsDTOList = from question in questions
                                   select new QuestionDTO
                                   {
                                       Title = question.Title,
                                       QuestionContent = question.QuestionContent,
                                       MentorReply = question.MentorReply,
                                       IsUseful = question.IsUseful,
                                       WillShown = question.WillShown,
                                       ProfessionId = question.ProfessionId,
                                       Mentor = question.Mentor.CheckObject(i => new MentorDTO
                                       {
                                           Id = (Guid)question.MentorId
                                       }),
                                       Student = question.Student.CheckObject(i => new StudentDTO
                                       {
                                           Id = i.Id
                                       }),
                                       CreationDate = question.CreationDate
                                   };

            return questionsDTOList.ToList();

        }

        /// <summary>
        /// Get all questions for student.
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuestionDTO>> GetEntitiesForStudentAsync(QuestionSpec spec)
        {

            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                    .Include(st => st.Student);

            var questions = await _questionService.GetAllAsync(includes, spec?.ToExpression()).ConfigureAwait(false);

            var questionsDTOList = from question in questions
                                   select new QuestionDTO
                                   {
                                       Title = question.Title,
                                       QuestionContent = question.QuestionContent,
                                       MentorReply = question.MentorReply,
                                       ProfessionId = question.ProfessionId,
                                       Mentor = question.Mentor.CheckObject(i => new MentorDTO
                                       {
                                           Id = (Guid)question.MentorId
                                       }),
                                       Student = question.Student.CheckObject(i => new StudentDTO
                                       {
                                           Id = i.Id
                                       })
                                   };

            return questionsDTOList.ToList();

        }

        /// <summary>
        /// Get student for admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<QuestionDTO> GetEntityForAdminAsync(Guid id)
        {

            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var question = await _questionService.GetByIdAsync(id, includes).ConfigureAwait(false);

            return new QuestionDTO
            {
                Title = question.Title,
                QuestionContent = question.QuestionContent,
                MentorReply = question.MentorReply,
                IsUseful = question.IsUseful,
                WillShown = question.WillShown,
                ProfessionId = question.ProfessionId,
                Mentor = question.Mentor.CheckObject(i => new MentorDTO
                {
                    Id = (Guid)question.MentorId
                }),
                Student = question.Student.CheckObject(i => new StudentDTO
                {
                    Id = i.Id
                }),
                CreationDate = question.CreationDate,
                CreatorUser = question.CreatorUser,
                LastModifierUser = question.LastModifierUser,
                Id = question.Id
            };

        }

        /// <summary>
        /// Get student for mentor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<QuestionDTO> GetEntityForMentorAsync(Guid id)
        {

            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var question = await _questionService.GetByIdAsync(id, includes).ConfigureAwait(false);

            return new QuestionDTO
            {
                Title = question.Title,
                QuestionContent = question.QuestionContent,
                MentorReply = question.MentorReply,
                IsUseful = question.IsUseful,
                WillShown = question.WillShown,
                ProfessionId = question.ProfessionId,
                Mentor = question.Mentor.CheckObject(i => new MentorDTO
                {
                    Id = (Guid)question.MentorId
                }),
                Student = question.Student.CheckObject(i => new StudentDTO
                {
                    Id = i.Id
                }),
                CreationDate = question.CreationDate
            };

        }

        /// <summary>
        /// Get student for student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<QuestionDTO> GetEntityForStudentAsync(Guid id)
        {

            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                    .Include(st => st.Student);

            var question = await _questionService.GetByIdAsync(id).ConfigureAwait(false);

            return new QuestionDTO
            {
                Title = question.Title,
                QuestionContent = question.QuestionContent,
                MentorReply = question.MentorReply,
                ProfessionId = question.ProfessionId,
                Mentor = question.Mentor.CheckObject(i => new MentorDTO
                {
                    Id = (Guid)question.MentorId
                }),
                Student = question.Student.CheckObject(i => new StudentDTO
                {
                    Id = i.Id
                })
            };

        }

        /// <summary>
        /// Add student.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddQuestionDTO educationDTO)
        {
            var question = new Question
            {
                Title = educationDTO.Title,
                QuestionContent = educationDTO.QuestionContent,
                CreationDate = DateTime.Now,
                CreatorUserId = educationDTO.StudentId
            };
            await _questionService.AddAsync(question).ConfigureAwait(false);
        }

        /// <summary>
        /// Update student.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateQuestionDTO educationDTO)
        {

            var updatedQuestion = await _questionService.GetByIdAsync(educationDTO.Id).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete students.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> ids)
        {

            var deletedQuestions = await _questionService.GetAllAsync(i => ids.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _questionService.DeleteAsync(deletedQuestions).ConfigureAwait(false);

        }

        /// <summary>
        /// Delete student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid id)
        {

            var deletedQuestion = await _questionService.GetByIdAsync(id).ConfigureAwait(false);

            await _questionService.DeleteAsync(deletedQuestion).ConfigureAwait(false);

        }

        /// <summary>
        /// If questions to show.
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuestionDTO>> GetWillShowQuestions()
        {

            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                    .Include(st => st.Student);

            var questions = await _questionService.GetAllAsync(i => i.WillShown).ConfigureAwait(false);

            var questionDTOList = questions != null ? from question in questions
                                                      select new QuestionDTO
                                                      {
                                                          Title = question.Title,
                                                          QuestionContent = question.QuestionContent,
                                                          MentorReply = question.MentorReply,
                                                          ProfessionId = question.ProfessionId,
                                                          Mentor = question.Mentor.CheckObject(i => new MentorDTO
                                                          {
                                                              Id = (Guid)question.MentorId
                                                          }),
                                                          Student = question.Student.CheckObject(i => new StudentDTO
                                                          {
                                                              Id = i.Id
                                                          })
                                                      } : null;
            return questionDTOList.ToList();
        }
    }
}

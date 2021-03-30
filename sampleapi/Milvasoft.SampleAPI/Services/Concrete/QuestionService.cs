﻿using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Models;
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
        public async Task<PaginationDTO<QuestionDTO>> GetEntitiesForAdminAsync(int pageIndex,
                                                                               int requestedItemCount,
                                                                               string orderByProperty = null,
                                                                               bool orderByAscending = false, 
                                                                               QuestionSpec questionSpec=null)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var (questions, pageCount, totalDataCount) = await _questionService.PreparePaginationDTO<IBaseRepository<Question, Guid, EducationAppDbContext>, Question, Guid>
                                                                                                                (pageIndex, requestedItemCount, orderByProperty, orderByAscending, questionSpec?.ToExpression(),includes).ConfigureAwait(false);

            return new PaginationDTO<QuestionDTO>
            {
                DTOList = questions.CheckList(i => questions.Select(question=> new QuestionDTO
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
                    Id = question.Id
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get all questions for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<QuestionDTO>> GetEntitiesForMentorAsync(int pageIndex,
                                                                                int requestedItemCount,
                                                                                string orderByProperty = null,
                                                                                bool orderByAscending = false,
                                                                                QuestionSpec questionSpec = null)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);


            var (questions, pageCount, totalDataCount) = await _questionService.PreparePaginationDTO<IBaseRepository<Question, Guid, EducationAppDbContext>, Question, Guid>
                                                                                                                 (pageIndex, requestedItemCount, orderByProperty, orderByAscending, questionSpec?.ToExpression(),includes).ConfigureAwait(false);

            return new PaginationDTO<QuestionDTO>
            {
                DTOList = questions.CheckList(i => questions.Select(question => new QuestionDTO
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
                    })
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get all questions for student.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<QuestionDTO>> GetEntitiesForStudentAsync(int pageIndex,
                                                                                 int requestedItemCount,
                                                                                 string orderByProperty = null,
                                                                                 bool orderByAscending = false,
                                                                                 QuestionSpec questionSpec = null)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                    .Include(st => st.Student);

            var (questions, pageCount, totalDataCount) = await _questionService.PreparePaginationDTO<IBaseRepository<Question, Guid, EducationAppDbContext>, Question, Guid>
                                                                                                                 (pageIndex, requestedItemCount, orderByProperty, orderByAscending, questionSpec?.ToExpression(),includes).ConfigureAwait(false);

            return new PaginationDTO<QuestionDTO>
            {
                DTOList = questions.CheckList(i => questions.Select(question => new QuestionDTO
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
                })),
                PageCount = pageCount,
                TotalDataCount = totalDataCount
            };
        }

        /// <summary>
        /// Get student for admin.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public async Task<QuestionDTO> GetEntityForAdminAsync(Guid questionId)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var question = await _questionService.GetByIdAsync(questionId, includes).ConfigureAwait(false);

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
        /// <param name="questionId"></param>
        /// <returns></returns>
        public async Task<QuestionDTO> GetEntityForMentorAsync(Guid questionId)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var question = await _questionService.GetByIdAsync(questionId, includes).ConfigureAwait(false);

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
        /// <param name="questionId"></param>
        /// <returns></returns>
        public async Task<QuestionDTO> GetEntityForStudentAsync(Guid questionId)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                    .Include(st => st.Student);

            var question = await _questionService.GetByIdAsync(questionId).ConfigureAwait(false);

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
        /// <param name="addQuestionDTO"></param>
        /// <returns></returns>
        public async Task AddEntityAsync(AddQuestionDTO addQuestionDTO)
        {
            var question = new Question
            {
                Title = addQuestionDTO.Title,
                QuestionContent = addQuestionDTO.QuestionContent,
            };
            await _questionService.AddAsync(question).ConfigureAwait(false);
        }

        /// <summary>
        /// Update student.
        /// </summary>
        /// <param name="updateQuestionDTO"></param>
        /// <returns></returns>
        public async Task UpdateEntityAsync(UpdateQuestionDTO updateQuestionDTO)
        {
            var updatedQuestion = await _questionService.GetByIdAsync(updateQuestionDTO.Id).ConfigureAwait(false);

            updatedQuestion.IsUseful = updateQuestionDTO.IsUseful;
            updatedQuestion.MentorReply = updateQuestionDTO.MentorReply;
            updatedQuestion.ProfessionId = updateQuestionDTO.ProfessionId;
            updatedQuestion.QuestionContent = updateQuestionDTO.QuestionContent;
            updatedQuestion.WillShown = updateQuestionDTO.WillShown;

            await _questionService.UpdateAsync(updatedQuestion).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete students.
        /// </summary>
        /// <param name="questionIds"></param>
        /// <returns></returns>
        public async Task DeleteEntitiesAsync(List<Guid> questionIds)
        {
            var deletedQuestions = await _questionService.GetAllAsync(i => questionIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _questionService.DeleteAsync(deletedQuestions).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete student.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public async Task DeleteEntityAsync(Guid questionId)
        {
            var deletedQuestion = await _questionService.GetByIdAsync(questionId).ConfigureAwait(false);

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

            return (questions != null ? from question in questions
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
                                        } : null).ToList();
        }
    }
}
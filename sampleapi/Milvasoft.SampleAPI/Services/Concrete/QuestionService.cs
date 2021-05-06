using Milvasoft.Helpers.DataAccess.Abstract;
using Milvasoft.Helpers.DataAccess.IncludeLibrary;
using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.Data;
using Milvasoft.SampleAPI.DTOs;
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

        private readonly IBaseRepository<Question, Guid, EducationAppDbContext> _questionRepository;

        /// <summary>
        /// Performs constructor injection for repository interfaces used in this service.
        /// </summary>
        /// <param name="questionRepository"></param>
        public QuestionService(IBaseRepository<Question, Guid, EducationAppDbContext> questionRepository)
        {
            _questionRepository = questionRepository;
        }

        #region CRUD Operations

        /// <summary>
        /// Get all questions for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<QuestionForAdminDTO>> GetQuestionsForAdminAsync(PaginationParamsWithSpec<QuestionSpec> pagiantionParams)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var (questions, pageCount, totalDataCount) = await _questionRepository.PreparePaginationDTO<Question, Guid>(pagiantionParams.PageIndex,
                                                                                                                        pagiantionParams.RequestedItemCount,
                                                                                                                        pagiantionParams.OrderByProperty,
                                                                                                                        pagiantionParams.OrderByAscending,
                                                                                                                        pagiantionParams.Spec?.ToExpression(),
                                                                                                                        includes).ConfigureAwait(false);


            return new PaginationDTO<QuestionForAdminDTO>
            {
                DTOList = questions.CheckList(i => questions.Select(question => new QuestionForAdminDTO
                {
                    Id = question.Id,
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
        /// Get all questions for admin.
        /// </summary>
        /// <returns></returns>
        public async Task<PaginationDTO<QuestionForMentorDTO>> GetQuestionsForMentorAsync(PaginationParamsWithSpec<QuestionSpec> pagiantionParams)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);


            var (questions, pageCount, totalDataCount) = await _questionRepository.PreparePaginationDTO<Question, Guid>(pagiantionParams.PageIndex,
                                                                                                                        pagiantionParams.RequestedItemCount,
                                                                                                                        pagiantionParams.OrderByProperty,
                                                                                                                        pagiantionParams.OrderByAscending,
                                                                                                                        pagiantionParams.Spec?.ToExpression(),
                                                                                                                        includes).ConfigureAwait(false);

            return new PaginationDTO<QuestionForMentorDTO>
            {
                DTOList = questions.CheckList(i => questions.Select(question => new QuestionForMentorDTO
                {
                    Id = question.Id,
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
        public async Task<PaginationDTO<QuestionForStudentDTO>> GetQuestionsForStudentAsync(PaginationParamsWithSpec<QuestionSpec> pagiantionParams)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                    .Include(st => st.Student);

            var (questions, pageCount, totalDataCount) = await _questionRepository.PreparePaginationDTO<Question, Guid>(pagiantionParams.PageIndex,
                                                                                                                        pagiantionParams.RequestedItemCount,
                                                                                                                        pagiantionParams.OrderByProperty,
                                                                                                                        pagiantionParams.OrderByAscending,
                                                                                                                        pagiantionParams.Spec?.ToExpression(),
                                                                                                                        includes).ConfigureAwait(false);

            return new PaginationDTO<QuestionForStudentDTO>
            {
                DTOList = questions.CheckList(i => questions.Select(question => new QuestionForStudentDTO
                {
                    Id = question.Id,
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
        /// Get question for admin by <paramref name="questionId"/>.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public async Task<QuestionForAdminDTO> GetQuestionForAdminAsync(Guid questionId)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var question = await _questionRepository.GetByIdAsync(questionId, includes).ConfigureAwait(false);

            question.ThrowIfNullForGuidObject();

            return new QuestionForAdminDTO
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
        /// Get question for mentor by <paramref name="questionId"/>.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public async Task<QuestionForMentorDTO> GetQuestionForMentorAsync(Guid questionId)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                     .Include(st => st.Student);

            var question = await _questionRepository.GetByIdAsync(questionId, includes).ConfigureAwait(false);

            question.ThrowIfNullForGuidObject();

            return new QuestionForMentorDTO
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
        /// Get question for student by <paramref name="questionId"/>.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public async Task<QuestionForStudentDTO> GetQuestionForStudentAsync(Guid questionId)
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                    .Include(st => st.Student);

            var question = await _questionRepository.GetByIdAsync(questionId).ConfigureAwait(false);

            question.ThrowIfNullForGuidObject();

            return new QuestionForStudentDTO
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
        /// Maps <paramref name="addQuestionDTO"/> to <c><b>Question</b></c>  object and adds that product to repository.
        /// </summary>
        /// <param name="addQuestionDTO">Question to be added.</param>
        /// <returns></returns>
        public async Task AddQuestionAsync(AddQuestionDTO addQuestionDTO)
        {
            var question = new Question
            {
                Title = addQuestionDTO.Title,
                QuestionContent = addQuestionDTO.QuestionContent,
            };
            await _questionRepository.AddAsync(question).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates single question which that equals <paramref name="updateQuestionDTO"/> in repository by <paramref name="updateQuestionDTO"/>'s properties.
        /// </summary>
        /// <param name="updateQuestionDTO">Question to be updated.</param>
        /// <returns></returns>
        public async Task UpdateQuestionAsync(UpdateQuestionDTO updateQuestionDTO)
        {
            var toBeUpdatedQuestion = await _questionRepository.GetByIdAsync(updateQuestionDTO.Id).ConfigureAwait(false);

            toBeUpdatedQuestion.IsUseful = updateQuestionDTO.IsUseful;
            toBeUpdatedQuestion.MentorReply = updateQuestionDTO.MentorReply;
            toBeUpdatedQuestion.ProfessionId = updateQuestionDTO.ProfessionId;
            toBeUpdatedQuestion.QuestionContent = updateQuestionDTO.QuestionContent;
            toBeUpdatedQuestion.WillShown = updateQuestionDTO.WillShown;

            await _questionRepository.UpdateAsync(toBeUpdatedQuestion).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete question by <paramref name="questionIds"/>.
        /// </summary>
        /// <param name="questionIds"></param>
        /// <returns></returns>
        public async Task DeleteQuestionsAsync(List<Guid> questionIds)
        {
            var deletedQuestions = await _questionRepository.GetAllAsync(i => questionIds.Select(p => p).Contains(i.Id)).ConfigureAwait(false);

            await _questionRepository.DeleteAsync(deletedQuestions).ConfigureAwait(false);
        }

        /// <summary>
        /// If questions to show.
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuestionDTO>> GetWillShowQuestions()
        {
            Func<IIncludable<Question>, IIncludable> includes = i => i.Include(md => md.Mentor)
                                                                    .Include(st => st.Student);

            var questions = await _questionRepository.GetAllAsync(i => i.WillShown).ConfigureAwait(false);

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

        #endregion
    }
}

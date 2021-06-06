using Milvasoft.Helpers.Models;
using Milvasoft.SampleAPI.DTOs;
using Milvasoft.SampleAPI.DTOs.QuestionDTOs;
using Milvasoft.SampleAPI.Spec;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Services.Abstract
{
    /// <summary>
    /// Question service inteface.
    /// </summary>
    public interface IQuestionService
    {
        /// <summary>
        /// Get all questions for student from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<QuestionForStudentDTO>> GetQuestionsForStudentAsync(PaginationParamsWithSpec<QuestionSpec> paginationParams);

        /// <summary>
        /// Get all questions for admin from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<QuestionForAdminDTO>> GetQuestionsForAdminAsync(PaginationParamsWithSpec<QuestionSpec> paginationParams);

        /// <summary>
        /// Get all questions for mentor from database.
        /// </summary>
        /// <returns></returns>
        Task<PaginationDTO<QuestionForMentorDTO>> GetQuestionsForMentorAsync(PaginationParamsWithSpec<QuestionSpec> paginationParams);

        /// <summary>
        /// Fetches filtered question by id for student.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<QuestionForStudentDTO> GetQuestionForStudentAsync(Guid id);

        /// <summary>
        /// Fetches filtered question by id for admin.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<QuestionForAdminDTO> GetQuestionForAdminAsync(Guid id);

        /// <summary>
        /// Fetches filtered question by id for mentor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<QuestionForMentorDTO> GetQuestionForMentorAsync(Guid id);

        /// <summary>
        /// Add question to database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task AddQuestionAsync(AddQuestionDTO educationDTO);

        /// <summary>
        /// Updates question in database.
        /// </summary>
        /// <param name="educationDTO"></param>
        /// <returns></returns>
        Task UpdateQuestionAsync(UpdateQuestionDTO educationDTO);

        /// <summary>
        /// Delete questions by <paramref name="ids"/>
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task DeleteQuestionsAsync(List<Guid> ids);

        /// <summary>
        ///  Returns questions to display
        /// </summary>
        /// <returns></returns>
        Task<List<QuestionDTO>> GetWillShowQuestionsAsync();

    }
}

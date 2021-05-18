using Milvasoft.Helpers.Extensions;
using Milvasoft.SampleAPI.Entity;
using Milvasoft.SampleAPI.Entity.Enum;
using Milvasoft.SampleAPI.Spec.Abstract;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Milvasoft.SampleAPI.Spec
{
    /// <summary>
    /// Filtering student object list.
    /// </summary>
    public class StudentSpec : IBaseSpec<Student>
    {
        #region Fields
        private string _name;
        private string _surname;
        private string _university;
        #endregion

        #region Props

        /// <summary>
        /// Student's name.
        /// </summary>
        [OValidateString(5000)]
        public string Name { get => _name; set => _name = value?.ToUpper(); }

        /// <summary>
        /// Student's surname.
        /// </summary>
        [OValidateString(5000)]
        public string Surname { get => _surname; set => _surname = value?.ToUpper(); }

        /// <summary>
        /// Student's university.
        /// </summary>
        [OValidateString(5000)]
        public string University { get => _university; set => _university = value?.ToUpper(); }

        /// <summary>
        /// Age of student.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Did the student sign the contract?
        /// </summary>
        public bool? IsConfidentialityAgreementSigned { get; set; }

        /// <summary>
        /// Education status of student.
        /// </summary>
        public EducationStatus? GraduationStatus { get; set; }


        /// <summary>
        /// Gradution score of student.
        /// </summary>
        public int? GraduationScore { get; set; }

        /// <summary>
        /// Profession id of student.
        /// </summary>
        [OValidateId]
        public Guid? ProfessionId { get; set; }

        /// <summary>
        /// Mentor id of student.
        /// </summary>
        [OValidateId]
        public Guid? MentorId { get; set; }

        #endregion

        /// <summary>
        /// Filtering question list by  with requested properties.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public List<Student> GetFilteredEntities(IEnumerable<Student> entities)
        {
            if (!Name.IsNullOrEmpty()) entities = entities.Where(m => m.Name.ToUpper().Contains(Name));
            if (!Surname.IsNullOrEmpty()) entities = entities.Where(m => m.Surname.ToUpper().Contains(Surname));
            if (!University.IsNullOrEmpty()) entities = entities.Where(m => m.University.ToUpper().Contains(University));

            if (Age.HasValue) entities.Where(m => m.Age == Age);
            if (IsConfidentialityAgreementSigned.HasValue) entities.Where(m => m.IsConfidentialityAgreementSigned == IsConfidentialityAgreementSigned);

            if (MentorId.HasValue) entities = entities.Where(i => i.MentorId == MentorId);
            if (ProfessionId.HasValue) entities = entities.Where(i => i.ProfessionId == ProfessionId);


            return entities.ToList();
        }

        /// <summary>
        /// Converts spesifications to expression.
        /// </summary>
        /// <returns></returns>
        public Expression<Func<Student, bool>> ToExpression()
        {
            Expression<Func<Student, bool>> mainPredicate = null;
            List<Expression<Func<Student, bool>>> predicates = new List<Expression<Func<Student, bool>>>();

            if (!string.IsNullOrEmpty(Name)) predicates.Add(c => c.Name == Name);
            if (!string.IsNullOrEmpty(Surname)) predicates.Add(c => c.Surname == Surname);
            if (!string.IsNullOrEmpty(University)) predicates.Add(c => c.University == University);

            if (Age.HasValue) predicates.Add(c => c.Age == Age);
            if (IsConfidentialityAgreementSigned.HasValue) predicates.Add(c => c.IsConfidentialityAgreementSigned == IsConfidentialityAgreementSigned);
            if (ProfessionId.HasValue) predicates.Add(c => c.ProfessionId == ProfessionId);
            if (GraduationStatus.HasValue) predicates.Add(c => c.GraduationStatus == GraduationStatus);
            if (MentorId.HasValue) predicates.Add(c => c.MentorId == MentorId);
            if (GraduationScore.HasValue) predicates.Add(c => c.GraduationScore == GraduationScore);

            predicates?.ForEach(predicate => mainPredicate = mainPredicate.Append(predicate, ExpressionType.AndAlso));
            return mainPredicate;
        }
    }
}

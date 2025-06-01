using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSyncWebApi.Data;
using EduSyncWebApi.Models;
using EduSyncWebApi.DTO;

namespace EduSyncWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssessmentModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AssessmentModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentModel>>> GetAssessmentModels()
        {
            return await _context.AssessmentModels.ToListAsync();
        }

        // GET: api/AssessmentModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentModel>> GetAssessmentModel(Guid id)
        {
            var assessmentModel = await _context.AssessmentModels.FindAsync(id);

            if (assessmentModel == null)
            {
                return NotFound();
            }

            return assessmentModel;
        }

        // PUT: api/AssessmentModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssessment(Guid id, AssessmentDTO assessmentModel)
        {
            if (id != assessmentModel.AssessmentId)
            {
                return BadRequest();
            }

            var existingAssessment = await _context.AssessmentModels.FindAsync(id);
            if (existingAssessment == null)
            {
                return NotFound();
            }

            existingAssessment.Title = assessmentModel.Title;
            existingAssessment.Questions = assessmentModel.Questions;
            existingAssessment.MaxScore = assessmentModel.MaxScore;
            existingAssessment.CourseId = assessmentModel.CourseId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssessmentModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AssessmentModels
        [HttpPost]
        public async Task<ActionResult<AssessmentModel>> PostAssessment(AssessmentDTO assessment)
        {
            var assessmentModel = new AssessmentModel
            {
                AssessmentId = assessment.AssessmentId,
                Title = assessment.Title,
                Questions = assessment.Questions,
                MaxScore = assessment.MaxScore,
                CourseId = assessment.CourseId
            };

            _context.AssessmentModels.Add(assessmentModel);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (AssessmentModelExists(assessmentModel.AssessmentId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAssessmentModel", new { id = assessmentModel.AssessmentId }, assessmentModel);
        }

        // DELETE: api/AssessmentModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessmentModel(Guid id)
        {
            var assessmentModel = await _context.AssessmentModels.FindAsync(id);
            if (assessmentModel == null)
            {
                return NotFound();
            }

            _context.AssessmentModels.Remove(assessmentModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssessmentModelExists(Guid id)
        {
            return _context.AssessmentModels.Any(e => e.AssessmentId == id);
        }
    }
}

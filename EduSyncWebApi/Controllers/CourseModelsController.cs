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
using Azure.Storage.Blobs;
using System.Text;
using System.Text.Json;

namespace EduSyncWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CourseModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CourseModels
        // GET: api/CourseModels?instructorId=123
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseModel>>> GetCoursesByInstructor([FromQuery] Guid? instructorId)
        {
            if (instructorId != null)
            {
                return await _context.CourseModels
                    .Where(c => c.InstructorId == instructorId)
                    .ToListAsync();
            }

            return await _context.CourseModels.ToListAsync();
        }


        // GET: api/CourseModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseModel>> GetCourseModel(Guid id)
        {
            var courseModel = await _context.CourseModels.FindAsync(id);

            if (courseModel == null)
            {
                return NotFound();
            }

            return courseModel;
        }

        // PUT: api/CourseModels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(Guid id, CourseDTO course)
        {
            if (id != course.CourseId)
            {
                return BadRequest();
            }

            CourseModel orignalCourse = new CourseModel()
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            };

            _context.Entry(orignalCourse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseModelExists(id))
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


        // POST: api/CourseModels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CourseModel>> PostCourse(CourseDTO courseModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //course.CourseId = Guid.NewGuid();
            CourseModel orignalCourse = new CourseModel()
            {
                CourseId = courseModel.CourseId,
                Title = courseModel.Title,
                Description = courseModel.Description,
                InstructorId = courseModel.InstructorId,
                MediaUrl = courseModel.MediaUrl
            };

            _context.CourseModels.Add(orignalCourse);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CourseModelExists(orignalCourse.CourseId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCourseModel", new { id = orignalCourse.CourseId }, orignalCourse);
        }





        // DELETE: api/CourseModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseModel(Guid id)
        {
            var courseModel = await _context.CourseModels.FindAsync(id);
            if (courseModel == null)
            {
                return NotFound();
            }

            _context.CourseModels.Remove(courseModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool CourseModelExists(Guid id)
        {
            return _context.CourseModels.Any(e => e.CourseId == id);
        }
    }
}


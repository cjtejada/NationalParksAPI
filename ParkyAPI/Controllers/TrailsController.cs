using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Dtos;
using ParkyAPI.Models;
using ParkyAPI.Repository;

namespace ParkyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ParkyOpenAPISpecTrails")]
    [ProducesResponseType(400)]
    public class TrailsController : ControllerBase
    {
        private readonly ITrailRepository _trailRepo;
        private readonly IMapper _mapper;

        public TrailsController(ITrailRepository trailRepo, IMapper mapper)
        {
            _trailRepo = trailRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Get a list of all trails.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<TrailDto>))]
        [ProducesResponseType(400)]
        public IActionResult GetTrails()
        {
            var objList = _trailRepo.GetTrails();

            //This will ensure we send our Dto instead of the model
            var objDto = new List<TrailDto>();
            foreach (var obj in objList)
                objDto.Add(_mapper.Map<TrailDto>(obj));

            return Ok(objDto);
        }

        /// <summary>
        /// Get individual trail
        /// </summary>
        /// <param name="trailId">The trail Id</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{trailId:int}", Name = "GetTrail")]
        [ProducesResponseType(200, Type = typeof(TrailDto))]
        [ProducesResponseType(404)]
        [ProducesDefaultResponseType]
        public IActionResult GetTrail(int trailId)
        {
            var obj = _trailRepo.GetTrail(trailId);

            //This will ensure we send our Dto instead of the model
            var objDto = new List<TrailDto>();

            if (obj == null)
                return NotFound();

            objDto.Add(_mapper.Map<TrailDto>(obj));

            return Ok(objDto);
        }

        [HttpGet("GetTrailsInNationalPark/{nationalParkId}")]
        [ProducesResponseType(200, Type = typeof(TrailDto))]
        [ProducesResponseType(404)]
        [ProducesDefaultResponseType]
        public IActionResult GetTrailInNationalPark(int nationalParkId)
        {
            var objList = _trailRepo.GetTrailsInNationalPark(nationalParkId);

            if (objList == null)
                return NotFound();

            var objDto = new List<TrailDto>();

            foreach (var obj in objList)
                objDto.Add(_mapper.Map<TrailDto>(obj));

            return Ok(objDto);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(TrailDto))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult CreateTrail([FromBody] TrailCreateDto trailDto)
        {
            //ModelState contains all related encountered errors
            if (trailDto == null)
                return BadRequest(ModelState);

            if (_trailRepo.TrailExists(trailDto.Name))
            {
                //Give the ModelState a message and an error code 404 to return to the client
                ModelState.AddModelError("", "Trail Exists!");
                return StatusCode(404, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Map parameter Dto to the Model
            var trailObj = _mapper.Map<Trail>(trailDto);

            //If creation was unsuccessful
            if (!_trailRepo.CreateTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when saving the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetTrail", new { trailId = trailObj.Id }, trailObj);
        }

        [HttpPatch("{trailId:int}", Name = "UpdateTrail")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult UpdateTrail(int trailId, [FromBody] TrailUpdateDto trailDto)
        {
            //ModelState contains all related encountered errors
            if (trailDto == null || trailId != trailDto.Id)
                return BadRequest(ModelState);

            var trailObj = _mapper.Map<Trail>(trailDto);

            //If update was unsuccessful
            if (!_trailRepo.UpdateTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when updating the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{trailId:int}", Name = "DeleteTrail")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult DeleteTrail(int trailId)
        {
            //ModelState contains all related encountered errors
            if (!_trailRepo.TrailExists(trailId))
                return NotFound();

            var trailObj = _trailRepo.GetTrail(trailId);

            //If update was unsuccessful
            if (!_trailRepo.DeleteTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when deleting the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}